using System;
using System.Collections.Generic;

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

		public RegexConcatenation(RegexExpression left, RegexExpression right) {
			this.Left = left;
			this.Right = right;
		}

		public RegexExpression Left {
			get;
		}

		public RegexExpression Right {
			get;
		}

		public override TResult Visit<TContext, TResult>(IRegexVisitor<TContext, TResult> visitor, TContext context) {
			return visitor.Concatenation(this, context);
		}
	}
}
