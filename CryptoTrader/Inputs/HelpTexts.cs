using System;
using System.Collections.Generic;

namespace CryptoTrader.Inputs {

	internal static class HelpTexts {

		private readonly static Dictionary<string, string> helpTexts = ReadHelpTexts ();

		public static string GetHelpText (string key) {
			return helpTexts[key];
		}

		private static Dictionary<string, string> ReadHelpTexts () {
			Dictionary<string, string> helpTexts = new Dictionary<string, string> ();
			string file = Resources.ReadResource ("helptexts.txt").Replace ("\t", "    ");
			List<int> startIndeces = new List<int> ();
			List<int> endIndeces = new List<int> ();

			for (int i = 0; i < file.Length; i++) {
				if (file[i] == '{')
					startIndeces.Add (i);
				if (file[i] == '}')
					endIndeces.Add (i);
			}

			if (startIndeces.Count != endIndeces.Count)
				throw new SystemException ("Amount of opening and closing brackets do not match.");

			string[] parts = new string[startIndeces.Count];
			for (int i = 0; i < parts.Length; i++)
				parts[i] = file.Substring (startIndeces[i] + 1, endIndeces[i] - startIndeces[i] - 1);

			for (int i = 0; i < parts.Length; i += 2)
				helpTexts.Add (parts[i], parts[i + 1]);

			return helpTexts;
		}

	}

}
