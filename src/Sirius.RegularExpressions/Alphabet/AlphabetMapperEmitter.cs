using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Sirius.Collections;

namespace Sirius.RegularExpressions.Alphabet {
	public class AlphabetMapperEmitter<TLetter>
			where TLetter: struct, IComparable<TLetter>, IEquatable<TLetter> {
		private static readonly bool IsOperatorComparable = typeof(TLetter).IsPrimitive && (typeof(TLetter) != typeof(bool));
		private static readonly MethodInfo IComparable_CompareTo = Reflect<IComparable<TLetter>>.GetMethod(i => i.CompareTo(default(TLetter)));
		private static readonly ConstructorInfo LetterId_Ctor = Reflect.GetConstructor(() => new LetterId(default(int)));

		private static Expression Compare(ParameterExpression value, Func<Expression, Expression, Expression> op, TLetter rangeLimit) {
			return IsOperatorComparable
					? op(
							value,
							Expression.Constant(rangeLimit))
					: op(
							Expression.Call(value, IComparable_CompareTo, Expression.Constant(rangeLimit)),
							Expression.Constant(0));
		}

		public static Expression<Func<TLetter, LetterId>> CreateExpression(AlphabetBuilder<TLetter> builder, LetterId? defaultLetter = null) {
			var ranges = builder.AlphabetById.SelectMany(p => p.Value.Select(r => new KeyValuePair<Range<TLetter>, LetterId>(r, p.Key))).Where(p => p.Value != defaultLetter).OrderBy(p => p.Key.From).ToList();
			var paramValue = Expression.Parameter(typeof(TLetter), "value");
			var defaultExpression = defaultLetter.HasValue
					? (Expression)Expression.Constant(defaultLetter.Value.ToInt32())
					: Expression.Throw(
							Expression.New(
									Reflect.GetConstructor(() => new InvalidOperationException())), typeof(int));
			var body = Expression.New(LetterId_Ctor, BinaryCompare(ranges, 0, ranges.Count, paramValue, defaultExpression));
			return Expression.Lambda<Func<TLetter, LetterId>>(body, paramValue);
		}

		private static bool IsAdjacent(IReadOnlyList<KeyValuePair<Range<TLetter>, LetterId>> ranges, int left, int right) {
			Debug.Assert((Math.Max(left, right) >= 0) && (Math.Min(left, right) < ranges.Count) && ((left + 1) == right));
			if (left < 0) {
				return ranges[right].Key.From.Equals(Incrementor<TLetter>.MinValue);
			}
			if (right >= ranges.Count) {
				return ranges[left].Key.To.Equals(Incrementor<TLetter>.MaxValue);
			}
			return Incrementor<TLetter>.Increment(ranges[left].Key.To).Equals(ranges[right].Key.From);
		}

		private static Expression BinaryCompare(IReadOnlyList<KeyValuePair<Range<TLetter>, LetterId>> ranges, int left, int right, ParameterExpression paramValue, Expression defaultExpression) {
			if (left >= right) {
				return defaultExpression;
			}
			var mid = (left + right) / 2;
			var midRange = ranges[mid];
			Expression result = Expression.Constant(midRange.Value.ToInt32());
			if ((mid != right-1) || !IsAdjacent(ranges, mid, mid+1)) {
				result = Expression.Condition(
						Compare(paramValue, Expression.GreaterThan, midRange.Key.To),
						BinaryCompare(ranges, mid + 1, right, paramValue, defaultExpression),
						result);
			}
			if ((mid != left) || !IsAdjacent(ranges, mid-1, mid)) {
				result = Expression.Condition(
						Compare(paramValue, Expression.LessThan, midRange.Key.From),
						BinaryCompare(ranges, left, mid, paramValue, defaultExpression),
						result);
			}
			return result;
		}
	}
}
