using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

using Sirius.Collections;
using Sirius.Unicode;

namespace Sirius.RegularExpressions.Parser {
	public class RegexMatchSet: RegexExpression {
		private static readonly Regex rxEscape = new Regex(@"^\\((?<c>[Pp])((?<name>\p{L})|\{(?<name>[^\}]+)\})|x(?<hex>[0-9A-Fa-f]{2})|u(?<hex>[0-9A-Fa-f]{4})|U(?<hex>[0-9A-Fa-f]{8})|(?<c>[^Ppux]))$", RegexOptions.CultureInvariant|RegexOptions.ExplicitCapture|RegexOptions.Singleline);
		private static readonly Regex rxNamed = new Regex(@"^\{\s*(?<name>.+)\s*\}$", RegexOptions.CultureInvariant|RegexOptions.ExplicitCapture|RegexOptions.Singleline);

		private static readonly Regex rxSet = new Regex(
			@"^\[(?<neg>\^)?((?<from>\\([Pp](\p{L}|\{[^\}]+\})|x[0-9A-Fa-f]{2}|u[0-9A-Fa-f]{4}|U[0-9A-Fa-f]{8}|[^Ppux])|[^\\\]\p{M}*])-(?<to>\\([Pp](\p{L}|\{[^\}]+\})|x[0-9A-Fa-f]{2}|u[0-9A-Fa-f]{4}|U[0-9A-Fa-f]{8}|[^Ppux])|[^\\\]\p{M}*])|(?<letter>\\([Pp](\p{L}|\{[^\}]+\})|x[0-9A-Fa-f]{2}|u[0-9A-Fa-f]{4}|U[0-9A-Fa-f]{8}|[^Ppux])|[^\\\]\p{M}*]))*\]$",
			RegexOptions.CultureInvariant|RegexOptions.ExplicitCapture|RegexOptions.Singleline);

		private static bool EqualsInternal(RegexMatchSet x, RegexMatchSet y) {
			return Equals(x.Handle, y.Handle);
		}

		public static RegexMatchSet FromClass(CharSetClass charSetClass) {
			return new RegexMatchSet(".", new RangeSetHandle.Class(charSetClass, false));
		}

		public static RegexMatchSet Dot() {
			return FromClass(CharSetClass.Dot);
		}

		public static RegexMatchSet FromEscape(string escape) {
			return new RegexMatchSet(escape, ParseEscape(escape));
		}

		public static RegexMatchSet FromNamedCharset(string charset) {
			var match = rxNamed.Match(charset);
			if (!match.Success) {
				throw new ArgumentException("Named charset format is invalid", "charset");
			}
			return new RegexMatchSet(charset, new RangeSetHandle.Named(match.Groups["name"].Value, false));
		}

		public static RegexMatchSet FromSet(string set) {
			var match = rxSet.Match(set);
			if (!match.Success) {
				throw new ArgumentException("Set is invalid", "set");
			}
			var handle = new RangeSetHandle.Union(match.Groups["neg"].Success);
			foreach (Capture capture in match.Groups["letter"].Captures) {
				handle.Add((capture.Value[0] == '\\') ? ParseEscape(capture.Value) : new RangeSetHandle.Static(capture.Value.ToCodepoints().Single()));
			}
			var from = match.Groups["from"].Captures;
			var to = match.Groups["to"].Captures;
			Debug.Assert(from.Count == to.Count);
			for (var i = 0; i < from.Count; i++) {
				handle.Add(new RangeSetHandle.Static(ParseSingleLetter(from[i].Value), ParseSingleLetter(to[i].Value)));
			}
			return new RegexMatchSet(set, handle.Simplify());
		}

		public static RegexMatchSet FromCodepoints(IEnumerable<Codepoint> codepoints, bool negate = false) {
			return new RegexMatchSet($"[{codepoints.AsString()}]", new RangeSetHandle.Static(new RangeSet<Codepoint>(codepoints), negate));
		}

		public static RegexMatchSet FromCodepoints(params Codepoint[] codepoints) {
			return FromCodepoints((IEnumerable<Codepoint>)codepoints);
		}

		public static RegexMatchSet FromChars(params char[] chars) {
			return FromCodepoints(chars.ToCodepoints());
		}

		public static RegexMatchSet FromUnicode(string name, bool negate = false) {
			return new RegexMatchSet($@"\{(negate ? 'P' : 'p')}{{{name}}}", new RangeSetHandle.Static(UnicodeRanges.FromUnicodeName(name), negate));
		}

		internal static RangeSetHandle ParseEscape(string escape) {
			var match = rxEscape.Match(escape);
			if (!match.Success) {
				throw new ArgumentException("Escape is invalid", "escape");
			}
			if (match.Groups["name"].Success) {
				return new RangeSetHandle.Static(UnicodeRanges.FromUnicodeName(match.Groups["name"].Value), match.Groups["c"].Value == "P");
			}
			if (match.Groups["hex"].Success) {
				return new RangeSetHandle.Static(Codepoint.Parse(match.Groups["hex"].Value));
			}
			var c = match.Groups["c"].Value[0];
			switch (c) {
			case '0':
				return new RangeSetHandle.Static('\0');
			case 'r':
				return new RangeSetHandle.Static('\r');
			case 'n':
				return new RangeSetHandle.Static('\n');
			case 't':
				return new RangeSetHandle.Static('\t');
			case 'a':
				return new RangeSetHandle.Static('\x07');
			case 'e':
				return new RangeSetHandle.Static('\x1B');
			case 'f':
				return new RangeSetHandle.Static('\x0C');
			case 'v':
				return new RangeSetHandle.Static('\x0B');
			case 'd':
				return new RangeSetHandle.Class(CharSetClass.Digit, false);
			case 'D':
				return new RangeSetHandle.Class(CharSetClass.Digit, true);
			case 'w':
				return new RangeSetHandle.Class(CharSetClass.Word, false);
			case 'W':
				return new RangeSetHandle.Class(CharSetClass.Word, true);
			case 's':
				return new RangeSetHandle.Class(CharSetClass.Space, false);
			case 'S':
				return new RangeSetHandle.Class(CharSetClass.Space, true);
			default:
				if (char.IsLetterOrDigit(c)) {
					throw new ArgumentOutOfRangeException(nameof(escape), "Invalid escape character "+c);
				}
				return new RangeSetHandle.Static(c);
			}
		}

		private static Codepoint ParseSingleLetter(string letter) {
			if (letter[0] == '\\') {
				var handle = ParseEscape(letter) as RangeSetHandle.Static;
				Codepoint result;
				if ((handle == null) || !handle.TryGetSingle(out result)) {
					throw new InvalidOperationException("A single character was expected, but a character class was found");
				}
				return result;
			}
			return letter.Single();
		}

		public RegexMatchSet(string text, RangeSetHandle handle) {
			this.Handle = handle;
			this.Text = text;
		}

		public string Text {
			get;
		}

		public RangeSetHandle Handle {
			get;
		}

		public override TResult Visit<TContext, TResult>(IRegexVisitor<TContext, TResult> visitor, TContext context) {
			return visitor.MatchSet(this, context);
		}
	}
}
