using System;
using System.Collections.Generic;
using System.Linq;

using Sirius.Collections;

namespace Sirius.RegularExpressions.Invariant {
	internal class OptimizerVisitor<TLetter>: IRegexVisitor<TLetter, object, RxNode<TLetter>>
			where TLetter: IEquatable<TLetter> {
		public RxNode<TLetter> Accept(RxAccept<TLetter> node, object context) {
			return new RxAccept<TLetter>(node.Inner.Visit(this, context), node.Symbol);
		}

		public RxNode<TLetter> Alternation(RxAlternation<TLetter> node, object context) {
			var single = new HashSet<TLetter>();
			var other = new HashSet<RxNode<TLetter>>();
			foreach (var alternationNode in node.VisitBinary<TLetter, RxAlternation<TLetter>, object, RxNode<TLetter>>(this, context)) {
				var matchNode = alternationNode as RxMatch<TLetter>;
				if ((matchNode != null) && !matchNode.Negate) {
					single.UnionWith(matchNode.Letters);
				} else {
					other.Add(alternationNode);
				}
			}
			IEnumerable<RxNode<TLetter>> result = other;
			if (single.Count > 0) {
				result = result.Append(new RxMatch<TLetter>(false, single));
			}
			return result.JoinAlternation();
		}

		public RxNode<TLetter> Concatenation(RxConcatenation<TLetter> node, object context) {
			return node
				.VisitBinary<TLetter, RxConcatenation<TLetter>, object, RxNode<TLetter>>(this, context)
				.Where(n => !n.IsEmpty())
				.JoinConcatenation();
		}

		public RxNode<TLetter> Empty(RxEmpty<TLetter> node, object context) {
			return RxEmpty<TLetter>.Default;
		}

		public RxNode<TLetter> Match(RxMatch<TLetter> node, object context) {
			return node;
		}

		public RxNode<TLetter> Quantified(RxQuantified<TLetter> node, object context) {
			if ((node.Max == 0) || node.Inner.IsEmpty()) {
				return RxEmpty<TLetter>.Default;
			}
			var inner = node.Inner.Visit(this, context);
			if ((node.Min == 1) && (node.Max == 1)) {
				return inner;
			}
			return new RxQuantified<TLetter>(inner, node.Min, node.Max);
		}
	}
}
