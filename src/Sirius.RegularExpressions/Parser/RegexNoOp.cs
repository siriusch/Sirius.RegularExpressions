using System;

namespace Sirius.RegularExpressions.Parser {
	public sealed class RegexNoOp: RegexExpression {
		public static readonly RegexNoOp Default = new RegexNoOp();

		public override TResult Visit<TContext, TResult>(IRegexVisitor<TContext, TResult> visitor, TContext context) {
			return visitor.Empty(this, context);
		}

		private RegexNoOp() { }
	}
}
