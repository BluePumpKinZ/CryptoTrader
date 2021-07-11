using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CryptoTrader.NicehashAPI.JSONObjects {
	public abstract class IParsable {

		public void Parse (string s) {
			//string[] parts = Regex.Split (Regex.Replace(s, "[\t\r\n\\\" ]",""), ",(?![^{]*\\},)"); // ,(?![^{]*\\}),* could be better

			string[] parts = GetJSONSplits (s);

			for (int i = 0; i < parts.Length; i++) {
				string[] keyValueSplit = parts[i].Split (':', 2);
				string key = keyValueSplit[0];
				string value = keyValueSplit[1];

				/*key = Regex.Replace (key, "^[{]?", "");
				key = Regex.Replace (key, "[}]?$", "");
				value = Regex.Replace (value, "^[{]?", "");
				value = Regex.Replace (value, "[}]?$", "");*/

				if (!value.Contains (','))
					value = value.Replace (".", ",");
				ParsePart (key, value);
			}
		}

		internal abstract void ParsePart (string key, string value);

		internal static string[] GetJSONSplits (string s) {
			// Remove whitespace
			s = Regex.Replace (s, "[\t\r\n\\\" ]", "");

			// Remove brackets front and back
			s = Regex.Replace (s, "^[{\\[]?", "");
			s = Regex.Replace (s, "[}\\]]?$", "");

			// Scan for comma's outside brackets
			List<int> indecesToSplit = new List<int> ();
			byte scanLevel = 0;
			for (int i = 0; i < s.Length; i++) {
				char currentChar = s[i];
				if (scanLevel == 0 && currentChar == ',')
					indecesToSplit.Add (i);

				if (Regex.IsMatch (currentChar.ToString (), "[\\[{]"))
					scanLevel++;
				if (Regex.IsMatch (currentChar.ToString (), "[\\]}]"))
					scanLevel--;
			}

			if (indecesToSplit.Count == 0)
				return new string[] { s };

			// Split original string by comma's
			string[] splits = new string[indecesToSplit.Count + 1];

			indecesToSplit.Insert (0, -1);
			indecesToSplit.Add (s.Length);

			// Console.WriteLine (s);
			for (int i = 0; i < indecesToSplit.Count - 1; i++) {
				int startIndex = indecesToSplit[i] + 1;
				int endIndex = indecesToSplit[i + 1] - 1;
				int length = endIndex - startIndex + 1;
				// Console.WriteLine ("Start: " + startIndex);
				// Console.WriteLine ("End: " + endIndex);
				// Console.WriteLine ("Length: " + length);
				splits[i] = s.Substring (startIndex, length);
			}

			return splits;
		}

	}
}
