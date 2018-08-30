using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

using Sirius.Collections;

namespace Sirius.RegularExpressions.Parser {
	public sealed class RegexQuantifier: IEquatable<RegexQuantifier> {
		private static readonly Regex rxQuantifier = new Regex(@"^\{(?<min>[0-9]+)(?<upper>,(?<max>[0-9]+)?)?\}$", RegexOptions.Compiled|RegexOptions.CultureInvariant|RegexOptions.ExplicitCapture);
		private static readonly RegexQuantifier any = new RegexQuantifier(1, default(int?));
		private static readonly RegexQuantifier kleene = new RegexQuantifier(0, default(int?));
		private static readonly RegexQuantifier optional = new RegexQuantifier(0, 1);

		public static RegexQuantifier Any() {
			return any;
		}

		private static bool EqualsInternal(RegexQuantifier x, RegexQuantifier y) {
			return (x.Min == y.Min) && (x.Max == y.Max);
		}

		public static RegexQuantifier Kleene() {
			return kleene;
		}

		public static RegexQuantifier Optional() {
			return optional;
		}

		public static RegexQuantifier Repeat(string quantifier) {
			var match = rxQuantifier.Match(quantifier);
			var min = XmlConvert.ToInt32(match.Groups["min"].Value);
			var max = match.Groups["upper"].Success ? match.Groups["max"].Success ? XmlConvert.ToInt32(match.Groups["max"].Value) : default(int?) : min;
			return Create(min, max);
		}

		public static RegexQuantifier MinOccurs(int min) {
			return Create(min, null);
		}

		public static RegexQuantifier MaxOccurs(int max) {
			return Create(0, max);
		}

		public static RegexQuantifier Occurs(int count) {
			return Create(count, count);
		}

		public static RegexQuantifier Create(int min, int? max) {
			if (!max.HasValue) {
				if (min == 0) {
					return kleene;
				}
				if (min == 1) {
					return any;
				}
			} else if (min == 0 && max.Value == 1) {
				return optional;
			}
			return new RegexQuantifier(min, max);
		}

		private RegexQuantifier(int min, int? max) {
			if (min < 0) {
				throw new ArgumentOutOfRangeException("min");
			}
			if (max.HasValue) {
				if (max < 0) {
					throw new ArgumentOutOfRangeException("max");
				}
				if (max.Value < min) {
					throw new ArgumentException("The max count cannot be smaller then the min count");
				}
			}
			this.Min = min;
			this.Max = max;
		}

		public bool IsFixedLength => this.Min == this.Max;

		public bool IsOne => (this.Min == 1) && (this.Max == 1);

		public bool IsZero => this.Max == 0;

		public int? Max {
			get;
		}

		public int Min {
			get;
		}

		public bool Equals(RegexQuantifier other) {
			return this.InstanceEquals(other, EqualsInternal);
		}

		public override string ToString() {
			switch (this.Min) {
			case 0:
				switch (this.Max) {
				case 1:
					return "?";
				case null:
					return "*";
				}
				break;
			case 1:
				if (!this.Max.HasValue) {
					return "+";
				}
				break;
			}
			var result = new StringBuilder(10);
			result.Append('{');
			result.Append(XmlConvert.ToString(this.Min));
			if (this.Max != this.Min) {
				result.Append(',');
				if (this.Max.HasValue) {
					result.Append(XmlConvert.ToString(this.Max.Value));
				}
			}
			result.Append('}');
			return result.ToString();
		}
	}
}
