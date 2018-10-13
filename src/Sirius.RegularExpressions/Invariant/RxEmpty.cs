using System;

using Sirius.Text;

namespace Sirius.RegularExpressions.Invariant {
	public sealed class RxEmpty<TLetter>: RxNode<TLetter>
			where TLetter: IEquatable<TLetter>, IComparable<TLetter> {
		public static readonly RxEmpty<TLetter> Default = new RxEmpty<TLetter>();

		internal override int EvaluationPrecedence => 0;

		public override void ComputeLengths(out int min, out int? max) {
			min = 0;
			max = 0;
		}

		protected override bool EqualsInternal(RxNode<TLetter> other) {
			return true;
		}

		public override int GetHashCode() {
			return 0;
		}

		public override TResult Visit<TContext, TResult>(IRegexVisitor<TLetter, TContext, TResult> visitor, TContext context) {
			return visitor.Empty(this, context);
		}

		protected override void WriteToInternal(RichTextWriter writer) { }
	}
}
