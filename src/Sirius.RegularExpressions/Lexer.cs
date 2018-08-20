using System;
using System.Collections.Generic;

using Sirius.RegularExpressions.Automata;

namespace Sirius.RegularExpressions {
	public class Lexer<TLetter>: LexerBase<TLetter> where TLetter: struct, IComparable<TLetter> {
		private readonly Dfa<TLetter> dfa;

		public Lexer(Dfa<TLetter> dfa, SymbolId eof, Action<SymbolId, IEnumerable<TLetter>, long> tokenAction, params SymbolId[] symbolsToIgnore): base(dfa.StartState.Id, dfa.Eof.HasValue, eof, tokenAction, symbolsToIgnore) {
			this.dfa = dfa;
		}

		protected override SymbolId? ProcessStateMachine(ref Id<DfaState<TLetter>> state, TLetter input) {
			state = this.dfa.GetState(state).GetTransition(input);
			return this.dfa.SymbolStates.TryGetValue(state, out var result) ? result : default(SymbolId?);
		}
	}
}
