using System;

namespace Sirius.RegularExpressions.Automata {
	public delegate SymbolId? DfaStateMachine<TLetter>(ref Id<DfaState<TLetter>> state, TLetter input) where TLetter : struct, IComparable<TLetter>, IEquatable<TLetter>;
}