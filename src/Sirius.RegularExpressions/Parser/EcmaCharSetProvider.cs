using System;
using System.Collections.Generic;
using System.Threading;

using Sirius.Collections;
using Sirius.Unicode;

namespace Sirius.RegularExpressions.Parser {
	public class EcmaCharSetProvider: CharSetProviderBase {
		private static readonly Lazy<RangeSet<Codepoint>> digitCharSet = new Lazy<RangeSet<Codepoint>>(() => new RangeSet<Codepoint>("0123456789".ToCodepoints()), LazyThreadSafetyMode.PublicationOnly);
		private static readonly Lazy<RangeSet<Codepoint>> dotCharSet = new Lazy<RangeSet<Codepoint>>(() => RangeSet<Codepoint>.Subtract(Codepoints.Valid, new RangeSet<Codepoint>("\n".ToCodepoints())), LazyThreadSafetyMode.PublicationOnly);
		private static readonly Lazy<RangeSet<Codepoint>> spaceCharSet = new Lazy<RangeSet<Codepoint>>(() => new RangeSet<Codepoint>(" \f\n\r\t\v​\u00a0\u1680​\u180e\u2000​\u2001\u2002​\u2003\u2004​ \u2005\u2006​\u2007\u2008​\u2009\u200a​\u2028\u2029​​\u202f\u205f​ \u3000".ToCodepoints()), LazyThreadSafetyMode.PublicationOnly);
		private static readonly Lazy<RangeSet<Codepoint>> wordCharSet = new Lazy<RangeSet<Codepoint>>(() => new RangeSet<Codepoint>("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_".ToCodepoints()), LazyThreadSafetyMode.PublicationOnly);

		public EcmaCharSetProvider(IReadOnlyDictionary<string, RangeSet<Codepoint>> namedCharsets) : this(namedCharsets.CreateGetterForIndexer()) { }

		public EcmaCharSetProvider(Func<string, RangeSet<Codepoint>> namedCharsets = null): base(namedCharsets) { }

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
