using CryptoTrader.Exceptions;
using CryptoTrader.NicehashAPI;
using CryptoTrader.NicehashAPI.JSONObjects;

namespace CryptoTrader.Algorithms.Orders {

	public class MarketBuyOrder : MarketOrder {

		private protected override bool IsBuyOrderInternal () {
			return true;
		}

		private protected override bool IsSellOrderInternal () {
			return false;
		}

		private protected override bool IsMarketOrderInternal () {
			return true;
		}

		private protected override bool IsLimitOrderInternal () {
			return false;
		}

		public override bool HasSufficientFunds (Balances balances) {
			Balance btcBalance = balances.GetBalanceForCurrency (Currency.Bitcoin);
			try {
				Balance balance = balances.GetBalanceForCurrency (Currency);
				return balance.BTCRate * Value <= btcBalance.Available;
			} catch (NoPricesFoundException) {
				// Just returning false because if the balance can't be found there will be no available currency anyway
				return false;
			}
		}

		public MarketBuyOrder (Currency currency, double value, long time) {
			Currency = currency;
			Value = value;
			Time = time;
		}
	}

}
