using System.Collections.Generic;

namespace CryptoTrader.NicehashAPI.JSONObjects {
	public class FeeLimits : IParsable {

		public Dictionary<double, double> Limits { private set; get; } = new Dictionary<double, double> ();

		public FeeLimits (string s) {
			Parse (s);
		}

		internal override void ParsePart (string key, string value) {
			Limits.Add (double.Parse(key), double.Parse(value));
		}
	}
}
