using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Threading;

using Sirius.Collections;
using Sirius.RegularExpressions.Alphabet;
using Sirius.RegularExpressions.Automata;
using Sirius.RegularExpressions.Invariant;
using Sirius.Unicode;

namespace Sirius.RegularExpressions.Parser {
	public sealed class RegexLexer: Lexer<char, LetterId> {
		public const int SymWhitespace = 0;

		public const int SymCharset = 1;
		public const int SymRegexLetter = 2;
		public const int SymRegexEscape = 3;
		public const int SymRegexCharset = 4;
		public const int SymRegexDot = 5;

		public const int SymQuantifyKleene = 6;
		public const int SymQuantifyRepeat = 7;
		public const int SymQuantifyAny = 8;
		public const int SymQuantifyOptional = 9;

		public const int SymAlternate = 10;

		public const int SymSensitiveGroup = 11;
		public const int SymInsensitiveGroup = 12;
		public const int SymBeginGroup = 13;
		public const int SymEndGroup = 14;

		public const int SymSlash = 15;

		public static readonly Func<SymbolId, string> SymbolNameResolver = ((IReadOnlyDictionary<SymbolId, string>)new Dictionary<SymbolId, string>(17) {
				{SymbolId.Accept, "(Accept)"},
				{SymbolId.Reject, "(Reject)"},
				{SymbolId.Eof, "(Eof)"},
				{SymWhitespace, "(Whitespace)"},
				{SymCharset, "Charset"},
				{SymRegexLetter, "RegexLetter"},
				{SymRegexEscape, "RegexEscape"},
				{SymRegexCharset, "RegexCharset"},
				{SymRegexDot, "RegexDot"},
				{SymQuantifyKleene, "QuantifyKleene"},
				{SymQuantifyRepeat, "QuantifyRepeat"},
				{SymQuantifyAny, "QuantifyAny"},
				{SymQuantifyOptional, "QuantifyOptional"},
				{SymAlternate, "Alternate"},
				{SymSensitiveGroup, "SensitiveGroup"},
				{SymInsensitiveGroup, "InsensitiveGroup"},
				{SymBeginGroup, "BeginGroup"},
				{SymEndGroup, "EndGroup"},
				{SymSlash, "Slash"}
		}).CreateGetter();

		private static readonly Lazy<Func<Action<SymbolId, Capture<char>>, RegexLexer>> factory = new Lazy<Func<Action<SymbolId, Capture<char>>, RegexLexer>>(() => {
			CreateStateMachine(out var stateMachine, out var startStateId);
			return tokenAction => new RegexLexer(stateMachine.Compile(), new Id<DfaState<LetterId>>(startStateId), true, tokenAction);
		}, LazyThreadSafetyMode.PublicationOnly);

		static RegexLexer() {
			// Warmup the regex lexer
			ThreadPool.QueueUserWorkItem(arg => arg = factory.Value);
		}

		private static IReadOnlyDictionary<string, RangeSet<Codepoint>> CreateNamedSets() {
			return new Dictionary<string, RangeSet<Codepoint>>(StringComparer.Ordinal) {
					{"RegexChar", RangeSet<Codepoint>.Subtract(Codepoints.ValidBmp, RangeSet<Codepoint>.Union(UnicodeCharSetProvider.SpaceCharSet, new RangeSet<Codepoint>(@"|/\{}()[].+?*".ToCodepoints()), UnicodeRanges.FromUnicodeCategory(UnicodeCategory.Control), UnicodeRanges.InCombiningDiacriticalMarks))},
					{"RegexCharset", RangeSet<Codepoint>.Subtract(Codepoints.ValidBmp, new RangeSet<Codepoint>(@"\]".ToCodepoints()))},
					{"EscapePrintable", RangeSet<Codepoint>.Subtract(Codepoints.ValidBmp, new RangeSet<Codepoint>(@"PpxUu".ToCodepoints()))},
					{"CharsetPrintable", RangeSet<Codepoint>.Subtract(Codepoints.ValidBmp, new RangeSet<Codepoint>(@"}".ToCodepoints()))},
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
							SymWhitespace, 0),
					RegexAccept.Create(
							RegexMatchSet.FromCodepoints('/'), 
							SymSlash, 0),
					RegexAccept.Create(
							rxCharset,
							SymCharset, 0),
					RegexAccept.Create(
							RegexConcatenation.Create(
									RegexMatchSet.FromNamedCharset("{RegexChar}"),
									RegexQuantified.Create(
											RegexMatchSet.FromUnicode("InCombining_Diacritical_Marks"),
											RegexQuantifier.Kleene())),
							SymRegexLetter, 0),
					RegexAccept.Create(
							rxEscape,
							SymRegexEscape, 0),
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
							SymRegexCharset, 0),
					RegexAccept.Create(
							RegexMatchSet.FromChars('.'),
							SymRegexDot, 0),
					RegexAccept.Create(
							RegexMatchSet.FromChars('*'),
							SymQuantifyKleene, 0),
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
							SymQuantifyRepeat, 0),
					RegexAccept.Create(
							RegexMatchSet.FromChars('+'),
							SymQuantifyAny, 0),
					RegexAccept.Create(
							RegexMatchSet.FromChars('?'),
							SymQuantifyOptional, 0),
					RegexAccept.Create(
							RegexMatchSet.FromChars('|'),
							SymAlternate, 0),
					RegexAccept.Create(
							RegexMatchSet.FromChars('('),
							SymBeginGroup, 0),
					RegexAccept.Create(
							RegexConcatenation.Create(
									RegexMatchSet.FromChars('('),
									RegexMatchSet.FromChars('?'),
									RegexMatchSet.FromChars('-'),
									RegexMatchSet.FromChars('i'),
									RegexMatchSet.FromChars(':')
							),
							SymSensitiveGroup, 0),
					RegexAccept.Create(
							RegexConcatenation.Create(
									RegexMatchSet.FromChars('('),
									RegexMatchSet.FromChars('?'),
									RegexMatchSet.FromChars('i'),
									RegexMatchSet.FromChars(':')
							),
							SymInsensitiveGroup, 0),
					RegexAccept.Create(
							RegexMatchSet.FromChars(')'),
							SymEndGroup, 0));
		}

		public static RegexLexer Create(Action<SymbolId, Capture<char>> tokenAction) {
			return factory.Value(tokenAction);
		}

		public static RxNode<TLetter> CreateRx<TLetter>(IUnicodeMapper<TLetter> mapper, bool caseSensitive = true)
				where TLetter: IEquatable<TLetter>, IComparable<TLetter> {
			var provider = new UnicodeCharSetProvider(CreateNamedSets());
			return CreateTokenRegex().ToInvariant(mapper, provider, caseSensitive);
		}

		internal static void CreateStateMachine(out Expression<DfaStateMachine<LetterId, char>> stateMachine, out int startStateId) {
			var mapper = new UnicodeUtf16Mapper(false, false);
			var charRx = CreateRx(mapper, true);
			var alpha = new AlphabetBuilder<char>(charRx, Utf16Chars.EOF, Utf16Chars.ValidBmp);
			var charToLetter = AlphabetMapperEmitter<char>.CreateExpression(alpha);
			var nfa = NfaBuilder<LetterId>.Build(alpha.Expression);
			var dfa = DfaBuilder<LetterId>.Build(nfa, LetterId.Eof);
			stateMachine = DfaStateMachineEmitter.CreateExpression(dfa, charToLetter);
			startStateId = dfa.StartState.Id.ToInt32();
		}

		private RegexLexer(DfaStateMachine<LetterId, char> stateMachine, Id<DfaState<LetterId>> startStateId, bool handleEof, Action<SymbolId, Capture<char>> tokenAction):
				base(stateMachine, startStateId, handleEof, tokenAction) { }
	}
}
