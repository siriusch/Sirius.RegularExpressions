using System;

namespace Sirius.RegularExpressions.Invariant {
	public interface IRegexVisitor<TLetter, in TContext, out TResult>
			where TLetter: IEquatable<TLetter> {
		TResult Accept(RxAccept<TLetter> node, TContext context);

		TResult Alternation(RxAlternation<TLetter> node, TContext context);

		TResult Concatenation(RxConcatenation<TLetter> node, TContext context);

		TResult Empty(RxEmpty<TLetter> node, TContext context);

		TResult Match(RxMatch<TLetter> node, TContext context);

		TResult Quantified(RxQuantified<TLetter> node, TContext context);
	}
}
