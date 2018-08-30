using Sirius.RegularExpressions.Invariant;
using Sirius.Unicode;

namespace Sirius.RegularExpressions.Parser {
	public class RegexCaseGroup: RegexExpression {
		public static RegexCaseGroup CaseSensitve(RegexExpression inner) {
			return new RegexCaseGroup(inner, true);
		}

		public static RegexCaseGroup CaseInsensitve(RegexExpression inner) {
			return new RegexCaseGroup(inner, false);
		}

		private readonly bool caseSensitive;

		private readonly RegexExpression inner;

		private RegexCaseGroup(RegexExpression inner, bool caseSensitive) {
			this.inner = inner;
			this.caseSensitive = caseSensitive;
		}

		public override RxNode<TLetter> ToInvariant<TLetter>(IUnicodeMapper<TLetter> mapper, IRangeSetProvider<Codepoint> provider, bool caseSensitive) {
			return this.inner.ToInvariant(mapper, provider, this.caseSensitive);
		}
	}
}
