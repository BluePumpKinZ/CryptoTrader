using CryptoTrader.NicehashAPI;
using CryptoTrader.NicehashAPI.JSONObjects;
using System;
using System.Collections.Generic;
using System.Text;

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

		public LimitSellOrder (Currency currency, double value, double price, long time) {
			Currency = currency;
			Value = value;
			Price = price;
			Time = time;
		}
	}
}
