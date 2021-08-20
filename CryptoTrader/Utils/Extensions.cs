using System;
using System.Collections.Generic;
using CryptoTrader.Utils;

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

		public static ICopyable[] Copy (this ICopyable[] array) {
			int length = array.Length;
			ICopyable[] output = new ICopyable[length];
			for (int i = 0; i < length; i++) {
				ICopyable item = array[i];
				output[i] = item.Copy ();
			}
			return output;
		}

		public static List<ICopyable> Copy (this List<ICopyable> list) {
			int count = list.Count;
			List<ICopyable> output = new List<ICopyable> (count);
			for (int i = 0; i < count; i++) {
				ICopyable item = list[i];
				output.Add (item.Copy ());
			}
			return output;
		}

	}

}
