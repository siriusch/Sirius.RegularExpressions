using System;

using bsn.GoldParser.Semantic;

namespace Sirius.RegularExpressions.Parser {
	[Terminal("(EOF)")]
	[Terminal("(Error)")]
	[Terminal("(Whitespace)")]
	[Terminal("(")]
	[Terminal(")")]
	[Terminal("|")]
	[Terminal("SensitiveGroup")]
	[Terminal("InsensitiveGroup")]
	public class RegexToken: SemanticToken { }
}
