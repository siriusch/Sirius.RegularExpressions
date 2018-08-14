using System;
using System.Collections.Generic;
using System.ComponentModel;

using Sirius.Collections;

namespace Sirius.RegularExpressions.Automata {
	[DisplayName("DFA")]
	public class DfaState<TLetter>: IIdentifiable<DfaState<TLetter>>
			where TLetter: struct, IComparable<TLetter> {
		private readonly RangeDictionary<TLetter, Id<DfaState<TLetter>>> transitions = new RangeDictionary<TLetter, Id<DfaState<TLetter>>>();

		protected internal DfaState(Id<DfaState<TLetter>> id) {
			this.Id = id;
		}

		public IEnumerable<KeyValuePair<Range<TLetter>, Id<DfaState<TLetter>>>> Transitions => this.transitions;

		public Id<DfaState<TLetter>> Id {
			get;
		}

		public Id<DfaState<TLetter>> GetTransition(TLetter key) {
			return GetTransition(key, Dfa<TLetter>.Reject);
		}

		public Id<DfaState<TLetter>> GetTransition(TLetter key, Id<DfaState<TLetter>> defaultState) {
			Id<DfaState<TLetter>> result;
			if (this.transitions.TryGetValue(key, out result)) {
				return result;
			}
			return defaultState;
		}

		public void SetTransition(Range<TLetter> alpha, Id<DfaState<TLetter>> target) {
			this.transitions.Add(alpha, target);
		}
	}
}
