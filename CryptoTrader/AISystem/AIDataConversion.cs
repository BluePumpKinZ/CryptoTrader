using CryptoTrader.Algorithms.Orders;
using CryptoTrader.Utils;
using System;

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

		public static double[] GetNetworkInputFromPriceGraph (PriceGraph graph, long timeframe) {

			if (graph.GetTimeLength () < timeframe)
				throw new ArgumentException ("The pricegraph does not have enough data to cover the timeframe.");

			double lastPrice = graph.GetLastPrice ();
			long lastTime = graph.GetLastTime ();

			int totalSamples = 10000;
			double[] values = new double[totalSamples];
			long[] times = new long[totalSamples];

			double f = GetFForScaledTimeFrom01 (60000, timeframe, totalSamples);
			for (long i = 0; i < totalSamples; i++) {
				long timeDelta = GetScaledTimeFrom01 ((double)i / totalSamples, timeframe, f);
				long time = lastTime - timeDelta;
				times[i] = timeDelta;
				double price = graph.GetPrice (time);
				double adjustedprice = -MoreMath.Sigmoid (3 * (price / lastPrice - 1));
				values[i] = adjustedprice;
			}

			return values;
		}

		public static double[] GetDesiredNetworkOutputFromPriceGraph (PriceGraph graph, MarketOrder[] orders = null) {

			if (orders == null)
				orders = graph.GetOptimalTrades (out _);

			int length = graph.GetLength ();

			double[] amountsInvested = new double[length];

			int orderCount = 0;
			MarketOrder order = orders[0];

			bool buying = order.IsBuyOrder;
			bool selling = order.IsSellOrder;

			for (int i = 0; i < length; i++) {

				if (buying)
					amountsInvested[i] = 1;
				if (selling)
					amountsInvested[i] = -1;

				long time = graph.GetTimeByIndex (i);

				if (order.Time > time)
					continue;

				if (orderCount + 1 >= orders.Length)
					break;

				order = orders[++orderCount];
				buying = order.IsBuyOrder;
				selling = order.IsSellOrder;
			}

			return amountsInvested;
		}

		// As made here: https://www.desmos.com/calculator/szflobahzg
		/// <summary>
		/// Gets the parameter f for function GetScaledTimeFrom01. This factor is the same for every sample so whe store it.
		/// </summary>
		/// <param name="secondListItemValue"></param>
		/// <param name="totalSpan"></param>
		/// <param name="totalSamples"></param>
		/// <returns></returns>
		private static double GetFForScaledTimeFrom01 (long secondListItemValue, long totalSpan, int totalSamples) {
			
			double g = secondListItemValue;
			double m = totalSpan;
			double as_ = 1 - 1.0 / totalSamples;
			double f = Math.Log ((m - g) / m) / Math.Log (as_);
			return f;
		}

		/// <summary>
		/// Scales 0..1 to a space where values in the beginning will be squished together very precisely while discarding higher ones. Call GetFForScaledTimeFrom01 first to get f.
		/// </summary>
		/// <param name="t"></param>
		/// <param name="secondListItemValue"></param>
		/// <param name="totalSpan"></param>
		/// <param name="totalSamples"></param>
		/// <param name="f"></param>
		/// <returns></returns>
		private static long GetScaledTimeFrom01 (double t, long totalSpan, double f) {
			double h = totalSpan - totalSpan * Math.Pow (1 - t, f);
			return (long)h;
		}

	}

}
