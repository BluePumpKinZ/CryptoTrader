using System;
using System.Collections.Generic;

namespace CryptoTrader.NicehashAPI.JSONObjects {

	public class ExchangeStatus : IParsable {

		private List<MarketStatus> markets = new List<MarketStatus> ();

		public ExchangeStatus () {

		}

		public ExchangeStatus (string s) {
			Parse (s);
		}

		internal override void ParsePart (string key, string value) {
			switch (key) {
			case "symbols":
				markets = new List<MarketStatus> ();
				foreach (string split in GetJSONSplits (value)) {
					MarketStatus marketStatus = new MarketStatus (split);
					markets.Add (marketStatus);
				}
				break;
			}
		}

		public MarketStatus GetStatusForCurrency (Currency currency) {
			if (currency == Currency.Null)
				throw new ArgumentException ("Currency cannot be Null");

			for (int i = 0; i < markets.Count; i++) {
				if (markets[i].Currency == currency)
					return markets[i];
			}
			return new MarketStatus ();
		}

	}

}
