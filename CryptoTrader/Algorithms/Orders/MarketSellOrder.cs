using CryptoTrader.Exceptions;
using CryptoTrader.NicehashAPI;
using CryptoTrader.NicehashAPI.JSONObjects;

namespace CryptoTrader.Algorithms.Orders {

	public class MarketSellOrder : MarketOrder {

		protected override bool IsBuyOrderInternal () {
			return false;
		}

		protected override bool IsSellOrderInternal () {
			return true;
		}

		protected override bool IsMarketOrderInternal () {
			return true;
		}

		protected override bool IsLimitOrderInternal () {
			return false;
		}

		public override bool HasSufficientFunds (Balances balances) {
			try {
				Balance balance = balances.GetBalanceForCurrency (Currency);
				return balance.Available >= Value;
			} catch (NoPricesFoundException) {
				return false;
			}
		}

		public MarketSellOrder (Currency currency, double value, long time) {
			Currency = currency;
			Value = value;
			Time = time;
		}
	}
}