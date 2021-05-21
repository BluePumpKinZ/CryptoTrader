using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoTrader.NicehashAPI.JSONObjects {
	
	public class Balances : IParsable {

		public Balance balance;
		private List<Balance> balances;

		public Balances (string s) {
			balances = new List<Balance> ();
			Parse (s);
		}

		internal override void ParsePart (string key, string value) {
			switch (key) {
			case "currencies":
				Parse (value);
				break;
			default:
				try {

				}
				break;
			}
		}

		private Balance GetTotalBalance () {
			double totalValue = 0;
			for (int i = 0; i < balances.Count; i++) {
				totalValue += balances[i].GetBTCValue ();
			}
		}
	}
}
