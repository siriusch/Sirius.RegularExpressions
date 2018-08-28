using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;

using Sirius.Collections;
using Sirius.RegularExpressions.Alphabet;
using Sirius.RegularExpressions.Automata;
using Sirius.Unicode;

namespace Sirius.RegularExpressions.Parser {
	public sealed class RegexLexer: Lexer<char, LetterId> {
		internal static readonly SymbolId SymNoise = 0;
		public static readonly SymbolId SymCharset = 1;
		public static readonly SymbolId SymRegexLetter = 2;
		public static readonly SymbolId SymRegexEscape = 3;
		public static readonly SymbolId SymRegexCharset = 4;
		public static readonly SymbolId SymRegexKleene = 5;
		public static readonly SymbolId SymRegexRepeat = 6;
		public static readonly SymbolId SymRegexAny = 7;
		public static readonly SymbolId SymRegexOptional = 8;
		public static readonly SymbolId SymRegexDot = 9;
		public static readonly SymbolId SymSensitiveGroup = 10;
		public static readonly SymbolId SymInsensitiveGroup = 11;
		public static readonly SymbolId SymBeginGroup = 12;
		public static readonly SymbolId SymEndGroup = 13;

		public static readonly Func<SymbolId, string> SymbolNameResolver = new Dictionary<SymbolId, string>(17) {
				{SymbolId.Accept, "(Accept)"},
				{SymbolId.Reject, "(Reject)"},
				{SymbolId.Eof, "(Eof)"},
				{SymNoise, "(Noise)"},
				{SymCharset, "Charset"},
				{SymRegexLetter, "RegexLetter"},
				{SymRegexEscape, "RegexEscape"},
				{SymRegexCharset, "RegexCharset"},
				{SymRegexKleene, "RegexKleene"},
				{SymRegexRepeat, "RegexRepeat"},
				{SymRegexAny, "RegexAny"},
				{SymRegexOptional, "RegexOptional"},
				{SymRegexDot, "RegexDot"},
				{SymSensitiveGroup, "SensitiveGroup"},
				{SymInsensitiveGroup, "InsensitiveGroup"},
				{SymBeginGroup, "BeginGroup"},
				{SymEndGroup, "EndGroup"}
		}.CreateGetterForIndexer();

		private static readonly Lazy<Func<Action<SymbolId, IEnumerable<char>, long>, RegexLexer>> factory = new Lazy<Func<Action<SymbolId, IEnumerable<char>, long>, RegexLexer>>(() => {
			CreateStateMachine(out var stateMachine, out var startStateId);
			return tokenAction => new RegexLexer(stateMachine.Compile(), new Id<DfaState<LetterId>>(startStateId), true, tokenAction, SymNoise);
		}, LazyThreadSafetyMode.PublicationOnly);

		private static IReadOnlyDictionary<string, RangeSet<Codepoint>> CreateNamedSets() {
			var regexPrintable = RangeSet<Codepoint>.Subtract(Codepoints.ValidBmp, UnicodeCharSetProvider.SpaceCharSet);
			return new Dictionary<string, RangeSet<Codepoint>>(StringComparer.Ordinal) {
					{"RegexChar", RangeSet<Codepoint>.Subtract(regexPrintable, RangeSet<Codepoint>.Union(new RangeSet<Codepoint>(@"|/\{}()[].+?*".ToCodepoints()), UnicodeRanges.FromUnicodeName("InCombining_Diacritical_Marks")))},
					{"RegexCharset", RangeSet<Codepoint>.Subtract(regexPrintable, new RangeSet<Codepoint>(@"\]".ToCodepoints()))},
					{"EscapePrintable", RangeSet<Codepoint>.Subtract(regexPrintable, new RangeSet<Codepoint>(@"PpxUu".ToCodepoints()))},
					{"CharsetPrintable", RangeSet<Codepoint>.Subtract(regexPrintable, new RangeSet<Codepoint>(@"}".ToCodepoints()))},
					{"HexChar", new RangeSet<Codepoint>(@"0123456789AaBbCcDdEeFf".ToCodepoints())},
					{"Digit", new RangeSet<Codepoint>(@"0123456789".ToCodepoints())}
			};
		}

		private static RegexExpression CreateTokenRegex() {
			// To solve the chicken-egg problem of the Regex Parser, we specify the regex with the functional API instead of using the parser
			var rxCharset = RegexConcatenation.Create(
					RegexMatchSet.FromChars('{'),
					RegexMatchSet.FromUnicode("Letter"),
					RegexQuantified.Create(
							RegexMatchSet.FromNamedCharset("{CharsetPrintable}"),
							RegexQuantifier.Kleene()),
					RegexMatchSet.FromChars('}'));
			var rxEscape = RegexConcatenation.Create(
					RegexMatchSet.FromChars('\\'),
					RegexAlternation.Create(
							RegexAlternation.Create(
									RegexConcatenation.Create(
											RegexMatchSet.FromChars('x'),
											RegexQuantified.Create(
													RegexMatchSet.FromNamedCharset("{HexChar}"),
													RegexQuantifier.Occurs(2))),
									RegexConcatenation.Create(
											RegexMatchSet.FromChars('u'),
											RegexQuantified.Create(
													RegexMatchSet.FromNamedCharset("{HexChar}"),
													RegexQuantifier.Occurs(4))),
									RegexConcatenation.Create(
											RegexMatchSet.FromChars('U'),
											RegexQuantified.Create(
													RegexMatchSet.FromNamedCharset("{HexChar}"),
													RegexQuantifier.Occurs(8)))),
							RegexConcatenation.Create(
									RegexMatchSet.FromChars('p', 'P'),
									RegexAlternation.Create(
											RegexMatchSet.FromUnicode("Letter"),
											RegexConcatenation.Create(
													RegexMatchSet.FromChars('{'),
													RegexMatchSet.FromUnicode("Letter"),
													RegexQuantified.Create(
															RegexMatchSet.FromNamedCharset("{CharsetPrintable}"),
															RegexQuantifier.Kleene()),
													RegexMatchSet.FromChars('}')))),
							RegexMatchSet.FromNamedCharset("{EscapePrintable}")));
			return RegexAlternation.Create(
					RegexAccept.Create(
							RegexQuantified.Create(
									RegexMatchSet.FromClass(CharSetClass.Space),
									RegexQuantifier.Any()),
							SymNoise),
					RegexAccept.Create(
							rxCharset,
							SymCharset),
					RegexAccept.Create(
							RegexConcatenation.Create(
									RegexMatchSet.FromNamedCharset("{RegexChar}"),
									RegexQuantified.Create(
											RegexMatchSet.FromUnicode("InCombining_Diacritical_Marks"),
											RegexQuantifier.Kleene())),
							SymRegexLetter),
					RegexAccept.Create(
							rxEscape,
							SymRegexEscape),
					RegexAccept.Create(
							RegexConcatenation.Create(
									RegexMatchSet.FromChars('['),
									RegexQuantified.Create(
											RegexMatchSet.FromChars('^'),
											RegexQuantifier.Optional()),
									RegexQuantified.Create(
											RegexAlternation.Create(
													RegexMatchSet.FromNamedCharset("{RegexCharset}"),
													rxEscape),
											RegexQuantifier.Any()),
									RegexMatchSet.FromChars(']')),
							SymRegexCharset),
					RegexAccept.Create(
							RegexMatchSet.FromChars('*'),
							SymRegexKleene),
					RegexAccept.Create(
							RegexConcatenation.Create(
									RegexMatchSet.FromChars('{'),
									RegexQuantified.Create(
											RegexMatchSet.FromNamedCharset("{Digit}"),
											RegexQuantifier.Any()),
									RegexQuantified.Create(
											RegexConcatenation.Create(
													RegexMatchSet.FromChars(','),
													RegexQuantified.Create(
															RegexMatchSet.FromNamedCharset("{Digit}"),
															RegexQuantifier.Kleene())),
											RegexQuantifier.Optional()),
									RegexMatchSet.FromChars('}')),
							SymRegexRepeat),
					RegexAccept.Create(
							RegexMatchSet.FromChars('+'),
							SymRegexAny),
					RegexAccept.Create(
							RegexMatchSet.FromChars('?'),
							SymRegexOptional),
					RegexAccept.Create(
							RegexMatchSet.FromChars('.'),
							SymRegexDot),
					RegexAccept.Create(
							RegexMatchSet.FromChars('*'),
							SymRegexKleene),
					RegexAccept.Create(
							RegexMatchSet.FromChars('('),
							SymBeginGroup),
					RegexAccept.Create(
							RegexConcatenation.Create(
									RegexMatchSet.FromChars('('),
									RegexMatchSet.FromChars('?'),
									RegexMatchSet.FromChars('-'),
									RegexMatchSet.FromChars('i'),
									RegexMatchSet.FromChars(':')
							),
							SymSensitiveGroup),
					RegexAccept.Create(
							RegexConcatenation.Create(
									RegexMatchSet.FromChars('('),
									RegexMatchSet.FromChars('?'),
									RegexMatchSet.FromChars('i'),
									RegexMatchSet.FromChars(':')
							),
							SymInsensitiveGroup),
					RegexAccept.Create(
							RegexMatchSet.FromChars(')'),
							SymEndGroup));
		}

		public static RegexLexer Create(Action<SymbolId, IEnumerable<char>, long> tokenAction) {
			return factory.Value(tokenAction);
		}

		internal static void CreateStateMachine(out Expression<DfaStateMachine<LetterId, char>> stateMachine, out int startStateId) {
			var mapper = new UnicodeUtf16Mapper(false, false);
			var provider = new UnicodeCharSetProvider(CreateNamedSets());
			var charRx = CreateTokenRegex().ToInvariant(mapper, provider, true);
			var alpha = new AlphabetBuilder<char>(charRx, mapper.Negate, Utf16Chars.EOF, Utf16Chars.ValidBmp);
			var charToLetter = AlphabetMapperEmitter<char>.CreateExpression(alpha);
			var nfa = NfaBuilder<LetterId>.Build(alpha.Expression, alpha.Negate);
			var dfa = DfaBuilder<LetterId>.Build(nfa, LetterId.Eof);
			stateMachine = DfaStateMachineEmitter.CreateExpression(dfa, charToLetter);
			startStateId = dfa.StartState.Id.ToInt32();
		}

		private RegexLexer(DfaStateMachine<LetterId, char> stateMachine, Id<DfaState<LetterId>> startStateId, bool handleEof, Action<SymbolId, IEnumerable<char>, long> tokenAction, params SymbolId[] symbolsToIgnore):
				base(stateMachine, startStateId, handleEof, tokenAction, symbolsToIgnore) { }
	}
}
