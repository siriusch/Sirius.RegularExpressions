using System;
using System.Collections.Generic;

using Sirius.RegularExpressions.Invariant;

namespace Sirius.RegularExpressions.Alphabet {
	internal class LetterRangeUser<TLetter>
			where TLetter: IEquatable<TLetter>, IComparable<TLetter> {
		public static IEnumerable<LetterRangeUser<TLetter>> Enumerate(LetterRangeUser<TLetter> letterRangeUser) {
			while (letterRangeUser != null) {
				yield return letterRangeUser;
				letterRangeUser = letterRangeUser.Next;
			}
		}

		public LetterRangeUser(Id<RxMatch<TLetter>> user, LetterRangeUser<TLetter> next) {
			this.Next = next;
			this.User = user;
		}

		public LetterRangeUser<TLetter> Next {
			get;
		}

		public Id<RxMatch<TLetter>> User {
			get;
		}
	}
}
