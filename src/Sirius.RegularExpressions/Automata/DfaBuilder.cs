using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;

using Sirius.Collections;

namespace Sirius.RegularExpressions.Automata {
	public static class DfaBuilder<TLetter>
			where TLetter: struct, IComparable<TLetter>, IEquatable<TLetter> {
		public static Dfa<TLetter> Build(INonFiniteAutomaton<TLetter> nfa, TLetter? eof = null, Id<DfaState<TLetter>> firstId = default(Id<DfaState<TLetter>>)) {
			var dfaStates = new Dictionary<string, DfaStateBuilder<TLetter>>(StringComparer.Ordinal);
			var result = new List<DfaState<TLetter>>();
			// Step 1: compute the epsilon closure information for all NFA nodes
			var closures = nfa.States.ToDictionary(s => s.Id, state => state.EpsilonClosure().ToArray());
			// Step 2: simulate transitions
			var pending = new Queue<DfaStateBuilder<TLetter>>();
			DfaStateBuilder<TLetter> startDfaState;
			if (!GetStateBuilder(dfaStates, firstId, closures[nfa.StartState.Id], out startDfaState)) {
				throw new InvalidOperationException("A new DFA state builder was expected");
			}
			DfaStateBuilder<TLetter> acceptDfaState;
			GetStateBuilder(dfaStates, firstId, new NfaState<TLetter>[0], out acceptDfaState);
			pending.Enqueue(startDfaState);
			do {
				var currentDfaState = pending.Dequeue();
				var allNfaTransitions = new RangeDictionary<TLetter, IEnumerable<NfaState<TLetter>>>();
				foreach (var right in currentDfaState.NfaStates.Select(s => s.MatchTransitions)) {
					var left = allNfaTransitions;
					allNfaTransitions = new RangeDictionary<TLetter, IEnumerable<NfaState<TLetter>>>(RangeSet<TLetter>.EnumerateRanges(left.Keys, right.Keys, (rng, leftIndex, rightIndex) => {
						var rangeStates = leftIndex.HasValue ? left.Values[leftIndex.Value] : Enumerable.Empty<NfaState<TLetter>>();
						if (rightIndex.HasValue) {
							rangeStates = rangeStates.Append(right.Values[rightIndex.Value]);
						}
						return new KeyValuePair<Range<TLetter>, IEnumerable<NfaState<TLetter>>>(rng, rangeStates);
					}));
				}
				var groupedNfaTransitions = new RangeDictionary<TLetter, HashSet<NfaState<TLetter>>>(
						allNfaTransitions.Select(p => new KeyValuePair<Range<TLetter>, HashSet<NfaState<TLetter>>>(p.Key, new HashSet<NfaState<TLetter>>(p.Value))),
						SetEqualityComparer<NfaState<TLetter>>.Default);
				foreach (var matchTarget in groupedNfaTransitions) {
					DfaStateBuilder<TLetter> targetDfaState;
					if (GetStateBuilder(dfaStates, firstId, matchTarget.Value.SelectMany(t => closures[t.Id]), out targetDfaState)) {
						pending.Enqueue(targetDfaState);
					}
					currentDfaState.SetTransition(matchTarget.Key, targetDfaState);
				}
			} while (pending.Count > 0);
			// Step 3: identify and remove identical (same transitions) states
			while (true) {
				var dupes = dfaStates
						.Values
						.GroupBy(s => s)
						.Select(g => new KeyValuePair<DfaStateBuilder<TLetter>, ICollection<DfaStateBuilder<TLetter>>>(g.Key, g.Where(s => !ReferenceEquals(s, g.Key)).ToList()))
						.Where(p => p.Value.Count > 1)
						.ToDictionary();
				if (dupes.Count == 0) {
					break;
				}
				foreach (var dupe in dupes) {
					foreach (var builder in dfaStates.Values) {
						var dupeIds = new HashSet<int>(dupe.Value.Select(s => s.Id));
						builder.ReplaceTransition(b => dupeIds.Contains(b.Id), dupe.Key);
					}
				}
				foreach (var builder in dupes.SelectMany(d => d.Value)) {
					dfaStates.Remove(builder.Key);
				}
			}
			// Step 4: make the DFA states; the first is the start state
			var states = new Dictionary<DfaStateBuilder<TLetter>, DfaState<TLetter>>();
			var symbolStates = new Dictionary<Id<DfaState<TLetter>>, SymbolId>();
			pending.Enqueue(startDfaState);
			do {
				var builder = pending.Dequeue();
				if (!states.ContainsKey(builder)) {
					var state = ((Func<Id<DfaState<TLetter>>, DfaState<TLetter>>)(id => new DfaState<TLetter>(id)))(new Id<DfaState<TLetter>>(result.Count));
					result.Add(state);
					states.Add(builder, state);
					foreach (var transition in builder.GetTransitions()) {
						pending.Enqueue(transition.Value);
					}
					var acceptSymbolIds = builder
							.NfaStates
							.Where(s => s.AcceptId.HasValue)
							// ReSharper disable once PossibleInvalidOperationException
							.GroupBy(a => a.Precedence, a => a.AcceptId.Value)
							.OrderByDescending(g => g.Key)
							.Take(1)
							.SelectMany(g => g)
							.Distinct()
							.ToList();
					switch (acceptSymbolIds.Count) {
					case 0:
						break;
					case 1:
						symbolStates.Add(state.Id, acceptSymbolIds[0]);
						if (eof.HasValue) {
							state.SetTransition(Range<TLetter>.Create(eof.Value), Dfa<TLetter>.Accept);
						}
						break;
					default:
						throw new InvalidOperationException("The state " + state.Id + " has multiple same-precedence accept states " + string.Join(",", acceptSymbolIds));
					}
				}
			} while (pending.Count > 0);
			// Step 5: apply transitions
			foreach (var pair in states) {
				foreach (var transition in pair.Key.GetTransitions()) {
					var range = transition.Key;
					var target = transition.Value;
					pair.Value.SetTransition(range, states[target].Id);
				}
			}
			return new Dfa<TLetter>(eof, result, symbolStates);
		}

		private static bool GetStateBuilder(Dictionary<string, DfaStateBuilder<TLetter>> dfaStates, Id<DfaState<TLetter>> firstId, IEnumerable<NfaState<TLetter>> states, out DfaStateBuilder<TLetter> builder) {
			var stateArray = states.Distinct().OrderBy(s => s.Id).ToArray();
			var key = string.Join("|", stateArray.Select(s => XmlConvert.ToString(s.Id.ToInt32())));
			if (string.IsNullOrEmpty(key)) {
				key = "EOF";
			}
			if (!dfaStates.TryGetValue(key, out builder)) {
				builder = new DfaStateBuilder<TLetter>(dfaStates.Count + firstId.ToInt32(), key, stateArray);
				dfaStates.Add(key, builder);
				return true;
			}
			Debug.Assert(stateArray.SetEqual(builder.NfaStates));
			return false;
		}
	}
}
