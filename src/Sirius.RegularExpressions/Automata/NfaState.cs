using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Sirius.Collections;

namespace Sirius.RegularExpressions.Automata {
	[DisplayName("NFA")]
	public sealed class NfaState<TLetter>: IEquatable<NfaState<TLetter>>, IIdentifiable<NfaState<TLetter>>
			where TLetter: struct, IEquatable<TLetter>, IComparable<TLetter> {
		private readonly HashSet<NfaState<TLetter>> epsilonTransitions = new HashSet<NfaState<TLetter>>(IdComparer<NfaState<TLetter>>.Default);
		private readonly RangeDictionary<TLetter, NfaState<TLetter>> matchTransitions = new RangeDictionary<TLetter, NfaState<TLetter>>(IdComparer<NfaState<TLetter>>.Default);

		internal NfaState(Id<NfaState<TLetter>> id, SymbolId? acceptId) {
			this.Id = id;
			this.AcceptId = acceptId;
		}

		public SymbolId? AcceptId {
			get;
		}

		public RangeDictionary<TLetter, NfaState<TLetter>> MatchTransitions => this.matchTransitions;

		public ICollection<NfaState<TLetter>> EpsilonTransitions => this.epsilonTransitions;

		public bool Equals(NfaState<TLetter> other) {
			if (ReferenceEquals(other, this)) {
				return true;
			}
			return (other != null) && Equals(this.AcceptId, other.AcceptId) && this.matchTransitions.Equals(other.matchTransitions) && this.epsilonTransitions.SetEqual(other.epsilonTransitions, IdComparer<NfaState<TLetter>>.Default);
		}

		public Id<NfaState<TLetter>> Id {
			get;
		}

		public void AddEpsilonTransition(NfaState<TLetter> target) {
			this.epsilonTransitions.Add(target);
		}

		public void AddMatchTransition(Range<TLetter> match, NfaState<TLetter> target) {
			this.matchTransitions.Add(match, target);
		}

		public IEnumerable<NfaState<TLetter>> EpsilonClosure() {
			var pending = new Queue<NfaState<TLetter>>();
			pending.Enqueue(this);
			var processed = new HashSet<Id<NfaState<TLetter>>>();
			do {
				var state = pending.Dequeue();
				if (processed.Add(state.Id)) {
					yield return state;
					foreach (var transition in state.epsilonTransitions.Where(t => !processed.Contains(t.Id))) {
						pending.Enqueue(transition);
					}
				}
			} while (pending.Count > 0);
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			if (ReferenceEquals(this, obj)) {
				return true;
			}
			if (obj.GetType() != GetType()) {
				return false;
			}
			return Equals((NfaState<TLetter>)obj);
		}

		public override int GetHashCode() {
			return unchecked( this.epsilonTransitions.Aggregate(this.matchTransitions.Aggregate(this.AcceptId.GetValueOrDefault().GetHashCode() * 397,
					(hashCode, transition) => hashCode^transition.Key.GetHashCode()^transition.Value.Id.GetHashCode()),
				(hashCode, transition) => hashCode^transition.Id.GetHashCode()));
		}
	}
}
