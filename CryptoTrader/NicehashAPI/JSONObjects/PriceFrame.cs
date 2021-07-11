using System.Collections.Generic;
using System.Text;

namespace CryptoTrader.NicehashAPI.JSONObjects {

	public class PriceFrame : IParsable {

		public SortedDictionary<string, double> Prices { private set; get; } = new SortedDictionary<string, double> ();

		public PriceFrame (string s) {
			Parse (s);
		}

		internal override void ParsePart (string key, string value) {
			Prices.Add (key, double.Parse (value));
		}

		public override string ToString () {
			StringBuilder sb = new StringBuilder ("Prices:");
			foreach (KeyValuePair<string, double> price in Prices) {
				sb.Append ($"\n{price.Key} - {price.Value}");
			}
			return sb.ToString ();
		}
	}
}
