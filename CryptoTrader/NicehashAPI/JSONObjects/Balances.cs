using CryptoTrader.Algorithms.Orders;
using CryptoTrader.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

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
				throw new NoPricesFoundException ($"No balance could be found for currency {Currencies.GetCurrencyToken(currency)}");
			}
			return balances[index];
		}

		public bool CanBuy (Order order) {
			if (order.IsSellOrder)
				throw new ArgumentException ("The order given was not a 'BUY' order");
			return order.HasSufficientFunds (this);
		}

		public bool CanSell (Order order) {
			if (order.IsBuyOrder)
				throw new ArgumentException ("The order given was not a 'SELL' order");
			return order.HasSufficientFunds (this);
		}

		public void AddBalance (Balance balance) {
			try {
				Balance baseBalance = GetBalanceForCurrency (balance.Currency);
				baseBalance.Add (balance);
				SetBalance (baseBalance);
			} catch (NoPricesFoundException) {
				balances.Add (balance);
			}
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

		public void AddEmptyBalance (Currency c, double price) {
			AddBalance (new Balance (c, 0, 0, price));
		}

		public void UpdateBTCRateForCurrency (Currency currency, double price) {
			try {
				GetBalanceForCurrency (currency).UpdateBTCRate (price);
			} catch (NoPricesFoundException) {
				throw new NoPricesFoundException ($"Couldn't update the btc rate for currency {currency}.");
			}
		}

		public void UpdateBTCRates () {
			balances.ForEach ((balance) => balance.UpdateBTCRate ());
		}

		/// <summary>
		/// copies the available and pending values from balances.
		/// </summary>
		/// <param name="balances">The balances from which to copy.</param>
		public void UpdateValuesFromBalances (Balances balances) {
			for (int i = 0; i < this.balances.Count; i++) {
				try {
					Balance balance = null;
					balance = balances.GetBalanceForCurrency (this.balances[i].Currency);
					if (balance != null)
						SetBalance (balance);
				} catch (NoPricesFoundException) {

				}
			}
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
