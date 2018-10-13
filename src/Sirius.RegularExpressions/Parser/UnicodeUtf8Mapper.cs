using System;
using System.Collections.Generic;
using System.Linq;

using Sirius.Collections;
using Sirius.RegularExpressions.Invariant;
using Sirius.Unicode;

namespace Sirius.RegularExpressions.Parser {
	public class UnicodeUtf8Mapper: UnicodeMapperBase<byte> {
		private static readonly Range<int> SingleByte = Range<int>.Create(0x000000, 0x00007f);
		private static readonly Range<int> DoubleByte = Range<int>.Create(0x000080, 0x0007ff);
		private static readonly Range<int> TripleByte = Range<int>.Create(0x000800, 0x00ffff);
		private static readonly Range<int> QuadripleByte = Range<int>.Create(0x010000, 0x10ffff);

		private static byte AdditionalUtf8Byte(int value) {
			return (byte)(0x80 | value);
		}

		private static IEnumerable<KeyValuePair<Range<int>, RxNode<byte>>> MakeSixBitRanges(Range<int> range) {
			var fromL = range.From & 0x3f;
			var toL = range.To & 0x3f;
			var fromH = (range.From >> 6);
			var toH = (range.To >> 6);
			if ((fromH < toH) && (fromL > 0)) {
				yield return new KeyValuePair<Range<int>, RxNode<byte>>(Range<int>.Create(fromH), new RxMatch<byte>(Range<byte>.Create(AdditionalUtf8Byte(fromL), 0x3f|0x80)));
				fromL = 0;
				fromH = (byte)(fromH + 1);
			}
			if ((fromH < toH) && (toL < 0x3f)) {
				yield return new KeyValuePair<Range<int>, RxNode<byte>>(Range<int>.Create(toH), new RxMatch<byte>(Range<byte>.Create(0x80, AdditionalUtf8Byte(toL))));
				toL = 0x3f;
				toH = (byte)(toH - 1);
			}
			yield return new KeyValuePair<Range<int>, RxNode<byte>>(Range<int>.Create(fromH, toH), new RxMatch<byte>(Range<byte>.Create(AdditionalUtf8Byte(fromL), AdditionalUtf8Byte(toL))));
		}

		private static Range<int> IntRange(Range<Codepoint> range) {
			return Range<int>.Create(range.From.ToInt32(), range.To.ToInt32());
		}

		private static IEnumerable<RxNode<byte>> MakeSingleRange(RangeSet<int> ranges) {
			if (ranges.Count > 0) {
				return new RxMatch<byte>(new RangeSet<byte>(ranges.Select(range => Range<byte>.Create((byte)range.From, (byte)range.To)))).Yield();
			}
			return Enumerable.Empty<RxMatch<byte>>();
		}

		private static IEnumerable<RxNode<byte>> MakeTwoRanges(Range<int> range, byte bits) {
			return MakeSixBitRanges(range).Select(p => new RxConcatenation<byte>(new RxMatch<byte>(Range<byte>.Create((byte)(p.Key.From|bits), (byte)(p.Key.To|bits))), p.Value));
		}

		private static IEnumerable<RxNode<byte>> MakeThreeRanges(Range<int> range, byte bits) {
			return MakeSixBitRanges(range).SelectMany(p => MakeTwoRanges(p.Key, bits).Select(r => new RxConcatenation<byte>(r, p.Value)));
		}

		private static IEnumerable<RxNode<byte>> MakeFourRanges(Range<int> range, byte bits) {
			return MakeSixBitRanges(range).SelectMany(p => MakeThreeRanges(p.Key, bits).Select(r => new RxConcatenation<byte>(r, p.Value)));
		}

		public UnicodeUtf8Mapper(bool includeKForm, bool generateVariations): base(includeKForm, generateVariations) { }

		public override RangeSet<byte> ValidLetters => Utf8Bytes.ValidAll;

		protected override IEnumerable<byte[]> GenerateCasedNormalizationLetterVariations(Grapheme grapheme, bool caseSensitive) {
			return this.GenerateCasedNormalizationCodepointVariations(grapheme, caseSensitive).Select(c => c.ToUtf8Bytes().ToArray());
		}

		private static RangeSet<int> Intersect(RangeSet<Codepoint> codepointRanges, Range<int> intRange) {
			return new RangeSet<int>(codepointRanges.Select(IntRange)) & intRange;
		}

		protected override RxNode<byte> MapCodepoints(RangeSet<Codepoint> codepointRanges) {
			return MakeSingleRange(Intersect(codepointRanges, SingleByte))
					.Concat(Intersect(codepointRanges, DoubleByte).SelectMany(r => MakeTwoRanges(r, 0xC0)))
					.Concat(Intersect(codepointRanges, TripleByte).SelectMany(r => MakeThreeRanges(r, 0xE0)))
					.Concat(Intersect(codepointRanges, QuadripleByte).SelectMany(r => MakeFourRanges(r, 0xF0)))
					.JoinAlternation();
		}
	}
}
