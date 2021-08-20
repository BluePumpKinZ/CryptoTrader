using CryptoTrader.NicehashAPI.JSONObjects;
using CryptoTrader.Utils;

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

		private protected new ICopyable CopyAbstractValues (ICopyable copy) {
			LimitOrder limitOrder = (LimitOrder)base.CopyAbstractValues (copy);
			limitOrder.Price = Price;
			return limitOrder;
		}

	}
}
