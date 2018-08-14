using System;
using System.Collections.Generic;
using System.Linq;

using Sirius.Collections;
using Sirius.Unicode;

using Xunit;

namespace Sirius.RegularExpressions.RegularExpressions {
	public class CharSetTest {
		private static void PrepareCharSets(out RangeSet<Codepoint> x, out RangeSet<Codepoint> y) {
			x = new RangeSet<Codepoint>("abdegh".ToCodepoints());
			y = new RangeSet<Codepoint>(Range<Codepoint>.Create('e', 'z'));
		}

		[Fact]
		public void CaseSensivity() {
			var charSet = new RangeSet<Codepoint>("aB".ToCodepoints());
			Assert.True(charSet.Contains('a'));
			Assert.False(charSet.Contains('A'));
			Assert.False(charSet.Contains('b'));
			Assert.True(charSet.Contains('B'));
			Assert.False(charSet.Contains('c'));
			Assert.False(charSet.Contains('C'));
			charSet = charSet.CaseInsensitive();
			Assert.True(charSet.Contains('a'));
			Assert.True(charSet.Contains('A'));
			Assert.True(charSet.Contains('b'));
			Assert.True(charSet.Contains('B'));
			Assert.False(charSet.Contains('c'));
			Assert.False(charSet.Contains('C'));
		}

		[Theory]
		[InlineData('0', false)]
		[InlineData('1', true)]
		[InlineData('2', true)]
		[InlineData('3', true)]
		[InlineData('4', false)]
		[InlineData('5', true)]
		[InlineData('6', false)]
		[InlineData('7', true)]
		[InlineData('8', true)]
		[InlineData('9', false)]
		public void Contains(char c, bool isSet) {
			var charSet = new RangeSet<Codepoint>(new[] { Range<Codepoint>.Create('1', '3'), Range<Codepoint>.Create('5', '5'), Range<Codepoint>.Create('7', '8') });
			Assert.Equal(isSet, charSet.Contains(c));
		}

		[Fact]
		public void RangeDiffEnum() {
			RangeSet<Codepoint> x;
			RangeSet<Codepoint> y;
			PrepareCharSets(out x, out y);
			Assert.Equal(new[] {
				new KeyValuePair<Range<Codepoint>, ContainedIn>(Range<Codepoint>.Create('a', 'b'), ContainedIn.Left),
				new KeyValuePair<Range<Codepoint>, ContainedIn>(Range<Codepoint>.Create('d', 'd'), ContainedIn.Left),
				new KeyValuePair<Range<Codepoint>, ContainedIn>(Range<Codepoint>.Create('e', 'e'), ContainedIn.Both),
				new KeyValuePair<Range<Codepoint>, ContainedIn>(Range<Codepoint>.Create('f', 'f'), ContainedIn.Right),
				new KeyValuePair<Range<Codepoint>, ContainedIn>(Range<Codepoint>.Create('g', 'h'), ContainedIn.Both),
				new KeyValuePair<Range<Codepoint>, ContainedIn>(Range<Codepoint>.Create('i', 'z'), ContainedIn.Right)
			}, RangeSet<Codepoint>.EnumerateRanges(x, y));
			Assert.Equal(new[] {
				new KeyValuePair<Range<Codepoint>, ContainedIn>(Range<Codepoint>.Create('a', 'b'), ContainedIn.Right),
				new KeyValuePair<Range<Codepoint>, ContainedIn>(Range<Codepoint>.Create('d', 'd'), ContainedIn.Right),
				new KeyValuePair<Range<Codepoint>, ContainedIn>(Range<Codepoint>.Create('e', 'e'), ContainedIn.Both),
				new KeyValuePair<Range<Codepoint>, ContainedIn>(Range<Codepoint>.Create('f', 'f'), ContainedIn.Left),
				new KeyValuePair<Range<Codepoint>, ContainedIn>(Range<Codepoint>.Create('g', 'h'), ContainedIn.Both),
				new KeyValuePair<Range<Codepoint>, ContainedIn>(Range<Codepoint>.Create('i', 'z'), ContainedIn.Left)
			}, RangeSet<Codepoint>.EnumerateRanges(y, x));
		}

		[Fact]
		public void RangeEmptyEnum() {
			RangeSet<Codepoint> x;
			RangeSet<Codepoint> y;
			PrepareCharSets(out x, out y);
			Assert.Equal(x.Select(r => new KeyValuePair<Range<Codepoint>, ContainedIn>(r, ContainedIn.Left)), RangeSet<Codepoint>.EnumerateRanges(x, RangeSet<Codepoint>.Empty));
			Assert.Equal(y.Select(r => new KeyValuePair<Range<Codepoint>, ContainedIn>(r, ContainedIn.Left)), RangeSet<Codepoint>.EnumerateRanges(y, RangeSet<Codepoint>.Empty));
			Assert.Equal(x.Select(r => new KeyValuePair<Range<Codepoint>, ContainedIn>(r, ContainedIn.Right)), RangeSet<Codepoint>.EnumerateRanges(RangeSet<Codepoint>.Empty, x));
			Assert.Equal(y.Select(r => new KeyValuePair<Range<Codepoint>, ContainedIn>(r, ContainedIn.Right)), RangeSet<Codepoint>.EnumerateRanges(RangeSet<Codepoint>.Empty, y));
		}

		[Fact]
		public void RangeSelfEnum() {
			RangeSet<Codepoint> x;
			RangeSet<Codepoint> y;
			PrepareCharSets(out x, out y);
			Assert.Equal(x.Select(r => new KeyValuePair<Range<Codepoint>, ContainedIn>(r, ContainedIn.Both)), RangeSet<Codepoint>.EnumerateRanges(x, x));
			Assert.Equal(y.Select(r => new KeyValuePair<Range<Codepoint>, ContainedIn>(r, ContainedIn.Both)), RangeSet<Codepoint>.EnumerateRanges(y, y));
		}

		[Fact]
		public void SetOne() {
			var charSet = new RangeSet<Codepoint>("abc".ToCodepoints());
			Assert.Equal(new[] { Range<Codepoint>.Create('a', 'c') }, charSet);
		}

		[Fact]
		public void SetTwo() {
			var charSet = new RangeSet<Codepoint>("abcxyz".ToCodepoints());
			Assert.Equal(new[] { Range<Codepoint>.Create('a', 'c'), Range<Codepoint>.Create('x', 'z') }, charSet);
		}

		[Fact]
		public void UnionComplement1() {
			var charSetA = new RangeSet<Codepoint>('a');
			var charSet = RangeSet<Codepoint>.Union(charSetA, RangeSet<Codepoint>.Negate(charSetA));
			Assert.Equal(new[] { Range<Codepoint>.Create(Codepoint.MinValue, Codepoint.MaxValue) }, charSet);
		}

		[Fact]
		public void UnionComplement2() {
			var charSetA = new RangeSet<Codepoint>('a');
			var charSetNotA = RangeSet<Codepoint>.Difference(RangeSet<Codepoint>.All, charSetA);
			var charSet = RangeSet<Codepoint>.Union(charSetNotA, charSetA);
			Assert.Equal(new[] { Range<Codepoint>.Create(Codepoint.MinValue, Codepoint.MaxValue) }, charSet);
		}
	}
}
