using System;
using System.Collections.Generic;

namespace Sirius.RegularExpressions.Parser {
	public class RegexAccept: RegexExpression {
		public static RegexExpression Create(RegexExpression inner, SymbolId symbol, int? precedence = null) {
			return new RegexAccept(inner, symbol, precedence);
		}

		public static RegexExpression CreateDeep(RegexExpression inner, SymbolId symbol, int? precedence = null) {
			return inner.Visit(AcceptDeepVisitor.Default, new KeyValuePair<SymbolId, int?>(symbol, precedence));
		}

		public RegexAccept(RegexExpression inner, SymbolId symbol, int? precedence) {
			this.Inner = inner;
			this.Symbol = symbol;
			this.Precedence = precedence;
		}

		public RegexExpression Inner {
			get;
		}

		public SymbolId Symbol {
			get;
		}

		public int? Precedence {
			get;
		}

		public override TResult Visit<TContext, TResult>(IRegexVisitor<TContext, TResult> visitor, TContext context) {
			return visitor.Accept(this, context);
		}
	}
}
