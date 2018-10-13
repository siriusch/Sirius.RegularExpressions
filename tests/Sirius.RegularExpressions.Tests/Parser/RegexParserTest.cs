using System;
using System.Threading;

using Sirius.Collections;
using Sirius.RegularExpressions.Automata;
using Sirius.Unicode;

using Xunit;
using Xunit.Abstractions;

namespace Sirius.RegularExpressions.Parser {
	public class RegexParserTest {
		private static readonly Lazy<Dfa<Codepoint>> acceptPrecedenceDfa = new Lazy<Dfa<Codepoint>>(() => {
			var expression = RegexAlternation.Create(
					RegexParser.Parse("abc", 1),
					RegexParser.Parse("abc+", 2),
					RegexParser.Parse("ab", 3),
					RegexParser.Parse("ab?c*", 4));
			var mapper = new UnicodeCodepointMapper(false, false);
			var invariant = expression.ToInvariant(mapper, new UnicodeCharSetProvider(name => new RangeSet<Codepoint>(name.ToCodepoints())), true);
			var nfa = NfaBuilder<Codepoint>.Build(invariant);
			return DfaBuilder<Codepoint>.Build(nfa, Codepoints.EOF);
		}, LazyThreadSafetyMode.PublicationOnly);

		private readonly ITestOutputHelper output;

		public RegexParserTest(ITestOutputHelper output) {
			this.output = output;
		}

		[Theory]
		[InlineData(1, @"abc")]
		[InlineData(2, @"abcc")]
		[InlineData(3, @"ab")]
		[InlineData(4, @"a")]
		public void RegexAcceptPrecedence(int expectedSymbol, string test) {
			var foundSymbol = default(SymbolId?);
			new Lexer<Codepoint>(acceptPrecedenceDfa.Value, (symbol, codepoints, index) => {
				if (symbol != SymbolId.Eof) {
					foundSymbol = symbol;
					this.output.WriteLine("{0} @ {1} => {2}", symbol, index, codepoints.AsString());
				}
			}).Push(test.ToCodepoints().Append(Codepoints.EOF));
			Assert.Equal<SymbolId>(expectedSymbol, foundSymbol.Value);
		}

		[Theory]
		[InlineData(@"\[^?({Regex Charset}|\\([x]{Hex Char}{2}|[u]{Hex Char}{4}|[Pp]\{{Letter}{AlphaNumeric}*\}|{Regex Printable}))+\]")]
		public void RegexParseOutput(string rx) {
			var expression = RegexParser.Parse(rx, null);
			var invariant = expression.ToInvariant(new UnicodeCodepointMapper(false, false), new UnicodeCharSetProvider(name => new RangeSet<Codepoint>(name.ToCodepoints())), true);
			this.output.WriteLine(invariant.ToString());
		}

		[Theory]
		[InlineData("a")]
		[InlineData("a?")]
		[InlineData("a+")]
		[InlineData("a*")]
		[InlineData("a{2,}")]
		[InlineData("a{1,2}")]
		[InlineData("a{3}")]
		[InlineData("ab")]
		[InlineData("a?b")]
		[InlineData("a+b")]
		[InlineData("a*b")]
		[InlineData("a|b|c")]
		[InlineData("[a-c]")]
		[InlineData("[a-c]|abc|cba")]
		[InlineData("ab?c*")]
		public void RegexParseRoundtrip(string rx) {
			var expression = RegexParser.Parse(rx, null);
			var invariant = expression.ToInvariant(new UnicodeCodepointMapper(false, false), new UnicodeCharSetProvider(), true);
			Assert.Equal(rx, invariant.ToString());
		}
	}
}
