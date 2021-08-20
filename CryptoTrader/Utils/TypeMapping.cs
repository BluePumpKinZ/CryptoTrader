using CryptoTrader.Algorithms;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CryptoTrader.Utils {

	public static class TypeMapping {

		public static Algorithm AlgorithmFromName (string algoName) {
			Type type = Type.GetType ($"CryptoTrader.Algorithms.{algoName}");
			return Activator.CreateInstance (type) as Algorithm;
		}

		public static string NameFromAlgorithm (Algorithm algorithm) {
			return algorithm.GetType ().Name;
		}

		public static byte[] NameToBytes (string name) {
			return Encoding.UTF8.GetBytes (name);
		}

		public static string BytesToString (byte[] bytes) {
			return Encoding.UTF8.GetString (bytes);
		}

		public static string[] GetAllDerivedTypes (Type type) {
			List<string> output = new List<string> ();
			Assembly assembly = Assembly.GetExecutingAssembly ();
			Type[] types = assembly.GetTypes ();
			for (int i = 0; i < types.Length; i++)
				if (types[i].IsSubclassOf (type))
					output.Add (types[i].Name);
			return output.ToArray ();
		}

	}

}
