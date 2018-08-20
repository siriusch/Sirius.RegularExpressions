using System;
using System.Collections.Generic;

using Sirius.Collections;
using Sirius.RegularExpressions.Invariant;
using Sirius.Unicode;

namespace Sirius.RegularExpressions.Parser {
	public class UnicodeCodepointMapper: UnicodeMapperBase<Codepoint> {
		public UnicodeCodepointMapper(bool includeKForm, bool generateVariations): base(includeKForm, generateVariations) { }

		protected override RangeSet<Codepoint> ValidLetters => Codepoints.Valid;

		protected override IEnumerable<Codepoint[]> GenerateCasedNormalizationLetterVariations(Grapheme grapheme, bool caseSensitive) {
			return this.GenerateCasedNormalizationCodepointVariations(grapheme, caseSensitive);
		}

		public override RxNode<Codepoint> MapCodepoints(bool negate, RangeSet<Codepoint> codepointRanges, bool caseSensitive) {
			return new RxMatch<Codepoint>(negate, this.ExpandCodepoints(codepointRanges, caseSensitive));
		}
	}
}
