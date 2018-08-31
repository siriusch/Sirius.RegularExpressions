using System;

using Sirius.RegularExpressions.Invariant;
using Sirius.Unicode;

namespace Sirius.RegularExpressions.Parser {
	internal sealed class ToInvariantVisitor<TLetter>: IRegexVisitor<ToInvariantVisitor<TLetter>.Context, RxNode<TLetter>>
			where TLetter: IEquatable<TLetter>, IComparable<TLetter> {
		public sealed class Context {
			public Context(IUnicodeMapper<TLetter> mapper, IRangeSetProvider<Codepoint> charSetProvider, bool caseSensitive) {
				this.Mapper = mapper;
				this.CharSetProvider = charSetProvider;
				this.CaseSensitive = caseSensitive;
			}

			public IUnicodeMapper<TLetter> Mapper {
				get;
			}

			public IRangeSetProvider<Codepoint> CharSetProvider {
				get;
			}

			public bool CaseSensitive {
				get;
			}
		}

		public static readonly ToInvariantVisitor<TLetter> Default = new ToInvariantVisitor<TLetter>();

		public RxNode<TLetter> Accept(RegexAccept node, Context context) {
			return new RxAccept<TLetter>(node.Inner.Visit(this, context), node.Symbol, node.Precedence);
		}

		public RxNode<TLetter> Alternation(RegexAlternation node, Context context) {
			return new RxAlternation<TLetter>(node.Left.Visit(this, context), node.Right.Visit(this, context));
		}

		public RxNode<TLetter> Concatenation(RegexConcatenation node, Context context) {
			return new RxConcatenation<TLetter>(node.Left.Visit(this, context), node.Right.Visit(this, context));
		}

		public RxNode<TLetter> Empty(RegexNoOp node, Context context) {
			return RxEmpty<TLetter>.Default;
		}

		public RxNode<TLetter> MatchSet(RegexMatchSet node, Context context) {
			return context.Mapper.MapCodepoints(node.Handle.Negate, node.Handle.GetCharSet(context.CharSetProvider), context.CaseSensitive);
		}

		public RxNode<TLetter> MatchGrapheme(RegexMatchGrapheme node, Context context) {
			return context.Mapper.MapGrapheme(node.Text, context.CaseSensitive);
		}

		public RxNode<TLetter> Quantified(RegexQuantified node, Context context) {
			return new RxQuantified<TLetter>(node.Inner.Visit(this, context), node.Quantifier.Min, node.Quantifier.Max);
		}

		public RxNode<TLetter> CaseGroup(RegexCaseGroup node, Context context) {
			return node.Inner.Visit(this, new Context(context.Mapper, context.CharSetProvider, node.CaseSensitive));
		}
	}
}
