using System;

using Sirius.Collections;
using Sirius.RegularExpressions.Automata;

namespace Sirius.RegularExpressions {
	public class Lexer<TLetter>: LexerBase<TLetter, TLetter>
			where TLetter: struct, IComparable<TLetter> {
		private readonly Dfa<TLetter> dfa;
		private readonly Action<SymbolId, Capture<TLetter>> tokenAction;

		public Lexer(Dfa<TLetter> dfa, Action<SymbolId, Capture<TLetter>> tokenAction): base(dfa.Eof.HasValue, SymbolId.Eof, dfa.StartState.Id) {
			this.dfa = dfa;
			this.tokenAction = tokenAction;
		}

		protected override SymbolId? ProcessStateMachine(ref Id<DfaState<TLetter>> state, TLetter input) {
			state = this.dfa.GetState(state).GetTransition(input);
			return this.dfa.SymbolStates.TryGetValue(state, out var result) ? result : default(SymbolId?);
		}

		protected override void TokenAction(SymbolId symbolId, Capture<TLetter> value) {
			this.tokenAction?.Invoke(symbolId, value);
		}
	}

	public class Lexer<TInput, TLetter>: LexerBase<TInput, TLetter>
			where TInput: struct, IComparable<TInput>, IEquatable<TInput>
			where TLetter: struct, IComparable<TLetter>, IEquatable<TLetter> {
		private readonly DfaStateMachine<TLetter, TInput> stateMachine;
		private readonly Action<SymbolId, Capture<TInput>> tokenAction;

		public Lexer(DfaStateMachine<TLetter, TInput> stateMachine, Id<DfaState<TLetter>> startStateId, bool handleEof, Action<SymbolId, Capture<TInput>> tokenAction): base(handleEof, SymbolId.Eof, startStateId) {
			this.stateMachine = stateMachine;
			this.tokenAction = tokenAction;
		}

		protected sealed override SymbolId? ProcessStateMachine(ref Id<DfaState<TLetter>> state, TInput input) {
			return this.stateMachine(ref state, input);
		}

		protected override void TokenAction(SymbolId symbolId, Capture<TInput> value) {
			this.tokenAction?.Invoke(symbolId, value);
		}
	}
}
