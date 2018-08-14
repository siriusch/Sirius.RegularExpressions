using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

using Sirius.Collections;
using Sirius.Unicode;

namespace Sirius.RegularExpressions.Parser {
	public class EcmaCharSetProvider: CharSetProviderBase {
		private static readonly Lazy<RangeSet<Codepoint>> digitCharSet = new Lazy<RangeSet<Codepoint>>(() => new RangeSet<Codepoint>("0123456789".ToCodepoints()), LazyThreadSafetyMode.PublicationOnly);
		private static readonly Lazy<RangeSet<Codepoint>> dotCharSet = new Lazy<RangeSet<Codepoint>>(() => RangeSet<Codepoint>.Negate(new RangeSet<Codepoint>("\n".ToCodepoints())), LazyThreadSafetyMode.PublicationOnly);
		private static readonly Lazy<RangeSet<Codepoint>> spaceCharSet = new Lazy<RangeSet<Codepoint>>(() => RangeSet<Codepoint>.Union(new RangeSet<Codepoint>(" \r\n\t".ToCodepoints()), UnicodeRanges.FromUnicodeCategory(UnicodeCategory.SpaceSeparator)), LazyThreadSafetyMode.PublicationOnly);
		private static readonly Lazy<RangeSet<Codepoint>> wordCharSet = new Lazy<RangeSet<Codepoint>>(() => new RangeSet<Codepoint>("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_".ToCodepoints()), LazyThreadSafetyMode.PublicationOnly);

		public EcmaCharSetProvider(Func<string, RangeSet<Codepoint>> namedCharsets): base(namedCharsets) { }

		public override RangeSet<Codepoint> GetClassSet(CharSetClass cls) {
			switch (cls) {
			case CharSetClass.Digit:
				return digitCharSet.Value;
			case CharSetClass.Dot:
				return dotCharSet.Value;
			case CharSetClass.Space:
				return spaceCharSet.Value;
			case CharSetClass.Word:
				return wordCharSet.Value;
			}
			throw new KeyNotFoundException("Invalid character set class");
		}
	}
}
