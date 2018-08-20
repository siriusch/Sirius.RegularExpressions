using System;

using Sirius.Collections;

namespace Sirius.RegularExpressions.Alphabet {
	public class AlphabetLetter<T>
			where T: IComparable<T> {
		public AlphabetLetter(LetterId id, RangeSet<T> ranges) {
			this.Id = id;
			this.Ranges = ranges;
		}

		public LetterId Id {
			get;
		}

		public RangeSet<T> Ranges {
			get;
		}
	}
}
