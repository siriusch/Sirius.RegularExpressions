using System;
using System.IO;

using Sirius.Text;

namespace Sirius.RegularExpressions.Invariant {
	public sealed class RxAccept<TLetter>: RxNode<TLetter>
			where TLetter: IEquatable<TLetter> {
		public RxAccept(RxNode<TLetter> inner, SymbolId symbol, int? precedence) {
			this.Symbol = symbol;
			if (precedence.HasValue) {
				this.AcceptPrecedence = precedence.Value;
			} else {
				inner.ComputeLengths(out var min, out var max);
				this.AcceptPrecedence = min == max ? int.MaxValue : min*(ushort.MaxValue + 1)+max.GetValueOrDefault(ushort.MaxValue);
			}
			this.Inner = inner;
		}

		public RxNode<TLetter> Inner {
			get;
		}

		public SymbolId Symbol {
			get;
		}

		public int AcceptPrecedence {
			get;
		}

		internal override int EvaluationPrecedence => this.Inner.EvaluationPrecedence;

		public override void ComputeLengths(out int min, out int? max) {
			this.Inner.ComputeLengths(out min, out max);
		}

		protected override bool EqualsInternal(RxNode<TLetter> other) {
			return ((RxAccept<TLetter>)other).Symbol == this.Symbol;
		}

		public override int GetHashCode() {
			return typeof(RxAccept<TLetter>).GetHashCode()^this.Symbol.GetHashCode();
		}

		public override TResult Visit<TContext, TResult>(IRegexVisitor<TLetter, TContext, TResult> visitor, TContext context) {
			return visitor.Accept(this, context);
		}

		protected override void WriteToInternal(RichTextWriter writer) {
			this.Inner.WriteTo(writer, this.EvaluationPrecedence);
			writer.Write("√");
			writer.Write("(");
			writer.Write(this.Symbol);
			writer.Write(")");
		}
	}
}
