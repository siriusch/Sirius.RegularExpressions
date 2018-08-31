using System;

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

		public RegexQuantified(RegexExpression inner, RegexQuantifier quantifier) {
			this.Inner = inner;
			this.Quantifier = quantifier;
		}

		public RegexExpression Inner {
			get;
		}

		public RegexQuantifier Quantifier {
			get;
		}

		public override TResult Visit<TContext, TResult>(IRegexVisitor<TContext, TResult> visitor, TContext context) {
			return visitor.Quantified(this, context);
		}
	}
}
