using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Sirius.Collections;
using Sirius.Text;

namespace Sirius.RegularExpressions.Invariant {
	public sealed class RxMatch<TLetter>: RxNode<TLetter>
			where TLetter: IEquatable<TLetter>, IComparable<TLetter> {
		private static readonly Func<TLetter, string> letterToString = typeof(TLetter).IsPrimitive && typeof(TLetter) != typeof(char)
				? typeof(TLetter) == typeof(byte)
						? new Func<TLetter, string>(letter => $"({letter:x2})")
						: new Func<TLetter, string>(letter => $"({letter})")
				: letter => Regex.Escape(letter.ToString());

		private readonly RangeSet<TLetter> letters;

		public RxMatch(params TLetter[] entities): this((IEnumerable<TLetter>)entities) { }

		public RxMatch(IEnumerable<TLetter> entities): this(new RangeSet<TLetter>(entities.Condense())) {}

		public RxMatch(RangeSet<TLetter> entities) {
			this.letters = entities;
		}

		internal override int EvaluationPrecedence => 0;

		public RangeSet<TLetter> Letters => this.letters;

		public override void ComputeLengths(out int min, out int? max) {
			min = 1;
			max = 1;
		}

		protected override bool EqualsInternal(RxNode<TLetter> other) {
			return this.letters.Equals(((RxMatch<TLetter>)other).Letters);
		}

		public override int GetHashCode() {
			return this.Letters.Aggregate(typeof(RxMatch<TLetter>).GetHashCode(), (hash, letter) => hash^letter.GetHashCode());
		}

		public override TResult Visit<TContext, TResult>(IRegexVisitor<TLetter, TContext, TResult> visitor, TContext context) {
			return visitor.Match(this, context);
		}

		protected override void WriteToInternal(RichTextWriter writer) {
			switch (this.Letters.Expand().Take(2).Count()) {
			case 0:
				break;
			case 1:
				writer.Write(letterToString(this.Letters.Expand().Single()));
				break;
			default:
				writer.Write('[');
				foreach (var range in this.letters) {
					writer.Write(letterToString(range.From));
					if (!range.From.Equals(range.To)) {
						writer.Write('-');
						writer.Write(letterToString(range.To));
					}
				}
				writer.Write(']');
				break;
			}
		}
	}
}
