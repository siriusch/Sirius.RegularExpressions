using System;

namespace Sirius.RegularExpressions.Automata {
	public delegate SymbolId? DfaStateMachine<TLetter, in TInput>(ref Id<DfaState<TLetter>> state, TInput input) where TLetter : struct, IComparable<TLetter>, IEquatable<TLetter>;
}
