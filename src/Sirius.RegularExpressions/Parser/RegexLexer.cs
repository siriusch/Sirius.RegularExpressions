using System;
using System.Collections.Generic;

using Sirius.Collections;
using Sirius.Unicode;

namespace Sirius.RegularExpressions.Parser {
	internal static class RegexLexer {
		public static readonly SymbolId SymNoise = 0;
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

		public static readonly IReadOnlyDictionary<string, RangeSet<Codepoint>> NamedSets = CreateNamedSets();
		public static readonly RegexExpression TokenRegex = CreateTokenRegex();

		private static IReadOnlyDictionary<string, RangeSet<Codepoint>> CreateNamedSets() {
			var regexPrintable = RangeSet<Codepoint>.Subtract(Codepoints.ValidBmp, UnicodeCharSetProvider.SpaceCharSet);
			return new Dictionary<string, RangeSet<Codepoint>>(StringComparer.Ordinal) {
					{"RegexChar", RangeSet<Codepoint>.Subtract(regexPrintable, RangeSet<Codepoint>.Union(new RangeSet<Codepoint>(@"|/\{}()[].+?*".ToCodepoints()), UnicodeRanges.FromUnicodeName("InCombining_Diacritical_Marks"))) },
					{"RegexCharset", RangeSet<Codepoint>.Subtract(regexPrintable, new RangeSet<Codepoint>(@"\]".ToCodepoints())) },
					{"EscapePrintable", RangeSet<Codepoint>.Subtract(regexPrintable, new RangeSet<Codepoint>(@"PpxUu".ToCodepoints())) },
					{"CharsetPrintable", RangeSet<Codepoint>.Subtract(regexPrintable, new RangeSet<Codepoint>(@"}".ToCodepoints())) },
					{"HexChar", new RangeSet<Codepoint>(@"0123456789AaBbCcDdEeFf".ToCodepoints()) },
					{"Digit", new RangeSet<Codepoint>(@"0123456789".ToCodepoints()) }
			};
		}

		// To solve the chicken-egg problem of the Regex Parser, we specify the regex with the functional API instead of using the parser
		private static RegexExpression CreateTokenRegex() {
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
	}
}
