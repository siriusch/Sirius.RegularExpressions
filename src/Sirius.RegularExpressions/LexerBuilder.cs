using System;
using System.Collections;
using System.Collections.Generic;

using Sirius.RegularExpressions.Automata;
using Sirius.RegularExpressions.Invariant;
using Sirius.RegularExpressions.Parser;

namespace Sirius.RegularExpressions {
	public class LexerBuilder<TLetter>: IEnumerable<KeyValuePair<SymbolId, RxNode<TLetter>>> where TLetter: struct, IEquatable<TLetter>, IComparable<TLetter> {
		private readonly TLetter? eof;
		private readonly IUnicodeMapper<TLetter> mapper;
		private readonly UnicodeCharSetProvider provider;
		private readonly Dictionary<SymbolId, RxNode<TLetter>> tokens = new Dictionary<SymbolId, RxNode<TLetter>>();

		public LexerBuilder(IUnicodeMapper<TLetter> mapper, TLetter? eof, UnicodeCharSetProvider provider = null) {
			this.eof = eof;
			this.provider = provider ?? new UnicodeCharSetProvider(null);
			this.mapper = mapper;
		}

		public IEnumerator<KeyValuePair<SymbolId, RxNode<TLetter>>> GetEnumerator() {
			return this.tokens.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return ((IEnumerable)this.tokens).GetEnumerator();
		}

		public void Add(SymbolId symbol, string regex, bool caseSensitive = true) {
			this.tokens.Add(symbol, RegexParser.Parse(regex, symbol).ToInvariant(this.mapper, this.provider, caseSensitive));
		}

		public Dfa<TLetter> ComputeDfa() {
			var regex = default(RxNode<TLetter>);
			using (var enumerator = this.tokens.Values.GetEnumerator()) {
				if (!enumerator.MoveNext()) {
					throw new InvalidOperationException("No terminals have been defined");
				}
				regex = enumerator.Current;
				while (enumerator.MoveNext()) {
					regex = new RxAlternation<TLetter>(enumerator.Current, regex);
				}
			}
			var nfa = NfaBuilder<TLetter>.Build(regex, this.mapper.Negate);
			return DfaBuilder<TLetter>.Build(nfa, this.eof);
		}
	}
}
