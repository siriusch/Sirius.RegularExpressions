using System;

using Sirius.Text;

namespace Sirius.RegularExpressions.Invariant {
	public sealed class RxAlternation<TLetter>: RxBinaryNode<TLetter>
			where TLetter: IEquatable<TLetter>, IComparable<TLetter> {
		public RxAlternation(RxNode<TLetter> left, RxNode<TLetter> right): base(left, right) { }

		internal override int EvaluationPrecedence => 2;

		public override void ComputeLengths(out int min, out int? max) {
			int leftMin;
			int? leftMax;
			this.Left.ComputeLengths(out leftMin, out leftMax);
			int rightMin;
			int? rightMax;
			this.Right.ComputeLengths(out rightMin, out rightMax);
			min = Math.Min(leftMin, rightMin);
			if (leftMax.HasValue && rightMax.HasValue) {
				max = Math.Max(leftMax.Value, rightMax.Value);
			} else {
				max = null;
			}
		}

		protected override bool EqualsInternal(RxNode<TLetter> other) {
			var otherAlternation = (RxAlternation<TLetter>)other;
			return (otherAlternation.Left.Equals(this.Left) && otherAlternation.Right.Equals(this.Right)) || (otherAlternation.Left.Equals(this.Right) && otherAlternation.Right.Equals(this.Left));
		}

		public override TResult Visit<TContext, TResult>(IRegexVisitor<TLetter, TContext, TResult> visitor, TContext context) {
			return visitor.Alternation(this, context);
		}

		protected override void WriteToInternal(RichTextWriter writer) {
			this.Left.WriteTo(writer, this.EvaluationPrecedence);
			writer.Write('|');
			this.Right.WriteTo(writer, this.EvaluationPrecedence);
		}
	}
}
