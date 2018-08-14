using System;

using Sirius.RegularExpressions.Invariant;
using Sirius.Unicode;

namespace Sirius.RegularExpressions.Parser {
	public class RegexAccept: RegexExpression {
		public static RegexAccept Create(RegexExpression inner, SymbolId symbol) {
			return new RegexAccept(inner, symbol);
		}

		private readonly RegexExpression inner;

		public RegexAccept(RegexExpression inner, SymbolId symbol) {
			this.inner = inner;
			this.Symbol = symbol;
		}

		public SymbolId Symbol {
			get;
		}

		public override RxNode<TLetter> ToInvariant<TLetter>(IUnicodeMapper<TLetter> mapper, IRangeSetProvider<Codepoint> provider, bool caseSensitive) {
			return new RxAccept<TLetter>(this.inner.ToInvariant(mapper, provider, caseSensitive), this.Symbol);
		}
	}
}
