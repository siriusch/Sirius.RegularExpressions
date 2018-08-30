using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Sirius.RegularExpressions.Automata {
	public static class DfaStateMachineEmitter {
		private static readonly ConstructorInfo ctor_LetterId = Reflect.GetConstructor(() => new LetterId(default(int)));
		private static readonly ConstructorInfo ctor_SymbolId = Reflect.GetConstructor(() => new SymbolId(default(int)));
		private static readonly ConstructorInfo ctor_NullableSymbolId = Reflect.GetConstructor(() => new SymbolId?(default(SymbolId)));
		private static readonly Expression new_InvalidOperationException = Expression.New(Reflect.GetConstructor(() => new InvalidOperationException()));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool LetterComparison<TLetter>(TLetter x, TLetter y)
				where TLetter: struct, IEquatable<TLetter> {
			return x.Equals(y);
		}

		public static Expression<DfaStateMachine<LetterId, LetterId>> CreateExpression(Dfa<LetterId> dfa) {
			var paramState = Expression.Parameter(typeof(Id<DfaState<LetterId>>).MakeByRefType(), "state");
			var paramInput = Expression.Parameter(typeof(LetterId), "input");
			return Expression.Lambda<DfaStateMachine<LetterId, LetterId>>(CreateLetterStateMachine(dfa, paramState, paramInput), "DfaLetterStateMachine", new[] {paramState, paramInput});
		}

		private static Expression CreateLetterStateMachine(Dfa<LetterId> dfa, ParameterExpression paramState, Expression exprInput) {
			var ctor_IdDfaStateLetter = Reflect.GetConstructor(() => new Id<DfaState<LetterId>>(default(int)));
			var meth_LetterId_ToInt32 = Reflect<LetterId>.GetMethod(i => i.ToInt32());
			var varInput = Expression.Variable(typeof(int), "input");
			var varResult = Expression.Variable(typeof(SymbolId?), "result");
			return Expression.Block(
					typeof(SymbolId?),
					new[] {varInput, varResult},
					Expression.Assign(varInput, exprInput is NewExpression exprInputNew && (exprInputNew.Constructor == ctor_LetterId)
							? exprInputNew.Arguments.Single()
							: Expression.Call(exprInput, meth_LetterId_ToInt32)),
					Expression.Assign(
							paramState,
							Expression.New(ctor_IdDfaStateLetter,
									Expression.Switch(
											Expression.Call(paramState, Reflect<Id<DfaState<LetterId>>>.GetMethod(i => i.ToInt32())),
											Expression.Throw(new_InvalidOperationException, typeof(int)),
											dfa.States.OrderBy(s => s.Id).Select(state => Expression.SwitchCase(
													Expression.Switch(
															varInput,
															Expression.Constant(Dfa<LetterId>.Reject.ToInt32()),
															state
																	.Transitions
																	.GroupBy(p => p.Value.ToInt32(), p => p.Key)
																	.Select(g => Expression.SwitchCase(dfa.SymbolStates.TryGetValue(new Id<DfaState<LetterId>>(g.Key), out var symbolId)
																			? Expression.Block(
																					Expression.Assign(varResult, Expression.New(ctor_NullableSymbolId, Expression.New(ctor_SymbolId, Expression.Constant((int)symbolId)))),
																					Expression.Constant(g.Key))
																			: (Expression)Expression.Constant(g.Key),
																			g.SelectMany(r => r.Expand()).Select(i => Expression.Constant(i.ToInt32()))))
																	.ToArray()),
													Expression.Constant(state.Id.ToInt32())
											)).ToArray()))),
					varResult);
		}

		public static Expression<DfaStateMachine<LetterId, TLetter>> CreateExpression<TLetter>(Dfa<LetterId> dfa, Expression<Func<TLetter, LetterId>> inputToLetter)
				where TLetter: struct, IComparable<TLetter>, IEquatable<TLetter> {
			var paramState = Expression.Parameter(typeof(Id<DfaState<LetterId>>).MakeByRefType(), "state");
			var paramInput = inputToLetter.Parameters.Single();
			return Expression.Lambda<DfaStateMachine<LetterId, TLetter>>(CreateLetterStateMachine(dfa, paramState, inputToLetter.Body), "DfaAlphabetStateMachine", new[] {paramState, paramInput});
		}
	}
}
