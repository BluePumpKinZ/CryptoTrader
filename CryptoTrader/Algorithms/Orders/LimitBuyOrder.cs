using CryptoTrader.NicehashAPI;
using CryptoTrader.NicehashAPI.JSONObjects;

namespace CryptoTrader.Algorithms.Orders {
	public class LimitBuyOrder : LimitOrder {

		protected override bool IsBuyOrderInternal () {
			return true;
		}

		protected override bool IsSellOrderInternal () {
			return false;
		}

		protected override bool IsMarketOrderInternal () {
			return false;
		}

		protected override bool IsLimitOrderInternal () {
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
