using System;

using AgileObjects.ReadableExpressions;

using Sirius.Collections;
using Sirius.Unicode;

using Xunit;
using Xunit.Abstractions;

namespace Sirius.RegularExpressions.Parser {
	public class RegexLexerTest {
		private readonly ITestOutputHelper output;

		public RegexLexerTest(ITestOutputHelper output) {
			this.output = output;
		}

		[Fact]
		public void RegexStateMachine() {
			RegexLexer.CreateStateMachine(out var stateMachine, out _);
			this.output.WriteLine(stateMachine.ToReadableString());
		}

		[Fact]
		public void RegexTokenize() {
			var rx = RegexLexer.Create((symbol, data, index) => this.output.WriteLine("{0}({1}): {2}", symbol.ToString(RegexLexer.SymbolNameResolver), index, data.AsString()));
			rx.Push(@"this\sis\sa\stest".Append(Utf16Chars.EOF));
		}
	}
}
