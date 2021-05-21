using CryptoTrader.NicehashAPI.JSONObjects;
using System;
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
	}
}
