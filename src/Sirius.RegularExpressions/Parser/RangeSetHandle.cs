using System;
using System.Collections.Generic;
using System.Linq;

using Sirius.Collections;
using Sirius.Unicode;

namespace Sirius.RegularExpressions.Parser {
	public abstract class RangeSetHandle: IEquatable<RangeSetHandle> {
		public class Class: RangeSetHandle {
			private static bool EqualsInternal(Class x, Class y) {
				return Equals(x.CharSetClass, y.CharSetClass);
			}

			public Class(CharSetClass cls, bool negate): base(negate) {
				this.CharSetClass = cls;
			}

			public CharSetClass CharSetClass {
				get;
			}

			public override bool Equals(RangeSetHandle other) {
				return this.InstanceEquals<Class>(other, EqualsInternal);
			}

			public override RangeSet<T> GetCharSet<T>(IRangeSetProvider<T> provider) {
				return provider.GetRangeSet(RangeSetCategory.Class, this.CharSetClass.ToString());
			}

			public override int GetHashCode() {
				unchecked {
					return (base.GetHashCode() * 375)^this.CharSetClass.GetHashCode();
				}
			}
		}

		public class Named: RangeSetHandle {
			private static bool EqualsInternal(Named x, Named y) {
				return StringComparer.Ordinal.Equals(x.Name, y.Name);
			}

			public Named(string name, bool negate): base(negate) {
				this.Name = name;
			}

			public string Name {
				get;
			}

			public override bool Equals(RangeSetHandle other) {
				return this.InstanceEquals<Named>(other, EqualsInternal);
			}

			public override RangeSet<T> GetCharSet<T>(IRangeSetProvider<T> provider) {
				return provider.GetRangeSet(RangeSetCategory.Name, this.Name);
			}

			public override int GetHashCode() {
				unchecked {
					return (base.GetHashCode() * 379)^StringComparer.Ordinal.GetHashCode(this.Name);
				}
			}
		}

		public class Static: RangeSetHandle {
			public Static(Codepoint ch): this(new RangeSet<Codepoint>(ch), false) { }

			public Static(Codepoint from, Codepoint to): this(new RangeSet<Codepoint>(Range<Codepoint>.Create(from, to)), false) { }

			public Static(RangeSet<Codepoint> charset, bool negate): base(negate) {
				this.Charset = charset;
			}

			public RangeSet<Codepoint> Charset {
				get;
			}

			public override bool Equals(RangeSetHandle other) {
				return this.InstanceEquals<Static>(other, (x, y) => x.Charset.Equals(y.Charset));
			}

			public override RangeSet<T> GetCharSet<T>(IRangeSetProvider<T> provider) {
				return provider.GetRangeSet(this.Charset);
			}

			public override int GetHashCode() {
				unchecked {
					return (base.GetHashCode() * 379)^this.Charset.GetHashCode();
				}
			}

			public bool TryGetSingle(out Codepoint ch) {
				var set = this.Negate ? ~this.Charset : this.Charset;
				if (set.Count == 1) {
					var range = set[0];
					if (range.From.CompareTo(range.To) == 0) {
						ch = range.From;
						return true;
					}
				}
				ch = Codepoints.EOF;
				return false;
			}
		}

		public class Union: RangeSetHandle {
			private readonly List<RangeSetHandle> handles = new List<RangeSetHandle>();

			public Union(bool negate): base(negate) { }

			public void Add(RangeSetHandle handle) {
				this.handles.Add(handle);
			}

			public override bool Equals(RangeSetHandle other) {
				return this.InstanceEquals<Union>(other, (x, y) => x.handles.SetEqual(y.handles));
			}

			public override RangeSet<T> GetCharSet<T>(IRangeSetProvider<T> provider) {
				var result = RangeSet<T>.Empty;
				foreach (var handle in this.handles) {
					var set = handle.GetCharSet(provider);
					result |= handle.Negate ? provider.Negate(set) : set;
				}
				return result;
			}

			public override int GetHashCode() {
				return this.handles.Aggregate(base.GetHashCode() * 397, (hashCode, handle) => hashCode^handle.GetHashCode());
			}

			public RangeSetHandle Simplify() {
				switch (this.handles.Count) {
				case 0:
					return new Static(RangeSet<Codepoint>.Empty, this.Negate);
				case 1:
					var union = this.handles[0] as Union;
					if (union != null) {
						var result = new Union(union.Negate^this.Negate);
						foreach (var handle in union.handles) {
							result.Add(handle);
						}
						return result.Simplify();
					}
					if (!this.Negate) {
						return this.handles[0];
					}
					var stat = this.handles[0] as Static;
					if (stat != null) {
						return new Static(stat.Charset, !stat.Negate);
					}
					var named = this.handles[0] as Named;
					if (named != null) {
						return new Named(named.Name, !named.Negate);
					}
					var cls = this.handles[0] as Class;
					if (cls != null) {
						return new Class(cls.CharSetClass, !cls.Negate);
					}
					break;
				}
				return this;
			}
		}

		protected RangeSetHandle(bool negate) {
			this.Negate = negate;
		}

		public bool Negate {
			get;
		}

		public abstract bool Equals(RangeSetHandle other);

		public override bool Equals(object obj) {
			return this.Equals(obj as RangeSetHandle);
		}

		public abstract RangeSet<T> GetCharSet<T>(IRangeSetProvider<T> provider)
				where T: IComparable<T>;

		public override int GetHashCode() {
			unchecked {
				return this.GetType().GetHashCode()^(this.Negate.GetHashCode() * 397);
			}
		}

		private bool InstanceEquals<THandle>(RangeSetHandle other, Func<THandle, THandle, bool> equals)
				where THandle: RangeSetHandle {
			return LinqExtensions.InstanceEquals((THandle)this, other, equals) && (this.Negate == other.Negate);
		}
	}
}
