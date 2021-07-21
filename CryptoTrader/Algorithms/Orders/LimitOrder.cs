using CryptoTrader.NicehashAPI.JSONObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoTrader.Algorithms.Orders {

	public abstract class LimitOrder : Order {

		public double Price { protected set; get; }

		public abstract bool HasPriceBeenReached (Balances balances);

		public MarketOrder ToMarketOrder () {
			if (IsBuyOrder)
				return new MarketBuyOrder (Currency, Value, Time);
			else
				return new MarketSellOrder (Currency, Value, Time);
		}

		new public string GetOrderUrl () {
			return base.GetOrderUrl () + $"&price={Price}";
		}
	}
}
