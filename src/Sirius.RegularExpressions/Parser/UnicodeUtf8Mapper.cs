using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Sirius.Collections;
using Sirius.RegularExpressions.Invariant;
using Sirius.Unicode;

namespace Sirius.RegularExpressions.Parser {
	public class UnicodeUtf8Mapper: UnicodeMapperBase<byte> {
		public UnicodeUtf8Mapper(bool includeKForm, bool generateVariations): base(includeKForm, generateVariations) { }

		protected override RangeSet<byte> ValidLetters => Utf8Bytes.ValidFirstByte;

		public override RxNode<byte> MapCodepoints(bool negate, RangeSet<Codepoint> codepointRanges, bool caseSensitive) {
			var singleCodepoints = new HashSet<byte>();
			var compoundCodepoints = new HashSet<byte[]>(ArrayContentEqualityComparer<byte>.Default);
			var codepoints = codepointRanges.Expand();
			if (!caseSensitive) {
				codepoints = codepoints.SelectMany(this.GenerateCaseInsensitiveCodepoints);
			}
			foreach (var letterCodepoints in codepoints.Select(c => c.ToUtf8Bytes())) {
				Debug.Assert(letterCodepoints.Length > 0);
				if (letterCodepoints.Length == 1) {
					singleCodepoints.Add(letterCodepoints[0]);
				} else if (negate) {
					throw new InvalidOperationException("Cannot negate UTF8 multi-byte unicode characters in the character set");
				} else {
					compoundCodepoints.Add(letterCodepoints);
				}
			}
			var result = compoundCodepoints.Select(l => l.Select(c => new RxMatch<byte>(false, c)).JoinConcatenation());
			if (singleCodepoints.Count > 0) {
				result = result.Append(new RxMatch<byte>(negate, singleCodepoints));
			}
			return result.JoinAlternation();
		}

		protected override IEnumerable<byte[]> GenerateCasedNormalizationLetterVariations(Grapheme grapheme, bool caseSensitive) {
			return this.GenerateCasedNormalizationCodepointVariations(grapheme, caseSensitive).Select(c => c.ToUtf8Bytes().ToArray());
		}
	}
}