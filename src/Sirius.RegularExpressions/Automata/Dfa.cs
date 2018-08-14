using System;
using System.Collections.Generic;

namespace Sirius.RegularExpressions.Automata {
	public class Dfa<TLetter>
			where TLetter: struct, IComparable<TLetter> {
		public static readonly Id<DfaState<TLetter>> Accept = new Id<DfaState<TLetter>>(-1);
		public static readonly Id<DfaState<TLetter>> Reject = new Id<DfaState<TLetter>>(-2);

		public static bool IsEndState(Id<DfaState<TLetter>> id) {
			return id.ToInt32() < 0;
		}

		public Dfa(TLetter? eof, IReadOnlyList<DfaState<TLetter>> states, IReadOnlyDictionary<Id<DfaState<TLetter>>, SymbolId> symbolStates) {
			this.Eof = eof;
			this.States = states;
			this.SymbolStates = symbolStates;
		}

		public TLetter? Eof {
			get;
		}

		public IReadOnlyList<DfaState<TLetter>> States {
			get;
		}

		public IReadOnlyDictionary<Id<DfaState<TLetter>>, SymbolId> SymbolStates {
			get;
		}

		public DfaState<TLetter> StartState => this.States[0];

		public DfaState<TLetter> GetState(Id<DfaState<TLetter>> id) {
			return this.States[id.ToInt32()];
		}
	}
}
