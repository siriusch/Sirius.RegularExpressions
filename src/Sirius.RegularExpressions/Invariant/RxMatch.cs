using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using bsn.GoldParser.Text;

namespace Sirius.RegularExpressions.Invariant {
	public sealed class RxMatch<TLetter>: RxNode<TLetter>
			where TLetter: IEquatable<TLetter> {
		private static readonly Func<TLetter, string> letterToString = typeof(TLetter).IsPrimitive && typeof(TLetter) != typeof(char)
				? typeof(TLetter) == typeof(byte)
						? new Func<TLetter, string>(letter => $"({letter:x2})")
						: new Func<TLetter, string>(letter => $"({letter})")
				: letter => Regex.Escape(letter.ToString());

		private readonly HashSet<TLetter> letters;

		public RxMatch(bool negate, params TLetter[] entities): this(negate, (IEnumerable<TLetter>)entities) { }

		public RxMatch(bool negate, IEnumerable<TLetter> entities) {
			this.Negate = negate;
			this.letters = new HashSet<TLetter>(entities);
		}

		internal override int PrecedenceLevel => 0;

		public ICollection<TLetter> Letters => this.letters;

		public bool Negate {
			get;
		}

		public override void ComputeLengths(out int min, out int? max) {
			min = 1;
			max = 1;
		}

		protected override bool EqualsInternal(RxNode<TLetter> other) {
			return this.letters.SetEquals(((RxMatch<TLetter>)other).Letters);
		}

		public override int GetHashCode() {
			return this.Letters.Aggregate(typeof(RxMatch<TLetter>).GetHashCode(), (hash, letter) => hash^letter.GetHashCode());
		}

		public override TResult Visit<TContext, TResult>(IRegexVisitor<TLetter, TContext, TResult> visitor, TContext context) {
			return visitor.Match(this, context);
		}

		protected override void WriteToInternal(RichTextWriter writer) {
			switch (this.Letters.Count) {
			case 0:
				break;
			case 1:
				if (this.Negate) {
					goto default;
				}
				writer.Write(letterToString(this.Letters.Single()));
				break;
			default:
				writer.Write('[');
				if (this.Negate) {
					writer.Write("^");
				}
				writer.Write(string.Join("", this.Letters.Select(letterToString)));
				writer.Write(']');
				break;
			}
		}
	}
}
