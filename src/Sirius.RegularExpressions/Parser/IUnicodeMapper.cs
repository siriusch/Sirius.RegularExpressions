using System;

using Sirius.Collections;
using Sirius.RegularExpressions.Invariant;
using Sirius.Unicode;

namespace Sirius.RegularExpressions.Parser {
	public interface IUnicodeMapper<TLetter>
			where TLetter: IEquatable<TLetter>, IComparable<TLetter> {
		RxNode<TLetter> MapCodepoints(bool negate, RangeSet<Codepoint> codepointRanges, bool caseSensitive);

		RxNode<TLetter> MapGrapheme(Grapheme grapheme, bool caseSensitive);

		RangeSet<TLetter> ValidLetters {
			get;
		}
	}
}
