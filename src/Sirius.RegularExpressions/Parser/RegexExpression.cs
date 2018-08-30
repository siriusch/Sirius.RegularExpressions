using System;

using Sirius.RegularExpressions.Invariant;
using Sirius.Unicode;

namespace Sirius.RegularExpressions.Parser {
	public abstract class RegexExpression {
		public abstract RxNode<TLetter> ToInvariant<TLetter>(IUnicodeMapper<TLetter> mapper, IRangeSetProvider<Codepoint> provider, bool caseSensitive)
				where TLetter: IEquatable<TLetter>, IComparable<TLetter>;
	}
}
