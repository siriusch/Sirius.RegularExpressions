using System;

using Sirius.Collections;
using Sirius.RegularExpressions.Parser;
using Sirius.Unicode;

using Xunit;
using Xunit.Abstractions;

namespace Sirius.RegularExpressions.RegularExpressions.Parser {
	public class RegexParserTest {
		private readonly ITestOutputHelper output;

		public RegexParserTest(ITestOutputHelper output) {
			this.output = output;
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
		[InlineData("[abc]")]
		public void RegexParseRoundtrip(string rx) {
			var expression = RegexParser.Parse(rx, null);
			var invariant = expression.ToInvariant(new UnicodeCodepointMapper(false, false), new UnicodeCharSetProvider(null), true);
			Assert.Equal(rx, invariant.ToString());
		}

		[Fact]
		public void RegexSemanticBinding() {
			RegexParser.Actions.Initialize(true);
		}
	}
}
