using CryptoTrader.Algorithms;
using CryptoTrader.NicehashAPI;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CryptoTrader.Utils {

	public static class TypeMapping {

		public static Algorithm AlgorithmFromName (string algoName) {
			Type type = Type.GetType ($"CryptoTrader.Algorithms.{algoName}");
			return Activator.CreateInstance (type, Currency.Null) as Algorithm;
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

		public static Type[] GetAllDerivedTypes (Type type) {
			List<Type> output = new List<Type> ();
			Assembly assembly = Assembly.GetExecutingAssembly ();
			Type[] types = assembly.GetTypes ();
			for (int i = 0; i < types.Length; i++)
				if (types[i].IsSubclassOf (type))
					output.Add (types[i]);

			return output.ToArray ();
		}

		public static string[] GetAllDerivedTypeNames (Type type) {
			Type[] types = GetAllDerivedTypes (type);
			string[] output = new string[types.Length];
			for (int i = 0; i < types.Length; i++)
				output[i] = types[i].Name;
			return output;
		}

	}

}
