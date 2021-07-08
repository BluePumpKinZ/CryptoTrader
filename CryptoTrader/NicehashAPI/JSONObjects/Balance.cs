using CryptoTrader.Exceptions;
using System;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;
using System.Text;

namespace CryptoTrader.NicehashAPI.JSONObjects {

	public class Balance : IParsable {

		public Currency Currency { private set; get; }
		public double Available { private set; get; }
		public double Pending { private set; get; }
		public double Total { get { return Available + Pending; } }
		public double BTCRate { private set; get; }

		public Balance (Currency c, double available, double pending, double btcRate) {
			Currency = c;
			Available = available;
			Pending = pending;
			BTCRate = btcRate;
		}

		public Balance (Currency c, double value, double btcRate) {
			Currency = c;
			Available = value;
			Pending = 0;
			BTCRate = btcRate;
		}

		public Balance (string s) {
			Parse (s);
		}

		internal override void ParsePart (string key, string value) {
			switch (key) {
			case "currency":
				bool foundPair = Currencies.TryGetCurrencyFromBTCPair (value, out Currency c);
				if (!foundPair)
					c = Currencies.GetCurrencyFromToken (value);
				Currency = c;
				break;
			case "available":
				Available = double.Parse (value);
				break;
			case "pending":
				Pending = double.Parse (value);
				break;
			case "btcRate":
				BTCRate = double.Parse (value);
				break;
			}
		}

		public Balance ToBTCBalance () {
			return new Balance (Currency.Bitcoin, Available * BTCRate, Pending * BTCRate, 1);
		}

		public Balance ToCurrency (Currency currency, double price) {
			Balance btc = ToBTCBalance ();
			return new Balance (currency, btc.Available / price, btc.Pending / price, price);
		}

		public Balance ToCurrencyActual (Currency currency) {
			if (!PriceWatcher.HasPrices)
				throw new NoPricesFoundException ("The pricewatcher hasn't been started.");
			return ToCurrency (currency, PriceWatcher.GetBTCPrice (currency));
		}

		public void Add (Balance balance) {
			if (Currency != balance.Currency) {
				balance = balance.ToCurrency (Currency, BTCRate);
			}
			Available += balance.Available;
			Pending += balance.Pending;
		}

		public void Subtract (Balance balance) {
			balance.Available = -balance.Available;
			balance.Pending = -balance.Pending;
			Add (balance);
		}

		public void ApplyFee (double fee) {
			if (fee < 0 || fee > 1)
				throw new ArgumentException ($"{fee} is not a valid fee value, it should be between 0 and 1 inclusive.");
			double multiplier = 1 - fee;
			Available *= 1 - multiplier;
			Pending *= 1 - multiplier;
		}

		public void UpdateBTCRate (double btcRate) {
			BTCRate = btcRate;
		}

		public void UpdateBTCRate () {
			BTCRate = PriceWatcher.GetBTCPrice (Currency);
		}

		public bool IsComplete () {
			return BTCRate != 0;
		}

		public override string ToString () {
			return $"Balance {Currencies.GetCurrencyToken (Currency)}\n\tTotal {Total}\n\tAvailable {Available}\n\tPending {Pending}";
		}
	}
}
