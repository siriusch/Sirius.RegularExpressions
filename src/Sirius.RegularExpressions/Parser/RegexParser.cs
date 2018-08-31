using System;
using System.Collections.Generic;

using Sirius.Collections;
using Sirius.Unicode;

namespace Sirius.RegularExpressions.Parser {
	public class RegexParser {
		public bool IgnorePatternWhitespace {
			get;
		}

		private class RegexGroup {
			private readonly RegexGroup parent;
			private readonly bool? caseSensitive;
			private RegexExpression alternate;
			private RegexExpression current;
			private RegexExpression pending;

			public RegexGroup(RegexGroup parent, bool? caseSensitive=null) {
				this.parent = parent;
				this.caseSensitive = caseSensitive;
				this.current = RegexNoOp.Default;
				this.pending = null;
			}

			public bool IsRoot => this.parent == null;

			public void Alternate() {
				this.alternate = this.Flush();
				this.current = RegexNoOp.Default;
				this.pending = null;
			}

			public void AppendExpression(RegexExpression expression) {
				if (this.pending != null) {
					this.current = RegexConcatenation.Create(this.current, this.pending);
				}
				this.pending = expression;
			}

			public RegexGroup BeginGroup(bool? caseSensitive) {
				return new RegexGroup(this, caseSensitive);
			}

			public RegexGroup EndGroup() {
				this.parent.AppendExpression(this.Flush());
				return this.parent;
			}

			public RegexExpression Flush() {
				AppendExpression(null);
				var expression = this.alternate != null ? RegexAlternation.Create(this.alternate, this.current) : this.current;
				if (this.caseSensitive.HasValue) {
					return this.caseSensitive.Value ? RegexCaseGroup.CreateSensitive(expression) : RegexCaseGroup.CreateInsensitive(expression);
				}
				return expression;
			}

			public void Quantify(RegexQuantifier quantifier) {
				if (this.pending == null) {
					throw new InvalidOperationException("Qualifier is not valid at this place");
				}
				this.pending = RegexQuantified.Create(this.pending, quantifier);
				AppendExpression(null);
			}
		}

		public static RegexExpression Parse(string regex, SymbolId? acceptSymbol, bool ignorePatternWhitespace = true) {
			var state = new RegexParser(null, ignorePatternWhitespace);
			RegexLexer.Create(state.ProcessTerminal).Push(regex.Append(Utf16Chars.EOF));
			var result = state.Flush();
			return acceptSymbol.HasValue ? RegexAccept.Create(result, acceptSymbol.Value) : result;
		}

		private readonly Action<RegexExpression> parseCallback;
		private RegexGroup currentGroup;

		public RegexParser(Action<RegexExpression> parseCallback = null, bool ignorePatternWhitespace = true) {
			this.IgnorePatternWhitespace = ignorePatternWhitespace;
			this.parseCallback = parseCallback;
			this.currentGroup = new RegexGroup(null);
		}

		public RegexExpression Flush() {
			return this.currentGroup.Flush();
		}

		private void ProcessTerminal(SymbolId symbol, IEnumerable<char> data, long index) {
			switch ((int)symbol) {
			case RegexLexer.SymWhitespace:
				if (!this.IgnorePatternWhitespace) {
					foreach (var ch in data) {
						this.currentGroup.AppendExpression(RegexMatchSet.FromChars(ch));
					}
				}
				break;
			case RegexLexer.SymCharset:
				this.currentGroup.AppendExpression(RegexMatchSet.FromNamedCharset(data.AsString()));
				break;
			case RegexLexer.SymRegexLetter:
				this.currentGroup.AppendExpression(RegexMatchGrapheme.Create(data.AsString()));
				break;
			case RegexLexer.SymRegexEscape:
				this.currentGroup.AppendExpression(RegexMatchSet.FromEscape(data.AsString()));
				break;
			case RegexLexer.SymRegexCharset:
				this.currentGroup.AppendExpression(RegexMatchSet.FromSet(data.AsString()));
				break;
			case RegexLexer.SymRegexDot:
				this.currentGroup.AppendExpression(RegexMatchSet.Dot());
				break;
			case RegexLexer.SymQuantifyKleene:
				this.currentGroup.Quantify(RegexQuantifier.Kleene());
				break;
			case RegexLexer.SymQuantifyAny:
				this.currentGroup.Quantify(RegexQuantifier.Any());
				break;
			case RegexLexer.SymQuantifyOptional:
				this.currentGroup.Quantify(RegexQuantifier.Optional());
				break;
			case RegexLexer.SymQuantifyRepeat:
				this.currentGroup.Quantify(RegexQuantifier.Repeat(data.AsString()));
				break;
			case RegexLexer.SymAlternate:
				this.currentGroup.Alternate();
				break;
			case RegexLexer.SymBeginGroup:
				this.currentGroup = this.currentGroup.BeginGroup(null);
				break;
			case RegexLexer.SymSensitiveGroup:
				this.currentGroup = this.currentGroup.BeginGroup(true);
				break;
			case RegexLexer.SymInsensitiveGroup:
				this.currentGroup = this.currentGroup.BeginGroup(false);
				break;
			case RegexLexer.SymEndGroup:
				this.currentGroup = this.currentGroup.EndGroup();
				break;
			case int.MinValue: // EOF
				if (!this.currentGroup.IsRoot) {
					throw new InvalidOperationException("Regex has open groups");
				}
				this.parseCallback?.Invoke(this.Flush());
				break;
			default:
				throw new NotSupportedException("Unexpected symbol at index "+index);
			}
		}

	}
}
