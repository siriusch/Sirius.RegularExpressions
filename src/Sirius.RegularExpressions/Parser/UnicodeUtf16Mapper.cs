using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Sirius.Collections;
using Sirius.RegularExpressions.Invariant;
using Sirius.Unicode;

namespace Sirius.RegularExpressions.Parser {
	public class UnicodeUtf16Mapper: UnicodeMapperBase<char> {
		private static readonly RangeSet<Codepoint> SingleChar = Range<Codepoint>.Create(char.MinValue, '\xD7FF') | Range<Codepoint>.Create('\xE000', '�');

		private static IEnumerable<RxNode<char>> MakeSingleRange(RangeSet<Codepoint> ranges) {
			if (ranges.Count > 0) {
				return new RxMatch<char>(new RangeSet<char>(ranges.Select(r => Range<char>.Create((char)r.From, (char)r.To)))).Yield();
			}
			return Enumerable.Empty<RxNode<char>>();
		}

		private static IEnumerable<RxNode<char>> MakeSurrogateRanges(Range<Codepoint> range) {
			var rangeFrom = range.From.ToInt32() - 0x10000;
			var rangeTo = range.To.ToInt32() - 0x10000;
			Debug.Assert((rangeFrom >= 0) && (rangeTo >= 0));
			var fromL = (char)((rangeFrom & 0x3ff) | 0xdc00);
			var toL = (char)((rangeTo & 0x3ff) | 0xdc00);
			var fromH = (char)((rangeFrom >> 10) | 0xd800);
			var toH = (char)((rangeTo >> 10) | 0xd800);
			if ((fromH < toH) && (fromL > 0xdc00)) {
				yield return new RxConcatenation<char>(new RxMatch<char>(Range<char>.Create(fromL, '\xDFFF')), new RxMatch<char>(Range<char>.Create(fromH++)));
				fromL = '\xDC00';
			}
			if ((fromH < toH) && (toL < 0xdbff)) {
				yield return new RxConcatenation<char>(new RxMatch<char>(Range<char>.Create('\xDC00', toL)), new RxMatch<char>(Range<char>.Create(toH--)));
				toL = '\xDFFF';
			}
			yield return new RxConcatenation<char>(new RxMatch<char>(Range<char>.Create(fromL, toL)), new RxMatch<char>(Range<char>.Create(fromH, toH)));
		}

		public UnicodeUtf16Mapper(bool includeKForm, bool generateVariations): base(includeKForm, generateVariations) { }

		public override RangeSet<char> ValidLetters => Utf16Chars.ValidAll;

		protected override IEnumerable<char[]> GenerateCasedNormalizationLetterVariations(Grapheme grapheme, bool caseSensitive) {
			return this.GenerateCasedNormalizationCodepointVariations(grapheme, caseSensitive).Select(c => c.ToChars().ToArray());
		}

		protected override RxNode<char> MapCodepoints(RangeSet<Codepoint> codepointRanges) {
			return MakeSingleRange(codepointRanges & SingleChar)
					.Concat((codepointRanges - SingleChar).SelectMany(MakeSurrogateRanges))
					.JoinAlternation();
		}
	}
}
