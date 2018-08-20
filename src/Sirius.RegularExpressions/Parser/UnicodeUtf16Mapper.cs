using System;
using System.Collections.Generic;
using System.Linq;

using Sirius.Collections;
using Sirius.RegularExpressions.Invariant;
using Sirius.Unicode;

namespace Sirius.RegularExpressions.Parser {
	public class UnicodeUtf16Mapper: UnicodeMapperBase<char> {
		public UnicodeUtf16Mapper(bool includeKForm, bool generateVariations): base(includeKForm, generateVariations) { }

		protected override RangeSet<char> ValidLetters => Utf16Chars.ValidFirstChar;

		protected override IEnumerable<char[]> GenerateCasedNormalizationLetterVariations(Grapheme grapheme, bool caseSensitive) {
			return this.GenerateCasedNormalizationCodepointVariations(grapheme, caseSensitive).Select(c => c.ToChars().ToArray());
		}

		public override RxNode<char> MapCodepoints(bool negate, RangeSet<Codepoint> codepointRanges, bool caseSensitive) {
			var singleCodepoints = new HashSet<char>();
			var compoundCodepoints = new HashSet<char[]>(ArrayContentEqualityComparer<char>.Default);
			var codepoints = codepointRanges.Expand();
			if (!caseSensitive) {
				codepoints = codepoints.SelectMany(this.GenerateCaseInsensitiveCodepoints);
			}
			foreach (var codepoint in codepoints) {
				if (codepoint.FitsIntoChar) {
					singleCodepoints.Add((char)codepoint);
				} else if (negate) {
					throw new InvalidOperationException("Cannot negate UTF16 characters requiring surrogates in the character set");
				} else {
					compoundCodepoints.Add(codepoint.ToChars());
				}
			}
			var result = compoundCodepoints.Select(l => l.Select(c => new RxMatch<char>(false, c)).JoinConcatenation());
			if (singleCodepoints.Count > 0) {
				result = result.Append(new RxMatch<char>(negate, singleCodepoints));
			}
			return result.JoinAlternation();
		}
	}
}
