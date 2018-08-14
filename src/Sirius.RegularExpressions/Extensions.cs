using System;
using System.Collections.Generic;
using System.Linq;

using Sirius.Unicode;

namespace Sirius.RegularExpressions {
	public static class Extensions {

		//		public static LetterId ReadFromReader(this Alphabet<string> that, TextReader reader) {
		//			var ch = reader.Read();
		//			if (ch < 0) {
		//				Debug.WriteLine("(EOF)");
		//				return Alphabet<string>.EOF;
		//			}
		//			var s = ((char)ch).ToString();
		//			if (char.IsHighSurrogate((char)ch)) {
		//				s = s+((char)reader.Read());
		//			}
		//			Debug.WriteLine(s);
		//			return that[s];
		//		}
	}
}
