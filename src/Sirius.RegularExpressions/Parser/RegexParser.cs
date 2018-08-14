using System;
using System.IO;

using bsn.GoldParser.Grammar;
using bsn.GoldParser.Parser;
using bsn.GoldParser.Semantic;

namespace Sirius.RegularExpressions.Parser {
	public static class RegexParser {
		internal static readonly SemanticActions<RegexToken> Actions = new SemanticTypeActions<RegexToken>(CompiledGrammar.Load(typeof(RegexParser), "Regex.egt"));

		public static RegexExpression Parse(string regex, SymbolId? acceptSymbol) {
			var processor = new SemanticProcessor<RegexToken>(new StringReader(regex), Actions);
			var message = processor.ParseAll();
			if (message != ParseMessage.Accept) {
				IToken token = processor.CurrentToken;
				throw new ArgumentException($"Parsing the regular expression \"{regex}\" failed: {message} at {token?.Position ?? new LineInfo(0, 1, 1)}", "regex");
			}
			var expression = (RegexExpression)processor.CurrentToken;
			return acceptSymbol.HasValue ? new RegexAccept(expression, acceptSymbol.Value) : expression;
		}
	}
}
