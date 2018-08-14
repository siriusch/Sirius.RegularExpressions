using System;
using System.Collections.Generic;

using bsn.GoldParser.Semantic;

using Sirius.RegularExpressions.Invariant;
using Sirius.Unicode;

namespace Sirius.RegularExpressions.Parser {
	public class RegexAlternation: RegexExpression {
		public static RegexExpression Create(params RegexExpression[] expressions) {
			return Create((IEnumerable<RegexExpression>)expressions);
		}

		public static RegexExpression Create(IEnumerable<RegexExpression> expressions) {
			using (var enumerator = expressions.GetEnumerator()) {
				if (!enumerator.MoveNext()) {
					return RegexNoOp.Default;
				}
				var result = enumerator.Current;
				while (enumerator.MoveNext()) {
					result = Create(result, enumerator.Current);
				}
				return result;
			}
		}

		[Rule("<RegexExpression> ::= <RegexConcatenation> ~'|' <RegexExpression>")]
		public static RegexExpression Create(RegexExpression left, RegexExpression right) {
			var leftMatch = left as RegexMatchSet;
			if (leftMatch != null) {
				var rightMatch = right as RegexMatchSet;
				if (rightMatch != null) {
					var handle = new RangeSetHandle.Union(false);
					handle.Add(leftMatch.Handle);
					handle.Add(rightMatch.Handle);
					return new RegexMatchSet(leftMatch.Text+'|'+rightMatch.Text, handle);
				}
			}
			return new RegexAlternation(left, right);
		}

		private readonly RegexExpression left;
		private readonly RegexExpression right;

		public RegexAlternation(RegexExpression left, RegexExpression right) {
			this.left = left;
			this.right = right;
		}

		public override RxNode<TLetter> ToInvariant<TLetter>(IUnicodeMapper<TLetter> mapper, IRangeSetProvider<Codepoint> provider, bool caseSensitive) {
			return new RxAlternation<TLetter>(this.left.ToInvariant(mapper, provider, caseSensitive), this.right.ToInvariant(mapper, provider, caseSensitive));
		}
	}
}
