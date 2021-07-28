using CryptoTrader.Algorithms;
using System;
using System.Text;

namespace CryptoTrader.Utils {

	public static class TypeMapping {

		public static Algorithm AlgorithmFromName (string algoName) {
			Type type = Type.GetType ($"CryptoTrader.Algorithms.{algoName}");
			return Activator.CreateInstance (type) as Algorithm;
		}

		public static string NameFromAlgorithm (Algorithm algorithm) {
			Console.WriteLine (algorithm.GetType ());
			return algorithm.GetType ().Name;
		}

		public static byte[] NameToBytes (string name) {
			return Encoding.UTF8.GetBytes (name);
		}

		public static string BytesToString (byte[] bytes) {
			return Encoding.UTF8.GetString (bytes);
		}

	}

}
