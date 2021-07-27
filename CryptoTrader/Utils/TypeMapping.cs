using CryptoTrader.Algorithms;
using System;

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

	}

}
