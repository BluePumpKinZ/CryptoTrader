using CryptoTrader.NicehashAPI;
using CryptoTrader.Utils;

namespace CryptoTrader.Algorithms.Orders {

	public abstract class MarketOrder : Order {

		public override string GetOrderUrl () {
			return base.GetOrderUrl () + $"&secQuantity={NumberFormatting.FormatAmount (0.9 * Value, Currency)}";
		}

		public override string ToString () {
			return $"Market{(IsBuyOrder ? "Buy" : "Sell")} (Time: {Time} Currency: {Currencies.GetCurrencyToken (Currency)} Value: {NumberFormatting.FormatAmount (Value, Currency)})";
		}

	}
}
