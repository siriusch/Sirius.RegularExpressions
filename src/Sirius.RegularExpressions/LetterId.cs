using System;
using System.Diagnostics.Contracts;

namespace Sirius.RegularExpressions {
	public struct LetterId: IComparable, IComparable<LetterId>, IEquatable<LetterId>, IIncrementable<LetterId> {
		public static readonly LetterId Eof = new LetterId(0);
		public static readonly LetterId MinValue = new LetterId(0);
		public static readonly LetterId MaxValue = new LetterId(int.MaxValue);

		public static implicit operator int(LetterId id) {
			return id.id;
		}

		[Pure]
		public static bool operator ==(LetterId left, LetterId right) {
			return left.id == right.id;
		}

		[Pure]
		public static bool operator !=(LetterId left, LetterId right) {
			return left.id != right.id;
		}

		[Pure]
		public static bool operator <(LetterId left, LetterId right) {
			return left.id < right.id;
		}

		[Pure]
		public static bool operator >(LetterId left, LetterId right) {
			return left.id > right.id;
		}

		[Pure]
		public static bool operator <=(LetterId left, LetterId right) {
			return left.id <= right.id;
		}

		[Pure]
		public static bool operator >=(LetterId left, LetterId right) {
			return left.id >= right.id;
		}

		private readonly int id;

		public LetterId(int id) {
			if (id < 0) {
				throw new ArgumentOutOfRangeException(nameof(id));
			}
			this.id = id;
		}

		[Pure]
		public int ToInt32() {
			return this.id;
		}

		[Pure]
		public LetterId Increment() {
			return new LetterId(this.id+1);
		}

		[Pure]
		public LetterId Decrement() {
			return new LetterId(this.id-1);
		}

		[Pure]
		public int CompareTo(object other) {
			return this.CompareTo((LetterId)other);
		}

		[Pure]
		public int CompareTo(LetterId other) {
			return this.id.CompareTo(other.id);
		}

		[Pure]
		public bool Equals(LetterId other) {
			return this.id == other.id;
		}

		[Pure]
		public override bool Equals(object other) {
			return (other is LetterId) && this.Equals((LetterId)other);
		}

		[Pure]
		public override int GetHashCode() {
			unchecked {
				return (this.id * 389)^typeof(LetterId).GetHashCode();
			}
		}

		[Pure]
		public override string ToString() {
			return string.Format("Letter:{0}", this.id);
		}
	}
}
