using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Sirius.Collections;
using Sirius.RegularExpressions.Automata;

namespace Sirius.RegularExpressions {
	public abstract class LexerBase<TInput, TLetter>
			where TInput: struct, IComparable<TInput>
			where TLetter: struct, IComparable<TLetter> {
		private readonly bool handleEof;
		private LinkedFifoBuffer<TInput> buffer;
		private LinkedFifoBuffer<TInput>.BufferPosition bufferPosition;
		private Id<DfaState<TLetter>> state;
		private LinkedFifoBuffer<TInput>.BufferPosition tokenEnd;
		private LinkedFifoBuffer<TInput>.BufferPosition tokenStart;
		private SymbolId? tokenSymbol;

		protected LexerBase(bool handleEof, SymbolId eof, Id<DfaState<TLetter>> startStateId) {
			this.handleEof = handleEof;
			this.StartStateId = startStateId;
			this.Eof = eof;
		}

		protected Id<DfaState<TLetter>> StartStateId {
			get;
		}

		protected Id<DfaState<TLetter>> CurrentStateId => this.state;

		protected SymbolId Eof {
			get;
		}

		protected abstract void TokenAction(SymbolId symbolId, Capture<TInput> value);

		private void AssertBuffer() {
			if (this.buffer == null) {
				this.buffer = new LinkedFifoBuffer<TInput>();
				this.Reset(this.buffer.HeadPosition);
			}
		}

		private void FlushPendingToken() {
			Debug.Assert(this.tokenSymbol.HasValue);
			this.TokenAction(this.tokenSymbol.Value, new Capture<TInput>(this.tokenStart, (int)(this.tokenEnd.Offset - this.tokenStart.Offset), this.tokenStart.Offset));
			this.Reset(this.tokenEnd);
		}

		protected virtual void HandleLexicalError(bool flushing) {
			throw new InvalidOperationException("Lexical error at position " + this.bufferPosition.Offset);
		}

		private void ProcessData() {
			TInput value;
			while (LinkedFifoBuffer<TInput>.TryGetValue(ref this.bufferPosition, out value)) {
				var newState = this.state;
				var newSymbol = this.ProcessStateMachine(ref newState, value);
				if (Dfa<TLetter>.IsEndState(newState)) {
					if (this.tokenSymbol.HasValue) {
						var position = this.bufferPosition;
						this.FlushPendingToken();
						if (newState == Dfa<TLetter>.Accept) {
							this.tokenEnd = this.tokenStart = this.bufferPosition = position; // restore position after EOF letter
							this.ProcessEof();
						} else {
							continue;
						}
					} else {
						this.HandleLexicalError(false);
					}
				}
				this.state = newState;
				if (newSymbol.HasValue) {
					this.tokenEnd = this.bufferPosition;
					this.tokenSymbol = newSymbol.Value;
				}
			}
		}

		protected virtual void ProcessEof() {
			this.TokenAction(this.Eof, new Capture<TInput>(Enumerable.Empty<TInput>(), 0, this.bufferPosition.Offset - 1));
		}

		protected abstract SymbolId? ProcessStateMachine(ref Id<DfaState<TLetter>> state, TInput input);

		public void Push(TInput letter) {
			this.AssertBuffer();
			LinkedFifoBuffer<TInput>.Write(ref this.buffer, letter);
			this.ProcessData();
		}

		public void Push(IEnumerable<TInput> letters) {
			this.AssertBuffer();
			foreach (var letter in letters) {
				LinkedFifoBuffer<TInput>.Write(ref this.buffer, letter);
			}
			this.ProcessData();
		}

		public void Push(TInput[] letters) {
			this.AssertBuffer();
			LinkedFifoBuffer<TInput>.Write(ref this.buffer, letters);
			this.ProcessData();
		}

		public void Push(TInput[] letters, int index, int count) {
			this.AssertBuffer();
			LinkedFifoBuffer<TInput>.Write(ref this.buffer, letters, index, count);
			this.ProcessData();
		}

		protected void Reset(LinkedFifoBuffer<TInput>.BufferPosition bufferPosition) {
			this.buffer = bufferPosition.Buffer;
			this.tokenStart = this.tokenEnd = this.bufferPosition = bufferPosition;
			this.tokenSymbol = null;
			this.state = this.StartStateId;
		}

		public void Terminate() {
			if (!this.handleEof) {
				if (this.buffer != null) {
					this.FlushPendingToken();
					if ((this.bufferPosition != this.tokenStart) || this.bufferPosition.HasData) {
						this.HandleLexicalError(true);
					}
				}
				this.ProcessEof();
				this.state = Dfa<TLetter>.Accept;
			} else if (this.state != Dfa<TLetter>.Accept) {
				this.HandleLexicalError(true);
			}
		}
	}
}
