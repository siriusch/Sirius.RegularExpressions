using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Sirius.Collections;
using Sirius.RegularExpressions.Automata;
using Sirius.RegularExpressions.Invariant;
using Sirius.RegularExpressions.Parser;
using Sirius.Unicode;

using Xunit;
using Xunit.Abstractions;

namespace Sirius.RegularExpressions.RegularExpressions.Automata {
	public class NfaFactoryTest {
		internal static void WriteDiagram<T>(ITestOutputHelper output, Nfa<T> builder)
				where T: struct, IEquatable<T>, IComparable<T> {
			output.WriteLine("digraph G {");
			output.WriteLine("graph[rankdir=\"LR\",dpi=65,size=100,forcelabels=true];");
			foreach (var pair in builder.States.SelectMany(s => s.EpsilonTransitions.Select(t => new KeyValuePair<NfaState<T>, NfaState<T>>(s, t)))) {
				output.WriteLine("\"{0}\" -> \"{1}\" [label=\"ε\"];", pair.Key.Id, pair.Value.Id);
			}
			foreach (var pair in builder.States.SelectMany(s => s.MatchTransitions.Select(t => new KeyValuePair<NfaState<T>, KeyValuePair<Range<T>, NfaState<T>>>(s, t)))) {
				output.WriteLine("\"{0}\" -> \"{1}\" [label=\"{2}\"];", pair.Key.Id, pair.Value.Value.Id, Regex.Replace(pair.Value.Key.ToString(), @"[^a-zA-Z0-9\.,_-]", c => string.Format("x{0:x2}", (int)c.Value[0])));
			}
			foreach (var state in builder.States) {
				if (state.AcceptId.HasValue) {
					output.WriteLine("\"{0}\" [peripheries=3,xlabel=\"Accept {1}\"];", state.Id, state.AcceptId);
				}
				if (state.Id == builder.EndState.Id) {
					output.WriteLine("\"{0}\" [peripheries=2,xlabel=\"END\"];", state.Id);
				}
			}
			output.WriteLine("}");
		}

		private readonly ITestOutputHelper output;

		public NfaFactoryTest(ITestOutputHelper output) {
			this.output = output;
		}

		[Theory]
		[InlineData("a", false)]
		[InlineData("ab", true)]
		[InlineData("a|b", true)]
		[InlineData("a|a", true)]
		[InlineData("a?", true)]
		[InlineData("a+", true)]
		[InlineData("a*", true)]
		[InlineData("a{3}", true)]
		[InlineData("a{2,4}", true)]
		[InlineData("(abc[abc]){2,3}|ac", false)]
		[InlineData("ab*(a|b)*", true)]
		[InlineData("a*b", true)]
		[InlineData("[ab]*b", true)]
		[InlineData(@"x(?i:[0-9A-F]{2})", true)]
		[InlineData(@"(a|a|c)*", true)]
		[InlineData(@"\[(^)?((\\(x[0-9A-Fa-f]{2}|u[0-9A-Fa-f]{4}|[^PpuxDdWwSs])|[^\\\]])-(\\(x[0-9A-Fa-f]{2}|u[0-9A-Fa-f]{4}|[^PpuxDdWwSs])|[^\\\]])|(\\([Pp](\p{L}|\{[^\}]+\})|x[0-9A-Fa-f]{2}|u[0-9A-Fa-f]{4}|[^Ppux])|[^\\\]]))*\]", false)]
		public void CreateDiagram(string regex, bool caseSensitive) {
			var expression = RegexParser.Parse(regex, 0);
			var mapper = new UnicodeCodepointMapper(false, false);
			var rxCodepoint = expression.ToInvariant(mapper, new UnicodeCharSetProvider(null), caseSensitive);
			var rxAccept = new RxAccept<Codepoint>(rxCodepoint);
			var nfa = NfaBuilder<Codepoint>.Build(rxAccept, mapper.Negate);
			WriteDiagram(this.output, nfa);
		}
	}
}
