using System;
using System.Collections.Generic;

namespace Sirius.RegularExpressions.Automata {
	public sealed class Nfa<TLetter>: INonFiniteAutomaton<TLetter>
			where TLetter: struct, IEquatable<TLetter>, IComparable<TLetter> {
		public Nfa(IReadOnlyDictionary<Id<NfaState<TLetter>>, NfaState<TLetter>> states, NfaState<TLetter> startState, NfaState<TLetter> endState) {
			this.States = states.Values;
			this.StartState = startState;
			this.EndState = endState;
		}

		public NfaState<TLetter> EndState {
			get;
		}

		public IEnumerable<NfaState<TLetter>> States {
			get;
		}

		public NfaState<TLetter> StartState {
			get;
		}
	}
}
