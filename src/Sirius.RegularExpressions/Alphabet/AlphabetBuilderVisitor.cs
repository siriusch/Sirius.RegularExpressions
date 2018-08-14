using System;
using System.Collections.Generic;

using Sirius.Collections;
using Sirius.RegularExpressions.Invariant;

namespace Sirius.RegularExpressions.Alphabet {
	internal class AlphabetBuilderVisitor<TLetter>: IRegexVisitor<TLetter, Func<ICollection<TLetter>, bool, RangeSet<TLetter>>, object>
			where TLetter: IEquatable<TLetter>, IComparable<TLetter> {
		private readonly Dictionary<Id<RxMatch<TLetter>>, KeyValuePair<RangeSet<TLetter>, ICollection<LetterId>>> charsets = new Dictionary<Id<RxMatch<TLetter>>, KeyValuePair<RangeSet<TLetter>, ICollection<LetterId>>>();
		private readonly Dictionary<RxMatch<TLetter>, Id<RxMatch<TLetter>>> idMap = new Dictionary<RxMatch<TLetter>, Id<RxMatch<TLetter>>>(ReferenceEqualityComparer<RxMatch<TLetter>>.Default);

		public IDictionary<Id<RxMatch<TLetter>>, KeyValuePair<RangeSet<TLetter>, ICollection<LetterId>>> Charsets => this.charsets;

		public object Accept(RxAccept<TLetter> node, Func<ICollection<TLetter>, bool, RangeSet<TLetter>> context) {
			node.Inner.Visit(this, context);
			return null;
		}

		public object Alternation(RxAlternation<TLetter> node, Func<ICollection<TLetter>, bool, RangeSet<TLetter>> context) {
			node.Left.Visit(this, context);
			node.Right.Visit(this, context);
			return null;
		}

		public object Concatenation(RxConcatenation<TLetter> node, Func<ICollection<TLetter>, bool, RangeSet<TLetter>> context) {
			node.Left.Visit(this, context);
			node.Right.Visit(this, context);
			return null;
		}

		public object Empty(RxEmpty<TLetter> node, Func<ICollection<TLetter>, bool, RangeSet<TLetter>> context) {
			return null;
		}

		public object Match(RxMatch<TLetter> node, Func<ICollection<TLetter>, bool, RangeSet<TLetter>> context) {
			Id<RxMatch<TLetter>> id;
			if (!this.idMap.TryGetValue(node, out id)) {
				id = new Id<RxMatch<TLetter>>(this.idMap.Count+1);
				this.idMap.Add(node, id);
			}
			this.charsets.Add(id, new KeyValuePair<RangeSet<TLetter>, ICollection<LetterId>>(context(node.Letters, node.Negate), new HashSet<LetterId>()));
			return null;
		}

		public object Quantified(RxQuantified<TLetter> node, Func<ICollection<TLetter>, bool, RangeSet<TLetter>> context) {
			node.Inner.Visit(this, context);
			return null;
		}

		public Id<RxMatch<TLetter>> GetId(RxMatch<TLetter> node) {
			return this.idMap[node];
		}
	}
}
