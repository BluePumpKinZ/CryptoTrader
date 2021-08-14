using CryptoTrader.Algorithms.Orders;
using CryptoTrader.Utils;
using System;

namespace CryptoTrader.AISystem {

	public static class AIDataConversion {

		public const int INPUT_LAYER_SAMPLES = 5000;
		public const long TIMEFRAME = 1000L * 60 * 60 * 24 * 30; // One month

		public static void GetTrainingDataBatch (PriceGraph graph, int batchSize, long minimumTimeframe, out double[][] input, out double[][] desiredOutput) {

			if (batchSize < 0)
				throw new ArgumentException ("Batch size can't be negative.", "batchSize");

			input = new double[batchSize][];
			desiredOutput = new double[batchSize][];

			int minIndex = 0;
			long minimumTime = graph.GetStartTime () + minimumTimeframe;
			for (int i = 0; i < graph.GetLength (); i++) {
				if (graph.GetTimeByIndex (i) > minimumTime) {
					minIndex = i;
					break;
				}
			}

			double[] output = GetDesiredNetworkOutputFromPriceGraph (graph);
			Random random = new Random ();
			for (int i = 0; i < batchSize; i++) {

				int randomIndex = random.Next (minIndex, graph.GetLength () - 1);

				PriceGraph rangedGraph = graph.GetRange (randomIndex);
				input[i] = GetNetworkInputFromPriceGraph (rangedGraph, minimumTimeframe);
				desiredOutput[i] = new double[] { output[i] };
			}

		}

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

			if (timeframe > graph.GetTimeLength ())
				throw new ArgumentException ("The pricegraph does not have enough data to cover the timeframe.");

			double lastPrice = graph.GetLastPrice ();
			long lastTime = graph.GetLastTime ();

			int totalSamples = INPUT_LAYER_SAMPLES;
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

			for (int i = 0; i < length; i++) {

				if (buying)
					amountsInvested[i] = 1;

				long time = graph.GetTimeByIndex (i);

				if (order.Time > time)
					continue;

				if (orderCount + 1 >= orders.Length)
					break;

				order = orders[++orderCount];
				buying = order.IsBuyOrder;
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

		public static LayerState[] ConvertToLayerStates (ref double[][] values) {
			LayerState[] output = new LayerState[values.Length];
			for (int i = 0; i < output.Length; i++)
				output[i] = new LayerState (values[i]);
			return output;
		}

	}

}
