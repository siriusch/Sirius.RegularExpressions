using System;
using System.Collections;
using System.Collections.Generic;

using Sirius.Collections;
using Sirius.RegularExpressions.Alphabet;
using Sirius.RegularExpressions.Automata;
using Sirius.RegularExpressions.Invariant;
using Sirius.RegularExpressions.Parser;

namespace Sirius.RegularExpressions {
	public class LexerBuilder<TLetter>: IEnumerable<KeyValuePair<SymbolId, RxNode<TLetter>>>
			where TLetter: struct, IEquatable<TLetter>, IComparable<TLetter> {
		private readonly TLetter? eof;
		private readonly IUnicodeMapper<TLetter> mapper;
		private readonly CharSetProviderBase provider;
		private readonly Dictionary<SymbolId, RxNode<TLetter>> tokens = new Dictionary<SymbolId, RxNode<TLetter>>();

		public LexerBuilder(IUnicodeMapper<TLetter> mapper, TLetter? eof = null, CharSetProviderBase provider = null) {
			this.eof = eof;
			this.provider = provider ?? new UnicodeCharSetProvider();
			this.mapper = mapper;
		}

		public IEnumerator<KeyValuePair<SymbolId, RxNode<TLetter>>> GetEnumerator() {
			return this.tokens.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return ((IEnumerable)this.tokens).GetEnumerator();
		}

		public void Add(SymbolId symbol, params TLetter[] matchSet) {
			this.Add(symbol, new RxMatch<TLetter>(matchSet));
		}

		public void Add(SymbolId symbol, RangeSet<TLetter> matchSet) {
			this.Add(symbol, new RxMatch<TLetter>(matchSet));
		}

		public void Add(SymbolId symbol, RxNode<TLetter> regex) {
			this.tokens.Add(symbol, regex);
		}

		public void Add(SymbolId symbol, string regex, bool caseSensitive = true) {
			this.Add(symbol, RegexParser.Parse(regex, symbol).ToInvariant(this.mapper, this.provider, caseSensitive));
		}

		public RangeSet<TLetter> ValidRange => this.mapper.ValidLetters;

		public Dfa<LetterId> ComputeDfa(out AlphabetBuilder<TLetter> alphabet, RangeSet<TLetter> validRanges = default) {
			var regex = this.ComputeRx();
			alphabet = new AlphabetBuilder<TLetter>(regex, this.eof, validRanges);
			var nfa = NfaBuilder<LetterId>.Build(alphabet.Expression);
			return DfaBuilder<LetterId>.Build(nfa, LetterId.Eof, true);
		}

		public Dfa<TLetter> ComputeDfa() {
			var regex = this.ComputeRx();
			var nfa = NfaBuilder<TLetter>.Build(regex);
			return DfaBuilder<TLetter>.Build(nfa, this.eof, true);
		}

		private RxNode<TLetter> ComputeRx() {
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
			return regex;
		}
	}
}
