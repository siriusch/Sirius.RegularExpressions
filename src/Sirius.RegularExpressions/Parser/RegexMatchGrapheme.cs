using System;

using bsn.GoldParser.Semantic;

using Sirius.RegularExpressions.Invariant;
using Sirius.Unicode;

namespace Sirius.RegularExpressions.Parser {
	public class RegexMatchGrapheme: RegexExpression {
		[Terminal("RegexLetter")]
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
