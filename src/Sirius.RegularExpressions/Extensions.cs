using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace Sirius.RegularExpressions {
	public static class Extensions {
		private static readonly ConcurrentDictionary<Type, MethodInfo> get_IReadOnlyDictionary_indexer = new ConcurrentDictionary<Type, MethodInfo>();

		public static Func<TKey, TValue> CreateGetterForIndexer<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> that) {
			var indexerGetter = get_IReadOnlyDictionary_indexer.GetOrAdd(typeof(IReadOnlyDictionary<TKey, TValue>), type => Reflect<IReadOnlyDictionary<TKey, TValue>>.GetProperty(d => d[default(TKey)]).GetMethod);
			return (Func<TKey, TValue>)Delegate.CreateDelegate(typeof(Func<TKey, TValue>), that, indexerGetter, true);
		}

		//		public static LetterId ReadFromReader(this Alphabet<string> that, TextReader reader) {
		//			var ch = reader.Read();
		//			if (ch < 0) {
		//				Debug.WriteLine("(EOF)");
		//				return Alphabet<string>.EOF;
		//			}
		//			var s = ((char)ch).ToString();
		//			if (char.IsHighSurrogate((char)ch)) {
		//				s = s+((char)reader.Read());
		//			}
		//			Debug.WriteLine(s);
		//			return that[s];
		//		}
	}
}
