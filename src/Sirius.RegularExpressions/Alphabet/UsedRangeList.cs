using System;
using System.Collections.Generic;
using System.Linq;

using Sirius.Collections;

namespace Sirius.RegularExpressions.Alphabet {
	internal class UsedRangeList<TLetter>: List<UsedLetterRange<TLetter>>, IRangeSet<TLetter>
			where TLetter: IComparable<TLetter>, IEquatable<TLetter> {
		IEnumerator<Range<TLetter>> IEnumerable<Range<TLetter>>.GetEnumerator() {
			return this.Select<UsedLetterRange<TLetter>, Range<TLetter>>(r => r.Range).GetEnumerator();
		}

		Range<TLetter> IReadOnlyList<Range<TLetter>>.this[int index] => this[index].Range;
	}
}
