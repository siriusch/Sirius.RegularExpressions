using System;

using bsn.GoldParser.Text;

namespace Sirius.RegularExpressions.Invariant {
	public sealed class RxAccept<TLetter>: RxNode<TLetter>
			where TLetter: IEquatable<TLetter> {
		public static RxAccept<TLetter> Create(RxNode<TLetter> inner, SymbolId symbol) {
			if (inner.IsEmpty()) {
				throw new ArgumentException("Zero-length accept", nameof(inner));
			}
			return new RxAccept<TLetter>(inner, symbol);
		}

		public RxAccept(RxNode<TLetter> inner, SymbolId symbol = default(SymbolId)) {
			this.Symbol = symbol;
			this.Inner = inner;
		}

		public RxNode<TLetter> Inner {
			get;
		}

		public SymbolId Symbol {
			get;
		}

		internal override int PrecedenceLevel => this.Inner.PrecedenceLevel;

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
			this.Inner.WriteTo(writer, this.PrecedenceLevel);
			writer.Write("√");
			writer.Write("(");
			writer.Write(this.Symbol);
			writer.Write(")");
		}
	}
}
