using System;
using System.Collections.Generic;

namespace Sirius.RegularExpressions.Parser {
	public interface IAlphabetFor<T>
			where T: class {
		int LetterCount {
			get;
		}

		IEnumerable<LetterId> GetLetterTransitions(Id<T> id);
	}
}
