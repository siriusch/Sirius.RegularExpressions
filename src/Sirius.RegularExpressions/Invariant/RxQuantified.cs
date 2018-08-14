using System;

using bsn.GoldParser.Text;

namespace Sirius.RegularExpressions.Invariant {
	public sealed class RxQuantified<TLetter>: RxNode<TLetter>
			where TLetter: IEquatable<TLetter> {
		public RxQuantified(RxNode<TLetter> inner, int min, int? max) {
			if (min < 0) {
				throw new ArgumentOutOfRangeException(nameof(min), $"{nameof(min)} cannot be inferior to 0");
			}
			if (max.HasValue && (max.Value < min)) {
				throw new ArgumentOutOfRangeException(nameof(max), $"{nameof(max)} cannot be inferior to {nameof(min)}");
			}
			this.Inner = inner;
			this.Min = min;
			this.Max = max;
		}

		public RxNode<TLetter> Inner {
			get;
		}

		public int Min {
			get;
		}

		public int? Max {
			get;
		}

		internal override int PrecedenceLevel => 0;

		public override void ComputeLengths(out int min, out int? max) {
			min = this.Min;
			max = this.Max;
		}

		protected override bool EqualsInternal(RxNode<TLetter> other) {
			var otherQuantifier = (RxQuantified<TLetter>)other;
			return this.Inner.Equals(otherQuantifier.Inner) && (this.Min == otherQuantifier.Min) && (this.Max == otherQuantifier.Max);
		}

		public override int GetHashCode() {
			return typeof(RxQuantified<TLetter>).GetHashCode()^this.Inner.GetHashCode()^this.Min.GetHashCode()^this.Max.GetHashCode();
		}

		public override TResult Visit<TContext, TResult>(IRegexVisitor<TLetter, TContext, TResult> visitor, TContext context) {
			return visitor.Quantified(this, context);
		}

		protected override void WriteToInternal(RichTextWriter writer) {
			this.Inner.WriteTo(writer, this.PrecedenceLevel);
			if (this.Max.GetValueOrDefault(int.MaxValue) > 0) {
				if ((this.Min == 0) && (this.Max == 1)) {
					writer.Write("?");
				} else if ((this.Min == 0) && !this.Max.HasValue) {
					writer.Write("*");
				} else if ((this.Min == 1) && !this.Max.HasValue) {
					writer.Write("+");
				} else {
					writer.Write('{');
					writer.Write(this.Min);
					if (this.Min != this.Max) {
						writer.Write(',');
						if (this.Max.HasValue) {
							writer.Write(this.Max.Value);
						}
					}
					writer.Write('}');
				}
			}
		}
	}
}
