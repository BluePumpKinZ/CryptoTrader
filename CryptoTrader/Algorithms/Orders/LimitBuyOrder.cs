using CryptoTrader.NicehashAPI;
using CryptoTrader.NicehashAPI.JSONObjects;

namespace CryptoTrader.Algorithms.Orders {
	public class LimitBuyOrder : LimitOrder {

		private protected override bool IsBuyOrderInternal () {
			return true;
		}

		private protected override bool IsSellOrderInternal () {
			return false;
		}

		private protected override bool IsMarketOrderInternal () {
			return false;
		}

		private protected override bool IsLimitOrderInternal () {
			return true;
		}

		public override bool HasSufficientFunds (Balances balances) {
			if (!HasPriceBeenReached (balances))
				return false;
			return true;
		}

		public override bool HasPriceBeenReached (Balances balances) {
			return balances.GetBalanceForCurrency (Currency).BTCRate <= Price;
		}

		public LimitBuyOrder (Currency currency, double value, double price, long time) {
			Currency = currency;
			Value = value;
			Price = price;
			Time = time;
		}
	}
}
