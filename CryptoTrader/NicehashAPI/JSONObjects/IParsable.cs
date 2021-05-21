using System.Text.RegularExpressions;

namespace CryptoTrader.NicehashAPI.JSONObjects {
	public abstract class IParsable {

		public void Parse (string s) {
			string[] parts = Regex.Split (Regex.Replace(s, "[\t\r\n\\\" ]",""), ",(?![^{]*\\},)");
			for (int i = 0; i < parts.Length; i++) {
				string[] keyValueSplit = parts[i].Split (':', 2);
				string key = keyValueSplit[0];
				string value = keyValueSplit[1];
				
				key = Regex.Replace (key ,"^[{]+","");
				value = Regex.Replace (value, "[}]+$", "");

				if (!value.Contains (','))
					value = value.Replace (".", ",");
				ParsePart (key, value);
			}
		}

		internal abstract void ParsePart (string key, string value);

	}
}
