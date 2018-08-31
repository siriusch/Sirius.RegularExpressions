using System;

using Sirius.RegularExpressions.Invariant;
using Sirius.Unicode;

namespace Sirius.RegularExpressions.Parser {
	public abstract class RegexExpression {
		public abstract TResult Visit<TContext, TResult>(IRegexVisitor<TContext, TResult> visitor, TContext context);

		public RxNode<TLetter> ToInvariant<TLetter>(IUnicodeMapper<TLetter> mapper, IRangeSetProvider<Codepoint> provider, bool caseSensitive)
				where TLetter : IEquatable<TLetter>, IComparable<TLetter> {
			return this.Visit(ToInvariantVisitor<TLetter>.Default, new ToInvariantVisitor<TLetter>.Context(mapper, provider, caseSensitive));
		}
	}
}
