using System;

using Sirius.Collections;
using Sirius.Unicode;

namespace Sirius.RegularExpressions.Parser {
	public interface IRangeSetProvider<TItem>
			where TItem: IComparable<TItem> {
		RangeSet<TItem> GetRangeSet(RangeSet<Codepoint> data);

		RangeSet<TItem> GetRangeSet(RangeSetCategory category, string data);

		RangeSet<TItem> Union(RangeSet<TItem> set1, RangeSet<TItem> set2);
	}
}
