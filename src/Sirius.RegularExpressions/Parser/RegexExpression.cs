using System;

using bsn.GoldParser.Semantic;

using Sirius.RegularExpressions.Invariant;
using Sirius.RegularExpressions.Parser;
using Sirius.Unicode;

[assembly: RuleTrim("<RegexGroup> ::= '(' <RegexExpression> ')'", "<RegexExpression>", SemanticTokenType = typeof(RegexToken))]

namespace Sirius.RegularExpressions.Parser {
	public abstract class RegexExpression: RegexToken {
		public abstract RxNode<TLetter> ToInvariant<TLetter>(IUnicodeMapper<TLetter> mapper, IRangeSetProvider<Codepoint> provider, bool caseSensitive)
				where TLetter: IEquatable<TLetter>, IComparable<TLetter>;
	}
}
