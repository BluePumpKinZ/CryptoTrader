using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoTrader.NicehashAPI.JSONObjects {
	
	public class Balances : IParsable {

		public Balance balance;
		private readonly List<Balance> balances;

		public Balances (string s) {
			balances = new List<Balance> ();
			Parse (s);
			balance = GetTotalBalance ();
		}

		internal override void ParsePart (string key, string value) {
			switch (key) {
			case "currencies":
				Parse (value);
				break;
			default:
				try {
					Balance b = new Balance (value);
					balances.Add (b);
				} catch (Exception) {

				}
				break;
			}
		}

		private Balance GetTotalBalance () {
			double totalValue = 0;
			for (int i = 0; i < balances.Count; i++) {
				totalValue += balances[i].GetBTCValue ();
			}
			return new Balance (Currency.Bitcoin, totalValue, 1);
		}

		public override string ToString () {
			StringBuilder sb = new StringBuilder ();
			sb.Append ($"TotalBalance\n\t{balance}\nBalances");
			for (int i = 0; i < balances.Count; i++) {
				sb.Append ("\n\t" + balances[i].ToString ());
			}

			return sb.ToString ();
		}
	}
}
