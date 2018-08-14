using System;
using System.Collections.Generic;
using System.Linq;

using Sirius.Collections;
using Sirius.RegularExpressions.Parser;
using Sirius.Unicode;

using Xunit;
using Xunit.Abstractions;

namespace Sirius.RegularExpressions.Alphabet {
	public class AlphabetBuilderTest {
		private readonly ITestOutputHelper helper;

		public AlphabetBuilderTest(ITestOutputHelper helper) {
			this.helper = helper;
		}

		private int Test<TLetter>(string pattern, bool caseSensitive, IUnicodeMapper<TLetter> mapper, TLetter? eof, RangeSet<TLetter> validRanges) where TLetter: struct, IEquatable<TLetter>, IComparable<TLetter> {
			this.helper.WriteLine("Input regex (Case Sensitive: {0}, EOF letter: {1}):", caseSensitive, eof.HasValue);
			this.helper.WriteLine(pattern);
			var provider = new UnicodeCharSetProvider(null);
			var regex = RegexParser.Parse(pattern, null).ToInvariant(mapper, provider, caseSensitive);
			this.helper.WriteLine("");
			this.helper.WriteLine("{0} regex:", typeof(TLetter).Name);
			this.helper.WriteLine(regex.ToString());
			var builder = new AlphabetBuilder<TLetter>(regex, mapper.Negate, eof, validRanges);
			this.helper.WriteLine("");
			this.helper.WriteLine("Generated letter mapping:");
			foreach (var pair in builder.AlphabetById) {
				this.helper.WriteLine("{0}: {1} ({2})", pair.Key, pair.Value, pair.Value.Count);
			}
			this.helper.WriteLine("");
			this.helper.WriteLine("Letter Regex:");
			this.helper.WriteLine(builder.Expression.ToString());
			this.helper.WriteLine("");
			this.helper.WriteLine("Mapping function pseudocode:");
			var inSwitch = false;
			foreach (var grouping in builder
				.AlphabetById
				.SelectMany(p => p.Value.Select(r => new KeyValuePair<Range<TLetter>, LetterId>(r, p.Key)))
				.GroupBy(p => new {
						Range = (!typeof(TLetter).IsPrimitive) || p.Key.Expand().Skip(2).Any(),
						LetterId = p.Value
					}, p => p.Key)
				.OrderBy(p => p.Key.Range)
				.ThenBy(p => p.Key.LetterId)) {
				if (grouping.Key.Range) {
					if (inSwitch) {
						this.helper.WriteLine("}");
						inSwitch = false;
					}
					this.helper.WriteLine("if ({0}) return {1}", string.Join(" ||"+Environment.NewLine+"    ", grouping.OrderBy(r => r.From).Select(r => r.From.CompareTo(r.To) == 0 ? $"(v == '{r.From}')" : $"(v >= '{r.From}' && v <= '{r.To}')")), grouping.Key.LetterId);
				} else {
					if (!inSwitch) {
						this.helper.WriteLine("switch (v) {");
						inSwitch = true;
					}
					this.helper.WriteLine("{0}"+Environment.NewLine+"        return {1}", string.Join(Environment.NewLine, grouping.SelectMany(g => g.Expand()).OrderBy(r => r).Select(r => $"    case '{r}':")), grouping.Key.LetterId);
				}
			}
			if (inSwitch) {
				this.helper.WriteLine("}");
			}
			return builder.AlphabetById.Count;
		}

		[Theory]
		[InlineData(@"[a-z0-9äöü]|Test", 6, false, true)]
		[InlineData(@"[a-z0-9äöü]|Test", 7, true, true)]
		[InlineData(@"[a-z0-9]", 3, false, false)]
		[InlineData(@"\U0001F78B", 4, false, true)]
		public void Utf16Test(string regex, int expectedLetters, bool caseSensitive, bool eof) {
			Assert.Equal(expectedLetters, Test(regex, caseSensitive, new UnicodeUtf16Mapper(false, false), eof ? Utf16Chars.EOF : default(char?), Utf16Chars.ValidAll));
		}

		[Theory]
		[InlineData(@"[a-z0-9äöü]|Test", 6, false, true)]
		[InlineData(@"[a-z0-9äöü]|Test", 7, true, true)]
		[InlineData(@"[a-z0-9]", 3, false, false)]
		[InlineData(@"\U0001F78B", 3, false, true)]
		public void Utf32Test(string regex, int expectedLetters, bool caseSensitive, bool eof) {
			Assert.Equal(expectedLetters, Test(regex, caseSensitive, new UnicodeCodepointMapper(false, false), eof ? Codepoints.EOF : default(Codepoint?), Codepoints.Valid));
		}

		[Theory]
		[InlineData(@"[a-z0-9äöü]|Test", 13, false, true)]
		[InlineData(@"[a-z0-9äöü]|Test", 11, true, true)]
		[InlineData(@"[a-z0-9]", 3, false, false)]
		[InlineData(@"\U0001F78B", 6, false, true)]
		public void Utf8Test(string regex, int expectedLetters, bool caseSensitive, bool eof) {
			Assert.Equal(expectedLetters, Test(regex, caseSensitive, new UnicodeUtf8Mapper(false, false), eof ? Utf8Bytes.EOF : default(byte?), Utf8Bytes.ValidAll));
		}
	}
}
