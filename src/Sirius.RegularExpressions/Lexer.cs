using System;
using System.Collections.Generic;

using Sirius.Collections;
using Sirius.RegularExpressions.Automata;

namespace Sirius.RegularExpressions {
	public class Lexer<TLetter>: LexerBase<TLetter, TLetter> where TLetter: struct, IComparable<TLetter> {
		private readonly Dfa<TLetter> dfa;

		public Lexer(Dfa<TLetter> dfa, Action<SymbolId, Capture<TLetter>> tokenAction, params SymbolId[] symbolsToIgnore): base(dfa.StartState.Id, dfa.Eof.HasValue, SymbolId.Eof, tokenAction, symbolsToIgnore) {
			this.dfa = dfa;
		}

		protected override SymbolId? ProcessStateMachine(ref Id<DfaState<TLetter>> state, TLetter input) {
			state = this.dfa.GetState(state).GetTransition(input);
			return this.dfa.SymbolStates.TryGetValue(state, out var result) ? result : default(SymbolId?);
		}
	}

	public class Lexer<TInput, TLetter>: LexerBase<TInput, TLetter>
			where TInput : struct, IComparable<TInput>, IEquatable<TInput>
			where TLetter : struct, IComparable<TLetter>, IEquatable<TLetter> {
		private readonly DfaStateMachine<TLetter, TInput> stateMachine;

		public Lexer(DfaStateMachine<TLetter, TInput> stateMachine, Id<DfaState<TLetter>> startStateId, bool handleEof, Action<SymbolId, Capture<TInput>> tokenAction, params SymbolId[] symbolsToIgnore) : base(startStateId, handleEof, SymbolId.Eof, tokenAction, symbolsToIgnore) {
			this.stateMachine = stateMachine;
		}

		protected sealed override SymbolId? ProcessStateMachine(ref Id<DfaState<TLetter>> state, TInput input) {
			return this.stateMachine(ref state, input);
		}
	}
}
