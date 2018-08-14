using System;
using System.Linq;

using Sirius.Collections;
using Sirius.Unicode;

namespace Sirius.RegularExpressions.Parser {
	public abstract class CharSetProviderBase: IRangeSetProvider<Codepoint> {
		private readonly Func<string, RangeSet<Codepoint>> namedCharsets;

		protected CharSetProviderBase(Func<string, RangeSet<Codepoint>> getNamedCharset) {
			this.namedCharsets = getNamedCharset;
		}

		public RangeSet<Codepoint> GetRangeSet(RangeSet<Codepoint> data) {
			return data;
		}

		public RangeSet<Codepoint> GetRangeSet(RangeSetCategory category, string data) {
			switch (category) {
			case RangeSetCategory.Name:
				return GetNamedSet(data);
			case RangeSetCategory.Class:
				return GetClassSet((CharSetClass)Enum.Parse(typeof(CharSetClass), data, false));
			}
			throw new ArgumentOutOfRangeException("category");
		}

		public RangeSet<Codepoint> Union(RangeSet<Codepoint> set1, RangeSet<Codepoint> set2) {
			return RangeSet<Codepoint>.Intersection(Codepoints.Valid, RangeSet<Codepoint>.Union(set1, set2));
		}

		public abstract RangeSet<Codepoint> GetClassSet(CharSetClass cls);

		public virtual RangeSet<Codepoint> GetNamedSet(string name) {
			return this.namedCharsets(name);
		}
	}
}
