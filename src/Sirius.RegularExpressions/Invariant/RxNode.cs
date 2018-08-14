using System;
using System.IO;

using bsn.GoldParser.Text;

namespace Sirius.RegularExpressions.Invariant {
	public abstract class RxNode<TLetter>: IEquatable<RxNode<TLetter>>
			where TLetter: IEquatable<TLetter> {
		internal abstract int PrecedenceLevel {
			get;
		}

		public bool Equals(RxNode<TLetter> other) {
			if (ReferenceEquals(other, null)) {
				return false;
			}
			if (ReferenceEquals(other, this)) {
				return true;
			}
			return (other.GetType() == GetType()) && EqualsInternal(other);
		}

		public abstract void ComputeLengths(out int min, out int? max);

		public sealed override bool Equals(object other) {
			return EqualsInternal(other as RxNode<TLetter>);
		}

		protected abstract bool EqualsInternal(RxNode<TLetter> other);

		public override int GetHashCode() {
			throw new NotImplementedException(nameof(GetHashCode)+" must be implemented by overriding class");
		}

		public sealed override string ToString() {
			using (var result = new StringWriter()) {
				using (var writer = RichTextWriter.Wrap(result)) {
					WriteTo(writer, this.PrecedenceLevel);
				}
				return result.ToString();
			}
		}

		public abstract TResult Visit<TContext, TResult>(IRegexVisitor<TLetter, TContext, TResult> visitor, TContext context);

		public void WriteTo(RichTextWriter writer, int currentPrecedenceLevel) {
			if (currentPrecedenceLevel < this.PrecedenceLevel) {
				writer.Write('(');
				WriteToInternal(writer);
				writer.Write(')');
			} else {
				WriteToInternal(writer);
			}
		}

		protected abstract void WriteToInternal(RichTextWriter writer);
	}
}
