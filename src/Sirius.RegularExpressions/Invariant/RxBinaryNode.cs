using System;

namespace Sirius.RegularExpressions.Invariant {
	public abstract class RxBinaryNode<TLetter>: RxNode<TLetter>
			where TLetter: IEquatable<TLetter>, IComparable<TLetter> {
		protected RxBinaryNode(RxNode<TLetter> left, RxNode<TLetter> right) {
			this.Left = left;
			this.Right = right;
		}

		public RxNode<TLetter> Left {
			get;
		}

		public RxNode<TLetter> Right {
			get;
		}

		public override int GetHashCode() {
			return this.GetType().GetHashCode()^this.Left.GetHashCode()^(this.Right.GetHashCode() * 397);
		}
	}
}
