using System;

using Sirius.RegularExpressions.Invariant;
using Sirius.Unicode;

namespace Sirius.RegularExpressions.Parser {
	public class RegexAccept: RegexExpression {
		public static RegexAccept Create(RegexExpression inner, SymbolId symbol, int? precedence=null) {
			return new RegexAccept(inner, symbol, precedence);
		}

		private readonly RegexExpression inner;

		public RegexAccept(RegexExpression inner, SymbolId symbol, int? precedence) {
			this.inner = inner;
			this.Symbol = symbol;
			this.Precedence = precedence;
		}

		public SymbolId Symbol {
			get;
		}

		public int? Precedence {
			get;
		}

		public override RxNode<TLetter> ToInvariant<TLetter>(IUnicodeMapper<TLetter> mapper, IRangeSetProvider<Codepoint> provider, bool caseSensitive) {
			return new RxAccept<TLetter>(this.inner.ToInvariant(mapper, provider, caseSensitive), this.Symbol, this.Precedence);
		}
	}
}
