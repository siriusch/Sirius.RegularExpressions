using System;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Sirius.RegularExpressions.Automata {
	public static class DfaStateMachineEmitter {
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool LetterComparison<TLetter>(TLetter x, TLetter y) where TLetter: struct, IEquatable<TLetter> {
			return x.Equals(y);
		}

		public static Expression<DfaStateMachine<TLetter>> CreateExpression<TLetter>(Dfa<TLetter> dfa) where TLetter : struct, IComparable<TLetter>, IEquatable<TLetter> {
			var ctor_IdDfaStateLetter = Reflect.GetConstructor(() => new Id<DfaState<TLetter>>(default(int)));
			var ctor_SymbolId = Reflect.GetConstructor(() => new SymbolId(default(int)));
			var ctor_Nullable = Reflect.GetConstructor(() => new SymbolId?(default(SymbolId)));
			var meth_TLetter_Comparison = typeof(TLetter).IsPrimitive ? null : Reflect.GetStaticMethod(() => LetterComparison(default(TLetter), default(TLetter)));
			var newInvalidOperationException = Reflect.AsExpression(() => new InvalidOperationException());
			var paramState = Expression.Parameter(typeof(Id<DfaState<TLetter>>).MakeByRefType(), "state");
			var paramInput = Expression.Parameter(typeof(TLetter), "input");
			return Expression.Lambda<DfaStateMachine<TLetter>>(
					Expression.Switch(
							Expression.Call(paramState, Reflect<Id<DfaState<TLetter>>>.GetMethod(i => i.ToInt32())),
							Expression.Throw(newInvalidOperationException, typeof(SymbolId?)),
							dfa.States.OrderBy(s => s.Id).Select(state => Expression.SwitchCase(
									Expression.Block(
											Expression.Assign(
													paramState,
													Expression.New(ctor_IdDfaStateLetter,
															Expression.Switch(
																	paramInput,
																	Expression.Constant(Dfa<TLetter>.Reject.ToInt32()),
																	meth_TLetter_Comparison,
																	state
																			.Transitions
																			.GroupBy(p => p.Value.ToInt32(), p => p.Key)
																			.Select(g => Expression.SwitchCase(Expression.Constant(g.Key), g.SelectMany(r => r.Expand()).Select(i => Expression.Constant(i))))
																			.ToArray()))),
											dfa.SymbolStates.TryGetValue(state.Id, out var symbolId)
													? (Expression)Expression.New(ctor_Nullable, Expression.New(ctor_SymbolId, Expression.Constant((int)symbolId)))
													: Expression.Default(typeof(SymbolId?))),
									Expression.Constant(state.Id.ToInt32())
							)).ToArray()), "DfaGenericStateMachine", new[] {paramState, paramInput});
		}

		public static Expression<DfaStateMachine<LetterId>> CreateExpression(Dfa<LetterId> dfa) {
			var ctor_IdDfaStateLetter = Reflect.GetConstructor(() => new Id<DfaState<LetterId>>(default(int)));
			var ctor_SymbolId = Reflect.GetConstructor(() => new SymbolId(default(int)));
			var ctor_Nullable = Reflect.GetConstructor(() => new SymbolId?(default(SymbolId)));
			var meth_LetterId_ToInt32 = Reflect<LetterId>.GetMethod(i => i.ToInt32());
			var newInvalidOperationException = Reflect.AsExpression(() => new InvalidOperationException());
			var paramState = Expression.Parameter(typeof(Id<DfaState<LetterId>>).MakeByRefType(), "state");
			var paramInput = Expression.Parameter(typeof(LetterId), "input");
			return Expression.Lambda<DfaStateMachine<LetterId>>(
					Expression.Switch(
							Expression.Call(paramState, Reflect<Id<DfaState<LetterId>>>.GetMethod(i => i.ToInt32())),
							Expression.Throw(newInvalidOperationException, typeof(SymbolId?)),
							dfa.States.OrderBy(s => s.Id).Select(state => Expression.SwitchCase(
									Expression.Block(
											Expression.Assign(
													paramState,
													Expression.New(ctor_IdDfaStateLetter,
															Expression.Switch(
																	Expression.Call(paramInput, meth_LetterId_ToInt32),
																	Expression.Constant(Dfa<LetterId>.Reject.ToInt32()),
																	state
																			.Transitions
																			.GroupBy(p => p.Value.ToInt32(), p => p.Key)
																			.Select(g => Expression.SwitchCase(Expression.Constant(g.Key), g.SelectMany(r => r.Expand()).Select(i => Expression.Constant(i.ToInt32()))))
																			.ToArray()))),
											dfa.SymbolStates.TryGetValue(state.Id, out var symbolId)
													? (Expression)Expression.New(ctor_Nullable, Expression.New(ctor_SymbolId, Expression.Constant((int)symbolId)))
													: Expression.Default(typeof(SymbolId?))),
									Expression.Constant(state.Id.ToInt32())
							)).ToArray()), "DfaLetterStateMachine", new[] {paramState, paramInput});
		}
	}
}
