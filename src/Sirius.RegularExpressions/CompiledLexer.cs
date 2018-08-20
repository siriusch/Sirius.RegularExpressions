using System;
using System.Collections.Generic;

using Sirius.RegularExpressions.Automata;

namespace Sirius.RegularExpressions {
	public class CompiledLexer<TLetter>: LexerBase<TLetter> where TLetter: struct, IComparable<TLetter>, IEquatable<TLetter> {
		private readonly DfaStateMachine<TLetter> stateMachine;

		public CompiledLexer(DfaStateMachine<TLetter> stateMachine, Id<DfaState<TLetter>> startStateId, bool handleEof, SymbolId eof, Action<SymbolId, IEnumerable<TLetter>, long> tokenAction, params SymbolId[] symbolsToIgnore): base(startStateId, handleEof, eof, tokenAction, symbolsToIgnore) {
			this.stateMachine = stateMachine;
		}

		protected sealed override SymbolId? ProcessStateMachine(ref Id<DfaState<TLetter>> state, TLetter input) {
			return this.stateMachine(ref state, input);
		}
	}
}
