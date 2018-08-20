// bsn Parser
// ----------
// 
// Copyright 2014 by Arsène von Wyss - avw@gmx.ch
// 
// Development has been supported by Sirius Technologies AG, Basel
// 
// Source:
// 
// https://bsn.kilnhg.com/Code/Parser/Trunk/Source
// 
// License:
// 
// The library is distributed under the GNU Lesser General Public License:
// http://www.gnu.org/licenses/lgpl.html
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Diagnostics;
using System.Linq;

using bsn.Parser.RegularExpressions.Alphabet;

using Xunit;
using Xunit.Extensions;

namespace bsn.Parser.RegularExpressions.Parser {
	public class AlphabetBuilderTest {
		[Theory]
		[InlineData(3, "1")]
		[InlineData(4, "12")]
		[InlineData(4, "1[12]2")]
		[InlineData(4, "1[12]2[12]1")]
		[InlineData(3, "[0-9]")]
		[InlineData(12, "0123456789")]
		public void GenerateAlphabet(int expectedCount, string regex) {
			Debug.WriteLine(regex);
			var expression = RegexParser.Parse(regex);
			var regexAlphabetBuilder = new RegexAlphabetBuilder(expression, new UnicodeCharSetProvider(null), CharSetTransformer.CaseSensitive);
			foreach (var entry in regexAlphabetBuilder.AlphabetEntries.OrderBy(ae => ae.Id)) {
				Debug.WriteLine("{0}: {1}", entry.Id, entry.Id == 0 ? "EOF" : string.Join(", ", entry.Ranges.Select(r => (r.From == r.To) ? new string(r.From, 1) : r.From+".."+r.To)));
			}
			Assert.Equal(expectedCount, regexAlphabetBuilder.AlphabetEntries.Count);
		}
	}
}
