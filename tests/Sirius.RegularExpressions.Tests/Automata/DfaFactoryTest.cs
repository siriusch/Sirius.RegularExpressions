using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sirius.Collections;
using Sirius.RegularExpressions.Invariant;
using Sirius.RegularExpressions.Parser;
using Sirius.Unicode;

using Xunit;
using Xunit.Abstractions;

namespace Sirius.RegularExpressions.Automata {
	public class DfaFactoryTest {
		private static void WriteDiagram<TLetter>(ITestOutputHelper output, Dfa<TLetter> dfa)
				where TLetter: struct, IComparable<TLetter>, IEquatable<TLetter> {
			output.WriteLine("digraph G {");
			output.WriteLine("graph[rankdir=\"LR\",dpi=65,size=100,forcelabels=true];");
			output.WriteLine("\"{0}\" [peripheries=2,label=\"Accept\"];", Dfa<TLetter>.Accept);
			foreach (var symbolState in dfa.SymbolStates) {
				output.WriteLine("\"{0}\" [xlabel=\"{1}\"];", symbolState.Key, symbolState.Value);
			}
			//			output.WriteLine("\"{0}\" [peripheries=2,label=\"Reject\"];", Dfa<TLetter>.Reject);
			//			foreach (var state in dfa.States) {
			//				output.WriteLine("\"{0}\" -> \"{1}\" [style=\"dashed\"];", state.Id, Dfa<TLetter>.Reject);
			//			}
			foreach (var pair in dfa.States.SelectMany(s => s.Transitions.Select(t => new KeyValuePair<DfaState<TLetter>, KeyValuePair<Range<TLetter>, Id<DfaState<TLetter>>>>(s, t)))) {
				output.WriteLine("\"{0}\" -> \"{1}\" [label=\"{2}\"];", pair.Key.Id, pair.Value.Value, pair.Value.Key);
			}
			output.WriteLine("}");
		}

		private readonly ITestOutputHelper output;

		public DfaFactoryTest(ITestOutputHelper output) {
			this.output = output;
		}

		[Fact]
		public void Actions() {
			var symbolA = 1;
			var regexA = "a";
			var r1 = RegexParser.Parse(regexA, symbolA);
			var symbolB = 2;
			var regexB = "b";
			var r2 = RegexParser.Parse(regexB, symbolB);
			RegexExpression r = new RegexAlternation(r1, r2);
			var mapper = new UnicodeCodepointMapper(false, false);
			var rxCodepoint = r.ToInvariant(mapper, new UnicodeCharSetProvider(), true);
			var nfa = NfaBuilder<Codepoint>.Build(rxCodepoint.Optimize());
			NfaFactoryTest.WriteDiagram(this.output, nfa);
			var dfa = DfaBuilder<Codepoint>.Build(nfa, Codepoints.EOF);
			WriteDiagram(this.output, dfa);
			var state = dfa.StartState;
			foreach (var codepoint in regexA.ToCodepoints().Append(Codepoints.EOF)) {
				var nextStateId = state.GetTransition(codepoint);
				Assert.NotEqual(Dfa<Codepoint>.Reject, nextStateId);
				if (nextStateId == Dfa<Codepoint>.Accept) {
					Assert.Equal(dfa.SymbolStates[state.Id], symbolA);
					break;
				}
				state = dfa.GetState(nextStateId);
				Assert.NotNull(state);
			}
			state = dfa.StartState;
			foreach (var codepoint in regexB.ToCodepoints().Append(Codepoints.EOF)) {
				var nextStateId = state.GetTransition(codepoint);
				Assert.NotEqual(Dfa<Codepoint>.Reject, nextStateId);
				if (nextStateId == Dfa<Codepoint>.Accept) {
					Assert.Equal(dfa.SymbolStates[state.Id], symbolB);
					break;
				}
				state = dfa.GetState(nextStateId);
				Assert.NotNull(state);
			}
		}

		[Theory]
		[InlineData(@"a", false)]
		[InlineData(@"ab", true)]
		[InlineData(@"a|b", true)]
		[InlineData(@"a?", true)]
		[InlineData(@"a+", true)]
		[InlineData(@"a*", true)]
		[InlineData(@"a{3}", true)]
		[InlineData(@"a{2,4}", true)]
		[InlineData(@"(abc[abc]){2,3}|ac", false)]
		[InlineData(@"ab*(a|b)*", true)]
		[InlineData(@"a*b", true)]
		[InlineData(@"[ab]*b", true)]
		[InlineData(@"a|a+", true)]
		[InlineData(@"ab|[^a]b", true)]
		[InlineData(@"this is a more complicated regex|anything that matters|[0-9]+|[a-z\x20]+", false)]
		[InlineData(@"\[(^)?((\\(x[0-9A-Fa-f]{2}|u[0-9A-Fa-f]{4}|[^PpuxDdWwSs])|[^\\\]])-(\\(x[0-9A-Fa-f]{2}|u[0-9A-Fa-f]{4}|[^PpuxDdWwSs])|[^\\\]])|(\\([Pp](\p{L}|\{[^\}]+\})|x[0-9A-Fa-f]{2}|u[0-9A-Fa-f]{4}|[^Ppux])|[^\\\]]))*\]", false)]
		public void CreateCodepointDiagram(string regex, bool caseSensitive) {
			var expression = RegexParser.Parse(regex, 0, false);
			var mapper = new UnicodeCodepointMapper(false, false);
			var rxCodepoint = expression.ToInvariant(mapper, new UnicodeCharSetProvider(), caseSensitive);
			var nfa = NfaBuilder<Codepoint>.Build(rxCodepoint.Optimize());
			var dfa = DfaBuilder<Codepoint>.Build(nfa, Codepoints.EOF);
			NfaFactoryTest.WriteDiagram(this.output, nfa);
			WriteDiagram(this.output, dfa);
		}

		[Theory]
		[InlineData(@"Arsène", true)]
		[InlineData(@"[a]", true)]
		[InlineData(@"[a]", false)]
		[InlineData(@"[^a]", true)]
		[InlineData(@"[^a]", false)]
		[InlineData(@"[^\x00-x1F""\\]", false)]
		[InlineData(@"\[|\{|""([^\x00-x1F""\\]|\\(u[0-9a-fA-F]{4}|[^u]))*""|-?[0-9]+(\.[0-9]+)([eE][+-]?[0-9]+)?", false)]
		public void CreateUtf8Diagram(string regex, bool caseSensitive) {
			var expression = RegexParser.Parse(regex, 0);
			var mapper = new UnicodeUtf8Mapper(false, false);
			var rxCodepoint = expression.ToInvariant(mapper, new UnicodeCharSetProvider(), caseSensitive);
			var nfa = NfaBuilder<byte>.Build(rxCodepoint.Optimize());
			var dfa = DfaBuilder<byte>.Build(nfa, 0xFF);
			NfaFactoryTest.WriteDiagram(this.output, nfa);
			WriteDiagram(this.output, dfa);
		}

		[Theory]
		[InlineData(@"a", false, "", false)]
		[InlineData(@"a?", false, "", true)]
		[InlineData(@"a*", false, "", true)]
		[InlineData(@"a+", false, "", false)]
		[InlineData(@"a|b", false, "", false)]
		[InlineData(@"a", false, "a", true)]
		[InlineData(@"a?", false, "a", true)]
		[InlineData(@"a*", false, "a", true)]
		[InlineData(@"a+", false, "a", true)]
		[InlineData(@"a|b", false, "a", true)]
		[InlineData(@"a", false, "aa", false)]
		[InlineData(@"a?", false, "aa", false)]
		[InlineData(@"a*", false, "aa", true)]
		[InlineData(@"a+", false, "aa", true)]
		[InlineData(@"a|b", false, "aa", false)]
		[InlineData(@"a.b", false, "a\nb", false)]
		[InlineData(@"a.b", false, "a\rb", true)]
		[InlineData(@"(a*|b*)+", false, "aabaabbabaa", true)]
		[InlineData(@"(a+|b+)+", false, "aabaabbabaa", true)]
		[InlineData(@"(a|b)+", false, "aabaabbabaa", true)]
		[InlineData(@"(a{2,}|b)+", false, "aabaabbabaa", false)]
		[InlineData(@"(a{2,}|b)+", false, "aabaabbbaabaa", true)]
		[InlineData(@"[\p{L}_][\p{L}_0-9]*", false, "aabaa_2342äöü", true)]
		[InlineData(@"\[(^)?((\\(x[0-9A-Fa-f]{2}|u[0-9A-Fa-f]{4}|[^PpuxDdWwSs])|[^\\\]])-(\\(x[0-9A-Fa-f]{2}|u[0-9A-Fa-f]{4}|[^PpuxDdWwSs])|[^\\\]])|(\\([Pp](\p{L}|\{[^\}]+\})|x[0-9A-Fa-f]{2}|u[0-9A-Fa-f]{4}|[^Ppux])|[^\\\]]))*\]", false, @"w", false)]
		[InlineData(@"\[(^)?((\\(x[0-9A-Fa-f]{2}|u[0-9A-Fa-f]{4}|[^PpuxDdWwSs])|[^\\\]])-(\\(x[0-9A-Fa-f]{2}|u[0-9A-Fa-f]{4}|[^PpuxDdWwSs])|[^\\\]])|(\\([Pp](\p{L}|\{[^\}]+\})|x[0-9A-Fa-f]{2}|u[0-9A-Fa-f]{4}|[^Ppux])|[^\\\]]))*\]", false, @"[a-z]", true)]
		[InlineData(@"\[(^)?((\\(x[0-9A-Fa-f]{2}|u[0-9A-Fa-f]{4}|[^PpuxDdWwSs])|[^\\\]])-(\\(x[0-9A-Fa-f]{2}|u[0-9A-Fa-f]{4}|[^PpuxDdWwSs])|[^\\\]])|(\\([Pp](\p{L}|\{[^\}]+\})|x[0-9A-Fa-f]{2}|u[0-9A-Fa-f]{4}|[^Ppux])|[^\\\]]))*\]", false, @"[\da-z$]", true)]
		[InlineData(@"\[(^)?((\\(x[0-9A-Fa-f]{2}|u[0-9A-Fa-f]{4}|[^PpuxDdWwSs])|[^\\\]])-(\\(x[0-9A-Fa-f]{2}|u[0-9A-Fa-f]{4}|[^PpuxDdWwSs])|[^\\\]])|(\\([Pp](\p{L}|\{[^\}]+\})|x[0-9A-Fa-f]{2}|u[0-9A-Fa-f]{4}|[^Ppux])|[^\\\]]))*\]", false, @"[\x20]", true)]
		[InlineData(@"\[(^)?((\\(x[0-9A-Fa-f]{2}|u[0-9A-Fa-f]{4}|[^PpuxDdWwSs])|[^\\\]])-(\\(x[0-9A-Fa-f]{2}|u[0-9A-Fa-f]{4}|[^PpuxDdWwSs])|[^\\\]])|(\\([Pp](\p{L}|\{[^\}]+\})|x[0-9A-Fa-f]{2}|u[0-9A-Fa-f]{4}|[^Ppux])|[^\\\]]))*\]", false, @"[\x]", false)]
		[InlineData(@"\[(^)?((\\(x[0-9A-Fa-f]{2}|u[0-9A-Fa-f]{4}|[^PpuxDdWwSs])|[^\\\]])-(\\(x[0-9A-Fa-f]{2}|u[0-9A-Fa-f]{4}|[^PpuxDdWwSs])|[^\\\]])|(\\([Pp](\p{L}|\{[^\}]+\})|x[0-9A-Fa-f]{2}|u[0-9A-Fa-f]{4}|[^Ppux])|[^\\\]]))*\]", false, @"[\u0020]", true)]
		[InlineData(@"\[(^)?((\\(x[0-9A-Fa-f]{2}|u[0-9A-Fa-f]{4}|[^PpuxDdWwSs])|[^\\\]])-(\\(x[0-9A-Fa-f]{2}|u[0-9A-Fa-f]{4}|[^PpuxDdWwSs])|[^\\\]])|(\\([Pp](\p{L}|\{[^\}]+\})|x[0-9A-Fa-f]{2}|u[0-9A-Fa-f]{4}|[^Ppux])|[^\\\]]))*\]", false, @"[\u]", false)]
		[InlineData(@"\[(^)?((\\(x[0-9A-Fa-f]{2}|u[0-9A-Fa-f]{4}|[^PpuxDdWwSs])|[^\\\]])-(\\(x[0-9A-Fa-f]{2}|u[0-9A-Fa-f]{4}|[^PpuxDdWwSs])|[^\\\]])|(\\([Pp](\p{L}|\{[^\}]+\})|x[0-9A-Fa-f]{2}|u[0-9A-Fa-f]{4}|[^Ppux])|[^\\\]]))*\]", false, @"[\p{L}]", true)]
		[InlineData(@"\[(^)?((\\(x[0-9A-Fa-f]{2}|u[0-9A-Fa-f]{4}|[^PpuxDdWwSs])|[^\\\]])-(\\(x[0-9A-Fa-f]{2}|u[0-9A-Fa-f]{4}|[^PpuxDdWwSs])|[^\\\]])|(\\([Pp](\p{L}|\{[^\}]+\})|x[0-9A-Fa-f]{2}|u[0-9A-Fa-f]{4}|[^Ppux])|[^\\\]]))*\]", false, @"[\P{L}]", true)]
		[InlineData(@"\[(^)?((\\(x[0-9A-Fa-f]{2}|u[0-9A-Fa-f]{4}|[^PpuxDdWwSs])|[^\\\]])-(\\(x[0-9A-Fa-f]{2}|u[0-9A-Fa-f]{4}|[^PpuxDdWwSs])|[^\\\]])|(\\([Pp](\p{L}|\{[^\}]+\})|x[0-9A-Fa-f]{2}|u[0-9A-Fa-f]{4}|[^Ppux])|[^\\\]]))*\]", false,
				@"[\\sa,fmhsadf,nb,sfa-s-dafgsdgfasg-a--sadgasdgsadfalkjwerkjhegfkasflkjasf]", true)]
		public void MatchTest(string regex, bool caseSensitive, string input, bool match) {
			var expression = RegexParser.Parse(regex, 0);
			var mapper = new UnicodeCodepointMapper(false, false);
			var rxCodepoint = expression.ToInvariant(mapper, new UnicodeCharSetProvider(), caseSensitive);
			var nfa = NfaBuilder<Codepoint>.Build(rxCodepoint.Optimize());
			var dfa = DfaBuilder<Codepoint>.Build(nfa, Codepoints.EOF);
			var state = dfa.StartState;
			foreach (var codepoint in input.ToCodepoints().Append(Codepoints.EOF)) {
				var nextStateId = state.GetTransition(codepoint);
				if (nextStateId == Dfa<Codepoint>.Reject) {
					Assert.False(match, "Regex has not matched input, but expected a match");
					break;
				}
				if (nextStateId == Dfa<Codepoint>.Accept) {
					Assert.True(match, "Regex has matched input, but expected a non-match");
					break;
				}
				state = dfa.GetState(nextStateId);
				Assert.NotNull(state);
			}
		}

		[Theory]
		[InlineData(@"arsène", false, new byte[] {65, 114, 115, 101, 204, 128, 110, 101}, true)]
		[InlineData(@"arsène", false, new byte[] {65, 114, 115, 195, 168, 110, 101}, true)]
		[InlineData(@"arsène", true, new byte[] {65, 114, 115, 195, 168, 110, 101}, false)]
		[InlineData(@"[^a]+", false, new byte[] {65}, false)]
		[InlineData(@"[^a]+", true, new byte[] {65}, true)]
		public void MatchTestUtf8(string regex, bool caseSensitive, byte[] input, bool match) {
			var inputString = Encoding.UTF8.GetString(input);
			this.output.WriteLine(inputString);
			this.output.WriteLine(string.Join(" ", inputString.Select(c => ((int)c).ToString("x4"))));
			var expression = RegexParser.Parse(regex, 0);
			var mapper = new UnicodeUtf8Mapper(false, false);
			var rxCodepoint = expression.ToInvariant(mapper, new UnicodeCharSetProvider(), caseSensitive);
			var nfa = NfaBuilder<byte>.Build(rxCodepoint.Optimize());
			var dfa = DfaBuilder<byte>.Build(nfa, Utf8Bytes.EOF);
			var state = dfa.StartState;
			foreach (var inputByte in input.Append(Utf8Bytes.EOF)) {
				var nextStateId = state.GetTransition(inputByte);
				if (nextStateId == Dfa<byte>.Reject) {
					Assert.False(match, "Regex has not matched input, but expected a match");
					break;
				}
				if (nextStateId == Dfa<byte>.Accept) {
					Assert.True(match, "Regex has matched input, but expected a non-match");
					break;
				}
				state = dfa.GetState(nextStateId);
				Assert.NotNull(state);
			}
		}
	}
}
