using System.Collections.Generic;

namespace Sirius.RegularExpressions.Parser {
	internal class AcceptDeepVisitor: IRegexVisitor<KeyValuePair<SymbolId, int?>, RegexExpression> {
		public static readonly AcceptDeepVisitor Default = new AcceptDeepVisitor();

		public RegexExpression Accept(RegexAccept node, KeyValuePair<SymbolId, int?> context) {
			return node;
		}

		public RegexExpression Alternation(RegexAlternation node, KeyValuePair<SymbolId, int?> context) {
			return RegexAlternation.Create(node.Left.Visit(this, context), node.Right.Visit(this, context));
		}

		public RegexExpression Concatenation(RegexConcatenation node, KeyValuePair<SymbolId, int?> context) {
			return RegexConcatenation.Create(node.Left.Visit(this, context), node.Right.Visit(this, context));
		}

		public RegexExpression Empty(RegexNoOp node, KeyValuePair<SymbolId, int?> context) {
			return node;
		}

		public RegexExpression MatchSet(RegexMatchSet node, KeyValuePair<SymbolId, int?> context) {
			return RegexAccept.Create(node, context.Key, context.Value);
		}

		public RegexExpression MatchGrapheme(RegexMatchGrapheme node, KeyValuePair<SymbolId, int?> context) {
			return RegexAccept.Create(node, context.Key, context.Value);
		}

		public RegexExpression Quantified(RegexQuantified node, KeyValuePair<SymbolId, int?> context) {
			return RegexQuantified.Create(node.Inner.Visit(this, context), node.Quantifier);
		}

		public RegexExpression CaseGroup(RegexCaseGroup node, KeyValuePair<SymbolId, int?> context) {
			return RegexCaseGroup.Create(node.Inner.Visit(this, context), node.CaseSensitive);
		}
	}
}