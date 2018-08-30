using System;
using System.Collections.Generic;

using Sirius.RegularExpressions.Invariant;
using Sirius.Unicode;

namespace Sirius.RegularExpressions.Parser {
	public class RegexConcatenation: RegexExpression {
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

		public static RegexExpression Create(RegexExpression left, RegexExpression right) {
			if (right is RegexNoOp) {
				return left;
			}
			if (left is RegexNoOp) {
				return right;
			}
			return new RegexConcatenation(left, right);
		}

		private readonly RegexExpression left;
		private readonly RegexExpression right;

		public RegexConcatenation(RegexExpression left, RegexExpression right) {
			this.left = left;
			this.right = right;
		}

		public override RxNode<TLetter> ToInvariant<TLetter>(IUnicodeMapper<TLetter> mapper, IRangeSetProvider<Codepoint> provider, bool caseSensitive) {
			return new RxConcatenation<TLetter>(this.left.ToInvariant(mapper, provider, caseSensitive), this.right.ToInvariant(mapper, provider, caseSensitive));
		}
	}
}
