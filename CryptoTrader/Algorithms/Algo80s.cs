using CryptoTrader.Algorithms.Orders;
using CryptoTrader.NicehashAPI;
using CryptoTrader.NicehashAPI.JSONObjects;

namespace CryptoTrader.Algorithms {

	public class Algo80s : Algorithm {

		private protected override void IterateInternal (PriceGraph graph, ref Balances balances) {

			double price = graph.GetLastPrice ();
			long time = graph.GetLastTime ();

			double valuebtc = balances.GetBalanceForCurrency (Currency.Bitcoin).Available;
			double valueCoin = balances.GetBalanceForCurrency (graph.Currency).Available;

			if (price < 0.00658) {

				MarketBuyOrder order = new MarketBuyOrder (graph.Currency, valuebtc * 0.5, time);
				CreateOrder (order, ref balances);
			}

			if (price > 0.00736) {
				MarketSellOrder order = new MarketSellOrder (graph.Currency, valueCoin * 0.5, time);
				CreateOrder (order, ref balances);
			}
		}
	}
}
