using CryptoTrader.NicehashAPI;
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
			return base.GetOrderUrl () + $"&price={NumberFormatting.FormatAmount (Price, Currency)}";
		}

		private protected new ICopyable CopyAbstractValues (ICopyable copy) {
			LimitOrder limitOrder = (LimitOrder)base.CopyAbstractValues (copy);
			limitOrder.Price = Price;
			return limitOrder;
		}

		public override string ToString () {
			return $"Limit{(IsBuyOrder ? "Buy" : "Sell")} (Time: {Time} Currency: {Currencies.GetCurrencyToken (Currency)} Value: {NumberFormatting.FormatAmount (Value, Currency)} Price: {NumberFormatting.FormatAmount (Value, Currency)})";
		}

	}
}
