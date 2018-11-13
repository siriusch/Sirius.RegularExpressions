using System;
using System.Collections.Generic;
using System.Linq;

using Sirius.Collections;
using Sirius.RegularExpressions.Invariant;

namespace Sirius.RegularExpressions.Alphabet {
	public class AlphabetBuilder<TLetter>
			where TLetter: struct, IEquatable<TLetter>, IComparable<TLetter> {
		private static UsedRangeList<TLetter> MakeRanges(IDictionary<Id<RxMatch<TLetter>>, KeyValuePair<RangeSet<TLetter>, ICollection<LetterId>>> charsets, RangeSet<TLetter> validRanges) {
			var ranges = new UsedRangeList<TLetter>();
			foreach (var validRange in validRanges) {
				ranges.Add(new UsedLetterRange<TLetter>(validRange, null));
			}
			foreach (var pair in charsets) {
				foreach (var charRange in pair.Value.Key) {
					// split left if necessary
					var left = RangeOperations<TLetter>.BinarySearch(ranges, charRange.From);
					var leftRange = ranges[left];
					if (leftRange.From.CompareTo(charRange.From) < 0) {
						ranges.Insert(left++, new UsedLetterRange<TLetter>(leftRange.From, Incrementor<TLetter>.Decrement(charRange.From), leftRange.Users));
						ranges[left] = new UsedLetterRange<TLetter>(charRange.From, leftRange.To, leftRange.Users);
					}
					// split right if necessary
					var right = RangeOperations<TLetter>.BinarySearch(ranges, charRange.To);
					var rightRange = ranges[right];
					if (rightRange.To.CompareTo(charRange.To) > 0) {
						ranges[right] = new UsedLetterRange<TLetter>(rightRange.From, charRange.To, rightRange.Users);
						ranges.Insert(right+1, new UsedLetterRange<TLetter>(Incrementor<TLetter>.Increment(charRange.To), rightRange.To, rightRange.Users));
					}
					// add user information
					for (var i = left; i <= right; i++) {
						ranges[i] = ranges[i].AddUser(pair.Key);
					}
				}
			}
			return ranges;
		}

		public AlphabetBuilder(RxNode<TLetter> expression, TLetter? eof = null, RangeSet<TLetter>? validRanges = default) {
			var eofRange = eof.HasValue ? new RangeSet<TLetter>(eof.Value) : RangeSet<TLetter>.Empty;
			// Step 1: Find all charset-generating regular expression parts
			var visitor = new AlphabetBuilderVisitor<TLetter>();
			expression.Visit(visitor, (letters, negate) => letters - eofRange);
			var charsets = visitor.Charsets;
			// Step 2: Get all ranges of all used charsets and register their "users"
			var ranges = MakeRanges(charsets, (validRanges ?? RangeSet<TLetter>.All) - eofRange);
			// Step 3: Group the information into alphabet entries
			var alphabetByKey = ranges
				.GroupBy<UsedLetterRange<TLetter>, string, Range<TLetter>>(r => r.GetUsersKey(), r => r.Range)
				.Select((g, ix) => new KeyValuePair<string, AlphabetLetter<TLetter>>(g.Key, new AlphabetLetter<TLetter>(new LetterId(ix+1), new RangeSet<TLetter>(g))))
				.ToDictionary(p => p.Key, p => p.Value);
			// Step 4: Store alphabet entries for each regex part
			foreach (var range in ranges) {
				var alphabetEntry = alphabetByKey[range.GetUsersKey()];
				for (var rangeUser = range.Users; rangeUser != null; rangeUser = rangeUser.Next) {
					charsets[rangeUser.User].Value.Add(alphabetEntry.Id);
				}
			}
			// Step 5: store alphabet information
			this.AlphabetById = alphabetByKey.Values.ToDictionary(e => e.Id, e => e.Ranges);
			this.AlphabetById.Add(LetterId.Eof, eofRange); // EOF
			// Step 6: rebuild expression
			this.Expression = expression.Visit(new AlphabetLetterVisitor<TLetter>(), node => charsets[visitor.GetId(node)].Value);
		}

		public IDictionary<LetterId, RangeSet<TLetter>> AlphabetById {
			get;
		}

		public RxNode<LetterId> Expression {
			get;
		}

		public RangeSet<LetterId> Negate(RangeSet<LetterId> arg) {
			return new RangeSet<LetterId>(this.AlphabetById.Keys) - arg;
		}
	}
}
