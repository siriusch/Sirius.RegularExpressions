using System;

using Sirius.Unicode;

namespace Sirius.RegularExpressions.Parser {
	public class RegexMatchGrapheme: RegexExpression {
		public static RegexExpression Create(string text) {
			if (string.IsNullOrEmpty(text)) {
				return RegexNoOp.Default;
			}
			return new RegexMatchGrapheme(text);
		}

		public RegexMatchGrapheme(string text) {
			this.Text = new Grapheme(text);
		}

		public Grapheme Text {
			get;
		}

		public override TResult Visit<TContext, TResult>(IRegexVisitor<TContext, TResult> visitor, TContext context) {
			return visitor.MatchGrapheme(this, context);
		}
	}
}
