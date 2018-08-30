using System;

using Sirius.RegularExpressions.Invariant;
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

		public override RxNode<TLetter> ToInvariant<TLetter>(IUnicodeMapper<TLetter> mapper, IRangeSetProvider<Codepoint> provider, bool caseSensitive) {
			return mapper.MapGrapheme(this.Text, caseSensitive);
		}
	}
}
