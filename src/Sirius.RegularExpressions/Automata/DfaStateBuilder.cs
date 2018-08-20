using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Sirius.Collections;

namespace Sirius.RegularExpressions.Automata {
	internal sealed class DfaStateBuilder<TLetter>: IEquatable<DfaStateBuilder<TLetter>>
			where TLetter: struct, IEquatable<TLetter>, IComparable<TLetter> {
		private readonly RangeDictionary<TLetter, DfaStateBuilder<TLetter>> transitions = new RangeDictionary<TLetter, DfaStateBuilder<TLetter>>(ReferenceEqualityComparer<DfaStateBuilder<TLetter>>.Default);

		public DfaStateBuilder(int id, string key, NfaState<TLetter>[] states) {
			if (states == null) {
				throw new ArgumentNullException(nameof(states));
			}
			this.Id = id;
			this.NfaStates = states;
			this.Key = key;
		}

		public int Id {
			get;
		}

		public string Key {
			get;
		}

		public NfaState<TLetter>[] NfaStates {
			get;
		}

		public bool Equals(DfaStateBuilder<TLetter> other) {
			if (ReferenceEquals(null, other)) {
				return false;
			}
			if (ReferenceEquals(this, other)) {
				return true;
			}
			return this.NfaStates.Select(s => s.Id).SetEqual(other.NfaStates.Select(s => s.Id)) && this.transitions.Equals(other.transitions);
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			if (ReferenceEquals(this, obj)) {
				return true;
			}
			return this.Equals((DfaStateBuilder<TLetter>)obj);
		}

		public override int GetHashCode() {
			unchecked {
				return this.NfaStates.Aggregate(typeof(NfaState<TLetter>).GetHashCode() * 397, (hash, state) => hash^state.Id.GetHashCode());
			}
		}

		/// <summary>
		///     Gets the state transitions leading to other real states (no REJECT or ACCEPT).
		/// </summary>
		public IEnumerable<KeyValuePair<Range<TLetter>, DfaStateBuilder<TLetter>>> GetTransitions() {
			return this.transitions;
		}

		public void ReplaceTransition(Func<DfaStateBuilder<TLetter>, bool> predicate, DfaStateBuilder<TLetter> replacement) {
			for (var i = 0; i < this.transitions.Values.Count; i++) {
				if (predicate(this.transitions.Values[i])) {
					Debug.Assert(Equals(this.transitions.Values[i], replacement));
					this.transitions.Values[i] = replacement;
				}
			}
		}

		public void SetTransition(Range<TLetter> letter, DfaStateBuilder<TLetter> target) {
			this.transitions.Add(letter, target);
		}
	}
}
