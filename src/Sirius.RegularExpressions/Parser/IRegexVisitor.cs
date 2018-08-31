using System;

namespace Sirius.RegularExpressions.Parser {
	public interface IRegexVisitor<in TContext, out TResult> {
		TResult Accept(RegexAccept node, TContext context);

		TResult Alternation(RegexAlternation node, TContext context);

		TResult Concatenation(RegexConcatenation node, TContext context);

		TResult Empty(RegexNoOp node, TContext context);

		TResult MatchSet(RegexMatchSet node, TContext context);

		TResult MatchGrapheme(RegexMatchGrapheme node, TContext context);

		TResult Quantified(RegexQuantified node, TContext context);

		TResult CaseGroup(RegexCaseGroup node, TContext context);
	}
}
