using System;

namespace CryptoTrader.Utils {

	public static class Extensions {

		public static T[] GetRange<T> (this T[] array, int startIndex, int length) {
			T[] copyTo = new T[length];
			Array.Copy (array, startIndex, copyTo, 0, length);
			return copyTo;
		}

		public static T[] Copy<T> (this T[] array) {
			T[] copy = new T[array.Length];
			array.CopyTo (copy, 0);
			return copy;
		}

	}

}
