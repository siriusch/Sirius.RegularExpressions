﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

using Sirius.Collections;
using Sirius.Unicode;

namespace Sirius.RegularExpressions.Parser {
	public class UnicodeCharSetProvider: CharSetProviderBase {
		private static readonly Lazy<RangeSet<Codepoint>> digitCharSet = new Lazy<RangeSet<Codepoint>>(() => UnicodeRanges.FromUnicodeCategory(UnicodeCategory.DecimalDigitNumber), LazyThreadSafetyMode.PublicationOnly);
		private static readonly Lazy<RangeSet<Codepoint>> dotCharSet = new Lazy<RangeSet<Codepoint>>(() => RangeSet<Codepoint>.Subtract(new RangeSet<Codepoint>(Range<Codepoint>.Create(Codepoint.MinValue, Codepoint.MaxValue)), Codepoint.Surrogates), LazyThreadSafetyMode.PublicationOnly);
		private static readonly Lazy<RangeSet<Codepoint>> spaceCharSet = new Lazy<RangeSet<Codepoint>>(() => RangeSet<Codepoint>.Union(new RangeSet<Codepoint>("\r\n".ToCodepoints()), UnicodeRanges.FromUnicodeName("Separator")), LazyThreadSafetyMode.PublicationOnly);
		private static readonly Lazy<RangeSet<Codepoint>> wordCharSet = new Lazy<RangeSet<Codepoint>>(() => RangeSet<Codepoint>.Union(new RangeSet<Codepoint>('_'), UnicodeRanges.FromUnicodeName("Letter"), UnicodeRanges.FromUnicodeCategory(UnicodeCategory.DecimalDigitNumber)), LazyThreadSafetyMode.PublicationOnly);

		public UnicodeCharSetProvider(Func<string, RangeSet<Codepoint>> namedCharsets): base(namedCharsets) { }

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