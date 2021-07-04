using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CryptoTrader.NicehashAPI.JSONObjects {

	public class Balances : IParsable {

		public Balance TotalBalance { get { return GetTotalBalance (); } }
		private readonly List<Balance> balances;

		public Balances () {
			balances = new List<Balance> ();
		}

		public Balances (List<Balance> balances) {
			this.balances = balances;
		}

		public Balances (string s) {
			balances = new List<Balance> ();
			Parse (s);
		}

		internal override void ParsePart (string key, string value) {
			switch (key) {
			case "currencies":
				foreach (string split in GetJSONSplits (value)) {
					Balance b = new Balance (split);
					if (b.IsComplete ())
						balances.Add (b);
				}
				break;
			}
		}

		public Balance GetBalanceForCurrency (Currency currency) {
			int index = GetBalanceIndex (currency);
			if (index == -1) {
				Balance balance = new Balance (currency, 0, 0, PriceWatcher.GetBTCPrice (currency));
				balances.Add (balance);
				return balance;
			}
			return balances[index];
		}

		public void AddBalance (Balance balance) {
			Balance baseBalance = GetBalanceForCurrency (balance.Currency);
			baseBalance.Add (balance);
			SetBalance (baseBalance);
		}

		public void SubtractBalance (Balance balance) {
			Balance baseBalance = GetBalanceForCurrency (balance.Currency);
			baseBalance.Subtract (balance);
			SetBalance (baseBalance);
		}

		private void SetBalance (Balance balance) {
			int index = GetBalanceIndex (balance.Currency);
			if (index == -1) {
				balances.Add (balance);
				return;
			}

			balances[index] = balance;
		}

		private int GetBalanceIndex (Currency c) {
			for (int i = 0; i < balances.Count; i++) {
				if (balances[i].Currency == c)
					return i;
			}
			return -1;
		}

		public void UpdateBTCRates () {
			balances.ForEach ((balance) => balance.UpdateBTCRate ());
		}

		private Balance GetTotalBalance () {
			double totalAvailable = 0;
			double totalPending = 0;
			for (int i = 0; i < balances.Count; i++) {
				Balance btcBalance = balances[i].ToBTCBalance ();
				totalAvailable += btcBalance.Available;
				totalPending += btcBalance.Pending;
			}
			return new Balance (Currency.Bitcoin, totalAvailable, totalPending, 1);
		}

		public override string ToString () {
			StringBuilder sb = new StringBuilder ();
			sb.Append ($"TotalBalance\n\t{TotalBalance}\nBalances");
			for (int i = 0; i < balances.Count; i++) {
				sb.Append ("\n\t" + balances[i].ToString ());
			}

			return sb.ToString ();
		}
	}
}
