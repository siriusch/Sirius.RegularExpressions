using System;
using System.Diagnostics;

using AgileObjects.ReadableExpressions;

using Sirius.Collections;
using Sirius.RegularExpressions.Automata;
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
			// Initialize unicode-related classes to avoid having their cost in the time computation
			var sw = Stopwatch.StartNew();
			var provider = new UnicodeCharSetProvider();
			provider.GetClassSet(CharSetClass.Digit);
			provider.GetClassSet(CharSetClass.Dot);
			provider.GetClassSet(CharSetClass.Space);
			provider.GetClassSet(CharSetClass.Word);
			UnicodeRanges.FromUnicodeName("InCombining_Diacritical_Marks");
			sw.Stop();
			this.output.WriteLine("Unicode init ms: " + sw.ElapsedMilliseconds);
			sw = Stopwatch.StartNew();
			RegexLexer.CreateStateMachine(out var stateMachine, out var startStateId);
			var lapBuild = sw.ElapsedTicks;
			var sm = stateMachine.Compile();
			var lapCompile = sw.ElapsedTicks;
			var startState = new Id<DfaState<LetterId>>(startStateId);
			sm(ref startState, ' ');
			var lapJit = sw.ElapsedTicks;
			sm(ref startState, ' ');
			sw.Stop();
			this.output.WriteLine("Build ticks: " + lapBuild);
			this.output.WriteLine("Compile ticks: " + (lapCompile - lapBuild));
			this.output.WriteLine("JIT ticks: " + (lapJit - lapCompile));
			this.output.WriteLine("Exec ticks: " + (sw.ElapsedTicks - lapJit));
			this.output.WriteLine("Total ms: " + sw.ElapsedMilliseconds);
			this.output.WriteLine(stateMachine.ToReadableString());
		}

		[Fact]
		public void RegexTokenize() {
			var rx = RegexLexer.Create((symbol, data, index) => this.output.WriteLine("{0}({1}): {2}", symbol.ToString(RegexLexer.SymbolNameResolver), index, data.AsString()));
			rx.Push(@"this\sis\sa\stest".Append(Utf16Chars.EOF));
		}
	}
}
