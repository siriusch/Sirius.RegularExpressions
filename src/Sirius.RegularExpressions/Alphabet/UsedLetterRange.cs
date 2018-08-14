using System;
using System.Linq;
using System.Xml;

using Sirius.Collections;
using Sirius.RegularExpressions.Invariant;

namespace Sirius.RegularExpressions.Alphabet {
	internal struct UsedLetterRange<TLetter>
			where TLetter: IEquatable<TLetter>, IComparable<TLetter> {
		public UsedLetterRange(TLetter from, TLetter to, LetterRangeUser<TLetter> users): this(Range<TLetter>.Create(from, to), users) { }

		public UsedLetterRange(Range<TLetter> range, LetterRangeUser<TLetter> users) {
			this.Range = range;
			this.Users = users;
		}

		public TLetter From => this.Range.From;

		public TLetter To => this.Range.To;

		public Range<TLetter> Range {
			get;
		}

		public LetterRangeUser<TLetter> Users {
			get;
		}

		public UsedLetterRange<TLetter> AddUser(Id<RxMatch<TLetter>> user) {
			return new UsedLetterRange<TLetter>(this.Range, new LetterRangeUser<TLetter>(user, this.Users));
		}

		public string GetUsersKey() {
			return '['+string.Join("|", LetterRangeUser<TLetter>.Enumerate(this.Users).Select(r => r.User.ToInt32()).OrderBy(r => r).Select(XmlConvert.ToString))+']';
		}
	}
}
