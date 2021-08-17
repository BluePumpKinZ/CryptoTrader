namespace CryptoTrader.Algorithms.Orders {

	public abstract class MarketOrder : Order {

		new public string GetOrderUrl () {
			return base.GetOrderUrl () + $"&secQuantity={0.9 * Value}";
		}

		public override string ToString () {
			return $"Market{(IsBuyOrder ? "Buy" : "Sell")}Order (Time: {Time} Currency: {Currency} Value: {Value})";
		}

	}
}
