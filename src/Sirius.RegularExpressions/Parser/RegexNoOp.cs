using System;

using Sirius.RegularExpressions.Invariant;
using Sirius.Unicode;

namespace Sirius.RegularExpressions.Parser {
	public sealed class RegexNoOp: RegexExpression {
		public static readonly RegexNoOp Default = new RegexNoOp();

		public override RxNode<TLetter> ToInvariant<TLetter>(IUnicodeMapper<TLetter> mapper, IRangeSetProvider<Codepoint> provider, bool caseSensitive) {
			return RxEmpty<TLetter>.Default;
		}

		private RegexNoOp() { }
	}
}
