using CryptoTrader.Algorithms.Orders;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoTrader.AISystem {

	public static class AIDataConversion {

		public static Tuple<double[], double[]> GetBuyAndSellsFromOrders (PriceGraph graph, MarketOrder[] orders) {

			int length = graph.GetLength ();

			double[] buys = new double[length];
			double[] sells = new double[length];

			int orderCount = 0;
			MarketOrder order = orders[0];

			for (int i = 0; i < length; i++) {
				long time = graph.GetTimeByIndex (i);

				if (order.Time > time)
					continue;

				if (order.IsBuyOrder)
					buys[i] = 1;
				else
					sells[i] = 1;

				if (orderCount + 1 >= orders.Length)
					break;
				
				order = orders[++orderCount];
			}

			return new Tuple<double[], double[]> (buys, sells);
		}

	}

}
