using System;

using Sirius.Text;

namespace Sirius.RegularExpressions.Invariant {
	public sealed class RxConcatenation<TLetter>: RxBinaryNode<TLetter>
			where TLetter: IEquatable<TLetter>, IComparable<TLetter> {
		public RxConcatenation(RxNode<TLetter> left, RxNode<TLetter> right): base(left, right) { }

		internal override int EvaluationPrecedence => 1;

		public override void ComputeLengths(out int min, out int? max) {
			int leftMin;
			int? leftMax;
			this.Left.ComputeLengths(out leftMin, out leftMax);
			int rightMin;
			int? rightMax;
			this.Right.ComputeLengths(out rightMin, out rightMax);
			min = leftMin+rightMin;
			if (leftMax.HasValue && rightMax.HasValue) {
				max = leftMax.Value+rightMax.Value;
			} else {
				max = null;
			}
		}

		protected override bool EqualsInternal(RxNode<TLetter> other) {
			var otherConcatenation = (RxConcatenation<TLetter>)other;
			return otherConcatenation.Left.Equals(this.Left) && otherConcatenation.Right.Equals(this.Right);
		}

		public override TResult Visit<TContext, TResult>(IRegexVisitor<TLetter, TContext, TResult> visitor, TContext context) {
			return visitor.Concatenation(this, context);
		}

		protected override void WriteToInternal(RichTextWriter writer) {
			this.Left.WriteTo(writer, this.EvaluationPrecedence);
			this.Right.WriteTo(writer, this.EvaluationPrecedence);
		}
	}
}
