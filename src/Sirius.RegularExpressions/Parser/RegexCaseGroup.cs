namespace Sirius.RegularExpressions.Parser {
	public class RegexCaseGroup: RegexExpression {
		public static RegexExpression Create(RegexExpression inner, bool caseSensitive) {
			return (RegexExpression)(inner as RegexNoOp) ?? new RegexCaseGroup(inner, caseSensitive);
		}

		public static RegexExpression CreateSensitive(RegexExpression inner) {
			return Create(inner, true);
		}

		public static RegexExpression CreateInsensitive(RegexExpression inner) {
			return Create(inner, false);
		}

		private RegexCaseGroup(RegexExpression inner, bool caseSensitive) {
			this.Inner = inner;
			this.CaseSensitive = caseSensitive;
		}

		public bool CaseSensitive {
			get;
		}

		public RegexExpression Inner {
			get;
		}

		public override TResult Visit<TContext, TResult>(IRegexVisitor<TContext, TResult> visitor, TContext context) {
			return visitor.CaseGroup(this, context);
		}
	}
}
