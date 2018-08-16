using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Sirius.Collections;
using Sirius.RegularExpressions.Parser;
using Sirius.Unicode;

using Xunit;
using Xunit.Abstractions;

namespace Sirius.RegularExpressions.Alphabet {
	public class AlphabetBuilderTest {
		private readonly ITestOutputHelper output;

		public AlphabetBuilderTest(ITestOutputHelper output) {
			this.output = output;
		}

		private int Test<TLetter>(string pattern, bool caseSensitive, IUnicodeMapper<TLetter> mapper, TLetter? eof, RangeSet<TLetter> validRanges, out AlphabetBuilder<TLetter> builder)
				where TLetter: struct, IEquatable<TLetter>, IComparable<TLetter> {
			this.output.WriteLine("Input regex (Case Sensitive: {0}, EOF letter: {1}):", caseSensitive, eof.HasValue);
			this.output.WriteLine(pattern);
			var provider = new UnicodeCharSetProvider(null);
			var regex = RegexParser.Parse(pattern, null).ToInvariant(mapper, provider, caseSensitive);
			this.output.WriteLine("");
			this.output.WriteLine("{0} regex:", typeof(TLetter).Name);
			this.output.WriteLine(regex.ToString());
			builder = new AlphabetBuilder<TLetter>(regex, mapper.Negate, eof, validRanges);
			this.output.WriteLine("");
			this.output.WriteLine("Generated letter mapping:");
			foreach (var pair in builder.AlphabetById) {
				this.output.WriteLine("{0}: {1} ({2})", pair.Key, pair.Value, pair.Value.Count);
			}
			this.output.WriteLine("");
			this.output.WriteLine("Letter Regex:");
			this.output.WriteLine(builder.Expression.ToString());
			this.output.WriteLine("");
			this.output.WriteLine("Mapping function pseudocode:");
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
						this.output.WriteLine("}");
						inSwitch = false;
					}
					this.output.WriteLine("if ({0}) return {1}", string.Join(" ||" + Environment.NewLine + "    ", grouping.OrderBy(r => r.From).Select(r => r.From.CompareTo(r.To) == 0 ? $"(v == '{r.From}')" : $"(v >= '{r.From}' && v <= '{r.To}')")), grouping.Key.LetterId);
				} else {
					if (!inSwitch) {
						this.output.WriteLine("switch (v) {");
						inSwitch = true;
					}
					this.output.WriteLine("{0}" + Environment.NewLine + "        return {1}", string.Join(Environment.NewLine, grouping.SelectMany(g => g.Expand()).OrderBy(r => r).Select(r => $"    case '{r}':")), grouping.Key.LetterId);
				}
			}
			if (inSwitch) {
				this.output.WriteLine("}");
			}
			return builder.AlphabetById.Count;
		}

		[Theory]
		[InlineData(@"[a-z0-9äöü]|Test", 6, false, true, new[] {2, 5, 3, 4, 5}, new[] {'Ä', 'T', 'E', 'S', 'T'})]
		[InlineData(@"[a-z0-9äöü]|Test", 7, true, true, new[] {2, 3, 4, 5, 6}, new[] {'ä', 'T', 'e', 's', 't'})]
		[InlineData(@"[a-z0-9]", 3, false, false, new[] {2, 2, 2, 2, 1}, new[] {'0', '9', 'A', 'Z', '?'})]
		[InlineData(@"\U0001F78B", 4, false, true, new[] {1, 2, 3}, new[] {' ', '\ud83d', '\udf8b'})]
		public void Utf16Test(string regex, int expectedLetterCount, bool caseSensitive, bool eof, int[] expectLetterIds, char[] testChars) {
			Debug.Assert(expectLetterIds.Length == testChars.Length);
			AlphabetBuilder<char> builder;
			Assert.Equal(expectedLetterCount, this.Test(regex, caseSensitive, new UnicodeUtf16Mapper(false, false), eof ? Utf16Chars.EOF : default(char?), Utf16Chars.ValidAll, out builder));
			var expression = AlphabetMapperEmitter<char>.CreateExpression(builder);
			this.output.WriteLine("");
			this.output.WriteLine("Expression:");
			this.output.WriteLine(expression.Body.ToString());
			var func = expression.Compile();
			Assert.Throws<InvalidOperationException>(() => func('\uFFFE'));
			if (eof) {
				Assert.Equal(new LetterId(0), func(Utf16Chars.EOF));
			} else {
				Assert.Throws<InvalidOperationException>(() => func(Utf16Chars.EOF));
			}
			for (var i = 0; i < testChars.Length; i++) {
				Assert.Equal(new LetterId(expectLetterIds[i]), func(testChars[i]));
			}
		}

		[Theory]
		[InlineData(@"[a-z0-9äöü]|Test", 6, false, true, new[] {2, 5, 3, 4, 5}, new int[] {'Ä', 'T', 'E', 'S', 'T'})]
		[InlineData(@"[a-z0-9äöü]|Test", 7, true, true, new[] {2, 3, 4, 5, 6}, new int[] {'ä', 'T', 'e', 's', 't'})]
		[InlineData(@"[a-z0-9]", 3, false, false, new[] {2, 2, 2, 2, 1}, new int[] {'0', '9', 'A', 'Z', '?'})]
		[InlineData(@"\U0001F78B", 3, false, true, new[] {1, 2}, new int[] {' ', 0x01F78B})]
		public void Utf32Test(string regex, int expectedLetters, bool caseSensitive, bool eof, int[] expectLetterIds, int[] testChars) {
			AlphabetBuilder<Codepoint> builder;
			Assert.Equal(expectedLetters, this.Test(regex, caseSensitive, new UnicodeCodepointMapper(false, false), eof ? Codepoints.EOF : default(Codepoint?), Codepoints.Valid, out builder));
			var expression = AlphabetMapperEmitter<Codepoint>.CreateExpression(builder);
			this.output.WriteLine("");
			this.output.WriteLine("Expression:");
			this.output.WriteLine(expression.Body.ToString());
			var func = expression.Compile();
			Assert.Throws<InvalidOperationException>(() => func((Codepoint)'\uFFFE'));
			if (eof) {
				Assert.Equal(new LetterId(0), func(Codepoints.EOF));
			} else {
				Assert.Throws<InvalidOperationException>(() => func(Codepoints.EOF));
			}
			for (var i = 0; i < testChars.Length; i++) {
				Assert.Equal(new LetterId(expectLetterIds[i]), func((Codepoint)testChars[i]));
			}
		}

		[Theory]
		[InlineData(@"[a-z0-9äöü]|Test", 13, false, true, new[] {1, 5, 3, 4, 5}, new byte[] {(byte)'?', (byte)'T', (byte)'E', (byte)'S', (byte)'T'})]
		[InlineData(@"[a-z0-9äöü]|Test", 11, true, true, new[] {1, 3, 4, 5, 6}, new byte[] {(byte)'?', (byte)'T', (byte)'e', (byte)'s', (byte)'t'})]
		[InlineData(@"[a-z0-9]", 3, false, false, new[] {2, 2, 2, 2, 1}, new byte[] {(byte)'0', (byte)'9', (byte)'A', (byte)'Z', (byte)'?'})]
		[InlineData(@"\U0001F78B", 6, false, true, new[] {1, 2}, new byte[] {32, 139})]
		public void Utf8Test(string regex, int expectedLetters, bool caseSensitive, bool eof, int[] expectLetterIds, byte[] testChars) {
			AlphabetBuilder<byte> builder;
			Assert.Equal(expectedLetters, this.Test(regex, caseSensitive, new UnicodeUtf8Mapper(false, false), eof ? Utf8Bytes.EOF : default(byte?), Utf8Bytes.ValidAll, out builder));
			var expression = AlphabetMapperEmitter<byte>.CreateExpression(builder);
			this.output.WriteLine("");
			this.output.WriteLine("Expression:");
			this.output.WriteLine(expression.Body.ToString());
			var func = expression.Compile();
			Assert.Throws<InvalidOperationException>(() => func(254));
			if (eof) {
				Assert.Equal(new LetterId(0), func(Utf8Bytes.EOF));
			} else {
				Assert.Throws<InvalidOperationException>(() => func(Utf8Bytes.EOF));
			}
			for (var i = 0; i < testChars.Length; i++) {
				Assert.Equal(new LetterId(expectLetterIds[i]), func(testChars[i]));
			}
		}
	}
}
