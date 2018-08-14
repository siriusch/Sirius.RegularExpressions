using System;
using System.Collections.Generic;

namespace Sirius.RegularExpressions.Automata {
	public interface INonFiniteAutomaton<TLetter>
			where TLetter: struct, IEquatable<TLetter>, IComparable<TLetter> {
		IEnumerable<NfaState<TLetter>> States {
			get;
		}

		NfaState<TLetter> StartState {
			get;
		}
	}
}
