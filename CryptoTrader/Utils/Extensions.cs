using System;

namespace CryptoTrader.Utils {

	public static class Extensions {

		public static T[] GetRange<T> (this Array array, int startIndex, int length) {
			T[] copyTo = new T[length];
			Array.Copy (array, startIndex, copyTo, 0, length);
			return copyTo;
		}

	}

}
