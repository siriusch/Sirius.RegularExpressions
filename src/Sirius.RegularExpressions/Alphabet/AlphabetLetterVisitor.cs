using System;
using System.Collections.Generic;

using Sirius.RegularExpressions.Invariant;

namespace Sirius.RegularExpressions.Alphabet {
	internal class AlphabetLetterVisitor<TLetter>: IRegexVisitor<TLetter, Func<RxMatch<TLetter>, ICollection<LetterId>>, RxNode<LetterId>>
			where TLetter: IEquatable<TLetter>, IComparable<TLetter> {
		public RxNode<LetterId> Accept(RxAccept<TLetter> node, Func<RxMatch<TLetter>, ICollection<LetterId>> context) {
			return new RxAccept<LetterId>(node.Inner.Visit(this, context), node.Symbol, node.AcceptPrecedence);
		}

		public RxNode<LetterId> Alternation(RxAlternation<TLetter> node, Func<RxMatch<TLetter>, ICollection<LetterId>> context) {
			return new RxAlternation<LetterId>(node.Left.Visit(this, context), node.Right.Visit(this, context));
		}

		public RxNode<LetterId> Concatenation(RxConcatenation<TLetter> node, Func<RxMatch<TLetter>, ICollection<LetterId>> context) {
			return new RxConcatenation<LetterId>(node.Left.Visit(this, context), node.Right.Visit(this, context));
		}

		public RxNode<LetterId> Empty(RxEmpty<TLetter> node, Func<RxMatch<TLetter>, ICollection<LetterId>> context) {
			return RxEmpty<LetterId>.Default;
		}

		public RxNode<LetterId> Match(RxMatch<TLetter> node, Func<RxMatch<TLetter>, ICollection<LetterId>> context) {
			return new RxMatch<LetterId>(context(node));
		}

		public RxNode<LetterId> Quantified(RxQuantified<TLetter> node, Func<RxMatch<TLetter>, ICollection<LetterId>> context) {
			return new RxQuantified<LetterId>(node.Inner.Visit(this, context), node.Min, node.Max);
		}
	}
}
