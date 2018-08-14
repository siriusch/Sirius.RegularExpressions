using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Sirius.Collections;
using Sirius.RegularExpressions.Automata;

namespace Sirius.RegularExpressions {
	public class Lexer<TLetter> where TLetter: struct, IComparable<TLetter> {
		private readonly Dfa<TLetter> dfa;
		private LinkedFifoBuffer<TLetter> buffer;
		private LinkedFifoBuffer<TLetter>.BufferPosition bufferPosition;
		private Id<DfaState<TLetter>> state;
		private LinkedFifoBuffer<TLetter>.BufferPosition tokenEnd;
		private LinkedFifoBuffer<TLetter>.BufferPosition tokenStart;
		private SymbolId? tokenSymbol;

		public Lexer(Dfa<TLetter> dfa, SymbolId eof, Action<SymbolId, IEnumerable<TLetter>, long> tokenAction, params SymbolId[] symbolsToIgnore) {
			this.dfa = dfa ?? throw new ArgumentNullException(nameof(dfa));
			this.Eof = eof;
			this.TokenAction = tokenAction ?? ((s, t, o) => { });
			this.SymbolsToIgnore = new HashSet<SymbolId>(symbolsToIgnore ?? new SymbolId[0]);
		}

		protected SymbolId Eof {
			get;
		}

		protected IReadOnlyCollection<SymbolId> SymbolsToIgnore {
			get;
		}

		protected Action<SymbolId, IEnumerable<TLetter>, long> TokenAction {
			get;
		}

		private void AssertBuffer() {
			if (this.buffer == null) {
				this.buffer = new LinkedFifoBuffer<TLetter>();
				Reset(this.buffer.HeadPosition);
			}
		}

		private void FlushPendingToken() {
			Debug.Assert(this.tokenSymbol.HasValue);
			ProcessToken(this.tokenSymbol.Value, this.tokenStart.Take((int)(this.tokenEnd.Offset-this.tokenStart.Offset)), this.tokenStart.Offset, this.tokenEnd);
		}

		protected virtual void HandleLexicalError(bool flushing) {
			throw new InvalidOperationException("Lexical error at position "+this.bufferPosition.Offset);
		}

		private void ProcessData() {
			TLetter value;
			while (LinkedFifoBuffer<TLetter>.TryGetValue(ref this.bufferPosition, out value)) {
				var newState = this.dfa.GetState(this.state).GetTransition(value);
				if (Dfa<TLetter>.IsEndState(newState)) {
					if (this.tokenSymbol.HasValue) {
						var position = this.bufferPosition;
						FlushPendingToken();
						if (newState == Dfa<TLetter>.Accept) {
							this.tokenEnd = this.tokenStart = this.bufferPosition = position; // restore position after EOF letter
							ProcessEof();
						} else {
							continue;
						}
					} else {
						HandleLexicalError(false);
					}
				}
				SetState(newState);
			}
		}

		protected virtual void ProcessEof() {
			this.TokenAction(this.Eof, Enumerable.Empty<TLetter>(), this.bufferPosition.Offset);
		}

		protected virtual void ProcessToken(SymbolId symbol, IEnumerable<TLetter> data, long offset, LinkedFifoBuffer<TLetter>.BufferPosition tokenEnd) {
			if (!this.SymbolsToIgnore.Contains(symbol)) {
				this.TokenAction(symbol, data, offset);
			}
			Reset(tokenEnd);
		}

		public void Push(TLetter letter) {
			AssertBuffer();
			LinkedFifoBuffer<TLetter>.Write(ref this.buffer, letter);
			ProcessData();
		}

		public void Push(TLetter[] letters) {
			AssertBuffer();
			LinkedFifoBuffer<TLetter>.Write(ref this.buffer, letters);
			ProcessData();
		}

		public void Push(TLetter[] letters, int index, int count) {
			AssertBuffer();
			LinkedFifoBuffer<TLetter>.Write(ref this.buffer, letters, index, count);
			ProcessData();
		}

		protected void Reset(LinkedFifoBuffer<TLetter>.BufferPosition bufferPosition) {
			this.buffer = bufferPosition.Buffer;
			this.tokenStart = this.tokenEnd = this.bufferPosition = bufferPosition;
			this.tokenSymbol = null;
			this.state = this.dfa.StartState.Id;
		}

		private void SetState(Id<DfaState<TLetter>> state) {
			this.state = state;
			SymbolId symbol;
			if (this.dfa.SymbolStates.TryGetValue(this.state, out symbol)) {
				this.tokenEnd = this.bufferPosition;
				this.tokenSymbol = symbol;
			}
		}

		public void Terminate() {
			if (!this.dfa.Eof.HasValue) {
				if (this.buffer != null) {
					FlushPendingToken();
					if ((this.bufferPosition != this.tokenStart) || this.bufferPosition.HasData) {
						HandleLexicalError(true);
					}
				}
				ProcessEof();
				this.state = Dfa<TLetter>.Accept;
			} else if (this.state != Dfa<TLetter>.Accept) {
				HandleLexicalError(true);
			}
		}
	}
}
