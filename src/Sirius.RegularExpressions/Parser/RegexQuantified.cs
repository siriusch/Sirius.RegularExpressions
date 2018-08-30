using System;

using Sirius.RegularExpressions.Invariant;
using Sirius.Unicode;

namespace Sirius.RegularExpressions.Parser {
	public class RegexQuantified: RegexExpression {
		public static RegexExpression Create(RegexExpression inner, RegexQuantifier quantifier) {
			if (quantifier.IsZero) {
				return RegexNoOp.Default;
			}
			if (quantifier.IsOne) {
				return inner;
			}
			return new RegexQuantified(inner, quantifier);
		}

		private readonly RegexExpression inner;
		private readonly RegexQuantifier quantifier;

		public RegexQuantified(RegexExpression inner, RegexQuantifier quantifier) {
			this.inner = inner;
			this.quantifier = quantifier;
		}

		public override RxNode<TLetter> ToInvariant<TLetter>(IUnicodeMapper<TLetter> mapper, IRangeSetProvider<Codepoint> provider, bool caseSensitive) {
			return new RxQuantified<TLetter>(this.inner.ToInvariant(mapper, provider, caseSensitive), this.quantifier.Min, this.quantifier.Max);
		}
	}
}
