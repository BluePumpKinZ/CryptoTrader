using CryptoTrader.NicehashAPI;
using CryptoTrader.NicehashAPI.JSONObjects;
using CryptoTrader.Utils;
using System;

namespace CryptoTrader.Algorithms.Orders {
	public class LimitSellOrder : LimitOrder {

		private protected override bool IsBuyOrderInternal () {
			return false;
		}

		private protected override bool IsSellOrderInternal () {
			return true;
		}

		private protected override bool IsMarketOrderInternal () {
			return false;
		}

		private protected override bool IsLimitOrderInternal () {
			return true;
		}

		public override bool HasSufficientFunds (Balances balances) {
			throw new NotImplementedException ();
		}

		public override bool HasPriceBeenReached (Balances balances) {
			return balances.GetBalanceForCurrency (Currency).BTCRate >= Price;
		}

		public override ICopyable Copy () {
			return new LimitSellOrder (Currency, Value, Price, Time);
		}

		public LimitSellOrder (Currency currency, double value, double price, long time) {
			Currency = currency;
			Value = value;
			Price = price;
			Time = time;
		}
	}
}
