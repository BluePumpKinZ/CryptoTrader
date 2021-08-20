using System;
using System.Collections.Generic;

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

		public static T[] CopyMembers<T> (this T[] array) where T : ICopyable {
			int length = array.Length;
			T[] output = new T[length];
			for (int i = 0; i < length; i++)
				output[i] = (T)array[i].Copy ();
			return output;
		}

		public static List<T> CopyMembers<T> (this List<T> list) where T : ICopyable {
			int count = list.Count;
			List<T> output = new List<T> (count);
			for (int i = 0; i < count; i++) {
				output.Add ((T)list[i].Copy ());
			}
			return output;
		}

		public static List<T> UpCast<TBase, T> (this List<TBase> list) where T : TBase {
			int count = list.Count;
			List<T> output = new List<T> (count);
			for (int i = 0; i < count; i++)
				output.Add ((T)list[i]);
			return output;
		}

		public static List<TBase> DownCast<T, TBase> (this List<T> list) where T : TBase {
			int count = list.Count;
			List<TBase> output = new List<TBase> (count);
			for (int i = 0; i < count; i++)
				output.Add (list[i]);
			return output;
		}

		public static T[] UpCast<TBase, T> (this TBase[] array) where T : TBase {
			int length = array.Length;
			T[] output = new T[length];
			for (int i = 0; i < length; i++)
				output[i] = (T)array[i];
			return output;
		}

		public static TBase[] DownCast<T, TBase> (this T[] array) where T : TBase {
			int length = array.Length;
			TBase[] output = new TBase[length];
			for (int i = 0; i < length; i++)
				output[i] = array[i];
			return output;
		}

	}

}
