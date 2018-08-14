using System;
using System.Collections.Generic;
using System.Linq;

namespace Sirius.RegularExpressions.Invariant {
	public static class RxExtensions {
		public static RxNode<TLetter> Optimize<TLetter>(this RxNode<TLetter> node)
				where TLetter: IEquatable<TLetter> {
			return node.Visit(new OptimizerVisitor<TLetter>(), null);
		}

		public static IEnumerable<TResult> VisitBinary<TLetter, TBinaryNode, TContext, TResult>(this RxNode<TLetter> node, IRegexVisitor<TLetter, TContext, TResult> visitor, TContext context)
				where TBinaryNode: RxBinaryNode<TLetter>
				where TLetter: IEquatable<TLetter> {
			var binaryNode = node as TBinaryNode;
			if (binaryNode != null) {
				return binaryNode.Left.VisitBinary<TLetter, TBinaryNode, TContext, TResult>(visitor, context)
					.Concat(binaryNode.Right.VisitBinary<TLetter, TBinaryNode, TContext, TResult>(visitor, context));
			}
			return new[] { node.Visit(visitor, context) };
		}

		public static RxNode<TLetter> JoinConcatenation<TLetter>(this IEnumerable<RxNode<TLetter>> input)
				where TLetter: IEquatable<TLetter> {
			return input.Where(n => !(n is RxEmpty<TLetter>)).JoinBinary((l, r) => new RxConcatenation<TLetter>(l, r));
		}

		public static RxNode<TLetter> JoinAlternation<TLetter>(this IEnumerable<RxNode<TLetter>> input)
				where TLetter: IEquatable<TLetter> {
			return input.Distinct().JoinBinary((l, r) => new RxAlternation<TLetter>(l, r));
		}

		private static RxNode<TLetter> JoinBinary<TLetter>(this IEnumerable<RxNode<TLetter>> input, Func<RxNode<TLetter>, RxNode<TLetter>, RxNode<TLetter>> joinFunc)
				where TLetter: IEquatable<TLetter> {
			var nodes = new Stack<RxNode<TLetter>>(input);
			if (nodes.Count == 0) {
				return RxEmpty<TLetter>.Default;
			}
			var result = nodes.Pop();
			while (nodes.Count > 0) {
				result = joinFunc(nodes.Pop(), result);
			}
			return result;
		}

		public static bool IsEmpty<TLetter>(this RxNode<TLetter> node)
				where TLetter: IEquatable<TLetter> {
			int min;
			int? max;
			node.ComputeLengths(out min, out max);
			return max == 0;
		}
	}
}
