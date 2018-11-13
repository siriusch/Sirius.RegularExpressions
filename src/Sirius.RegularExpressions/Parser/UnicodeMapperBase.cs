using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Sirius.Collections;
using Sirius.RegularExpressions.Invariant;
using Sirius.Unicode;

namespace Sirius.RegularExpressions.Parser {
	public abstract class UnicodeMapperBase<TLetter>: IUnicodeMapper<TLetter>
			where TLetter: IEquatable<TLetter>, IComparable<TLetter> {
		private static IEnumerable<Codepoint[]> GenerateCodepointVariations(Codepoint[] current, IList<Codepoint> pending) {
			if (pending.Count == 0) {
				return new[] { current };
			}
			var result = Enumerable.Empty<Codepoint[]>();
			for (var i = 0; i < pending.Count; i++) {
				result = result.Concat(GenerateCodepointVariations(current.Append(pending[i]).ToArray(), pending.ExceptAt(i).ToList()));
			}
			return result;
		}

		private readonly bool generateVariations;
		private readonly bool includeKForm;

		public UnicodeMapperBase(bool includeKForm, bool generateVariations) {
			this.includeKForm = includeKForm;
			this.generateVariations = generateVariations;
		}

		public abstract RangeSet<TLetter> ValidLetters {
			get;
		}

		public RxNode<TLetter> MapCodepoints(bool negate, RangeSet<Codepoint> codepointRanges, bool caseSensitive) {
			if (!caseSensitive) {
				codepointRanges = new RangeSet<Codepoint>(codepointRanges.Expand().SelectMany(this.GenerateCaseInsensitiveCodepoints).Condense());
			}
			if (negate) {
				codepointRanges = Codepoints.Valid - codepointRanges;
			}
			if (codepointRanges.Count == 0) {
				return RxEmpty<TLetter>.Default;
			}
			return MapCodepoints(codepointRanges);
		}

		protected abstract RxNode<TLetter> MapCodepoints(RangeSet<Codepoint> codepointRanges);

		public RxNode<TLetter> MapGrapheme(Grapheme grapheme, bool caseSensitive) {
			var singleCodepoints = new HashSet<TLetter>();
			var compoundCodepoints = new HashSet<TLetter[]>(ArrayContentEqualityComparer<TLetter>.Default);
			foreach (var letterCodepoints in this.GenerateCasedNormalizationLetterVariations(grapheme, caseSensitive)) {
				Debug.Assert(letterCodepoints.Length > 0);
				if (letterCodepoints.Length == 1) {
					singleCodepoints.Add(letterCodepoints[0]);
				} else {
					compoundCodepoints.Add(letterCodepoints);
				}
			}
			return compoundCodepoints
				.Select(l => l.Select(c => new RxMatch<TLetter>(c)).JoinConcatenation())
				.Append(new RxMatch<TLetter>(singleCodepoints))
				.JoinAlternation();
		}

		protected IEnumerable<Codepoint[]> GenerateCasedNormalizationCodepointVariations(Grapheme grapheme, bool caseSensitive) {
			return caseSensitive
					? this.GenerateNormalizationCodepointVariations(grapheme)
					: this.GenerateNormalizationCodepointVariations(grapheme.ToLowerInvariant())
							.Concat(this.GenerateNormalizationCodepointVariations(grapheme.ToUpperInvariant()));
		}

		protected abstract IEnumerable<TLetter[]> GenerateCasedNormalizationLetterVariations(Grapheme grapheme, bool caseSensitive);

		protected IEnumerable<Codepoint> GenerateCaseInsensitiveCodepoints(Codepoint codepoint) {
			if (codepoint.FitsIntoChar) {
				var orig = (char)codepoint;
				yield return orig;
				var upper = char.ToUpperInvariant(orig);
				if (upper != orig) {
					yield return upper;
				}
				var lower = char.ToLowerInvariant(orig);
				if (upper != lower) {
					yield return lower;
				}
			} else {
				yield return codepoint;
			}
		}

		private IEnumerable<Codepoint[]> GenerateCodepointVariations(IEnumerable<Codepoint> letter) {
			if (!this.generateVariations) {
				return letter.ToArray().Yield();
			}
			using (var enumerator = letter.GetEnumerator()) {
				if (!enumerator.MoveNext()) {
					throw new InvalidOperationException("Empty letter");
				}
				var basechar = new[] { enumerator.Current };
				var marks = new List<Codepoint>();
				while (enumerator.MoveNext()) {
					marks.Add(enumerator.Current);
				}
				return GenerateCodepointVariations(basechar, marks);
			}
		}

		private IEnumerable<Codepoint[]> GenerateNormalizationCodepointVariations(Grapheme grapheme) {
			var result = this.GenerateCodepointVariations(grapheme.Normalize(NormalizationForm.FormC).ToArray())
				.Concat(this.GenerateCodepointVariations(grapheme.Normalize(NormalizationForm.FormD).ToArray()));
			if (this.includeKForm) {
				result = result
					.Concat(this.GenerateCodepointVariations(grapheme.Normalize(NormalizationForm.FormKC).ToArray()))
					.Concat(this.GenerateCodepointVariations(grapheme.Normalize(NormalizationForm.FormKD).ToArray()));
			}
			return result;
		}
	}
}
