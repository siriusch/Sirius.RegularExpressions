using System;
using System.Linq;

using AgileObjects.ReadableExpressions;

using Sirius.RegularExprerssions.Alphabet;
using Sirius.RegularExprerssions.Automata;
using Sirius.RegularExpressions;
using Sirius.RegularExpressions.Alphabet;
using Sirius.RegularExpressions.Automata;
using Sirius.RegularExpressions.Parser;
using Sirius.Unicode;

using Xunit;
using Xunit.Abstractions;

namespace Sirius.RegularExprerssions.Parser {
	public class RegexLexerTest {
		private readonly ITestOutputHelper output;

		public RegexLexerTest(ITestOutputHelper output) {
			this.output = output;
		}

		[Fact]
		public void RegexTokenize() {
			var rx = RegexLexer.TokenRegex;
			var mapper = new UnicodeUtf16Mapper(false, false);
			var charSetProvider = new UnicodeCharSetProvider(name => RegexLexer.NamedSets[name]);
			var rxi = rx.ToInvariant(mapper, charSetProvider, true);
			var nfa1 = NfaBuilder<char>.Build(rxi, mapper.Negate);
			var dfa1 = DfaBuilder<char>.Build(nfa1, Utf16Chars.EOF);
			var ab = new AlphabetBuilder<char>(rxi, mapper.Negate);
			this.output.WriteLine($"Letters: {ab.AlphabetById.Count} | Ranges: {ab.AlphabetById.Values.SelectMany(v => v).Count()}");
			var expr = AlphabetMapperEmitter<char>.CreateExpression(ab);
			//			this.output.WriteLine(expr.ToReadableString());
			var func = expr.Compile();
			var nfa2 = NfaBuilder<LetterId>.Build(ab.Expression, ab.Negate);
			var dfa2 = DfaBuilder<LetterId>.Build(nfa2, LetterId.Eof);
			this.output.WriteLine($"Codepoint DFA states: {dfa1.States.Count} | Alphabet DFA states: {dfa2.States.Count}");
			this.output.WriteLine($"Codepoint DFA transitions: {dfa1.States.SelectMany(s => s.Transitions).Count()} | Alphabet DFA transitions: {dfa2.States.SelectMany(s => s.Transitions).Count()}");
			var expr2 = DfaStateMachineEmitter.CreateExpression(dfa2);
			this.output.WriteLine(expr2.ToReadableString());
			var expr3 = DfaStateMachineEmitter.CreateExpression<LetterId>(dfa2);
			this.output.WriteLine(expr3.ToReadableString());
		}
	}
}
