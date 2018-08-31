using System;
using System.Collections.Generic;

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

		public RegexAlternation(RegexExpression left, RegexExpression right) {
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
			return visitor.Alternation(this, context);
		}
	}
}
