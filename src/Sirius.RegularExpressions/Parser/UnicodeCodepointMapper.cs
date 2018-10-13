using System;
using System.Collections.Generic;

using Sirius.Collections;
using Sirius.RegularExpressions.Invariant;
using Sirius.Unicode;

namespace Sirius.RegularExpressions.Parser {
	public class UnicodeCodepointMapper: UnicodeMapperBase<Codepoint> {
		public UnicodeCodepointMapper(bool includeKForm, bool generateVariations): base(includeKForm, generateVariations) { }

		public override RangeSet<Codepoint> ValidLetters => Codepoints.Valid;

		protected override IEnumerable<Codepoint[]> GenerateCasedNormalizationLetterVariations(Grapheme grapheme, bool caseSensitive) {
			return this.GenerateCasedNormalizationCodepointVariations(grapheme, caseSensitive);
		}

		protected override RxNode<Codepoint> MapCodepoints(RangeSet<Codepoint> codepointRanges) {
			return new RxMatch<Codepoint>(codepointRanges);
		}
	}
}
