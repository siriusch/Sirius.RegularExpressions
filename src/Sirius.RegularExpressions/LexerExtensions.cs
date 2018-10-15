using System;
using System.Linq.Expressions;

using Sirius.Collections;
using Sirius.RegularExpressions.Alphabet;
using Sirius.RegularExpressions.Automata;

namespace Sirius.RegularExpressions {
	public static class LexerExtensions {
		public static Expression<DfaStateMachine<LetterId, TLetter>> CreateStateMachine<TLetter>(this LexerBuilder<TLetter> that, out Id<DfaState<LetterId>> dfaStartState, RangeSet<TLetter> validRanges = null, LetterId? defaultLetter = null)
				where TLetter: struct, IEquatable<TLetter>, IComparable<TLetter> {
			var dfa = that.ComputeDfa(out var alphabetBuilder, validRanges ?? that.ValidRange);
			dfaStartState = dfa.StartState.Id;
			var inputToLetter = AlphabetMapperEmitter<TLetter>.CreateExpression(alphabetBuilder, defaultLetter);
			return DfaStateMachineEmitter.CreateExpression(dfa, inputToLetter);
		}
	}
}
