using CryptoTrader.Algorithms.Orders;
using CryptoTrader.Exceptions;
using CryptoTrader.NicehashAPI;
using CryptoTrader.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoTrader {

	public class PriceGraph {

		public Currency Currency { get; }
		private List<GraphUnit> prices;

		public PriceGraph (Currency currency) {
			prices = new List<GraphUnit> ();
			Currency = currency;
		}

		public void AddPriceValue (long milliTime, double price) {

			int insertionIndex = prices.Count - 1;
			while (insertionIndex >= 0 && prices[insertionIndex].Time > milliTime)
				insertionIndex--;

			prices.Insert (insertionIndex + 1, new GraphUnit (milliTime, price));
		}

		/*public void RemoveExtraData () {
			for (int i = prices.Count - 3; i >= 0; i--) {
				double price = prices[i].Value;
				double pricePlusOne = prices[i + 1].Value;
				double pricePlusTwo = prices[i + 2].Value;
				if (price == pricePlusOne && price == pricePlusTwo) {
					prices.RemoveAt (i + 1);
				}
			}
		}*/

		private void BinarySearchTimeIndex (long milliTime, out int min, out int max) {
			min = 0;
			max = prices.Count - 1;
			int searchIndex;
			while (max - min > 1) {
				searchIndex = (min + max) / 2;
				long time = prices[searchIndex].Time;
				if (milliTime < time) {
					max = searchIndex;
				} else {
					min = searchIndex;
				}
			}
		}

		public double GetPrice (long milliTime, bool raw = false) {

			if (prices.Count == 0)
				throw new NoPricesFoundException ($"Graph for currency \"{Currency}\" is empty.");

			BinarySearchTimeIndex (milliTime, out int min, out int max);

			GraphUnit minUnit = prices[min];
			GraphUnit maxUnit = prices[max];
			double t = MoreMath.InverseLerp (minUnit.Time, maxUnit.Time, milliTime);
			t = MoreMath.Clamp01 (t);
			double price = MoreMath.Lerp (minUnit.Value, maxUnit.Value, t);
			if (Currency == Currency.Tether && !raw)
				price = 1 / price;
			return price;
		}

		public PriceGraph GetRange (int endIndex) {
			PriceGraph copy = new PriceGraph (Currency);
			copy.prices = prices.GetRange (0, endIndex + 1);
			return copy;
		}

		public long GetTimeByIndex (int i) {
			return prices[i].Time;
		}

		public double GetPriceByIndex (int i, bool raw = false) {
			double price = prices[i].Value;
			if (Currency == Currency.Tether && !raw)
				price = 1 / price;
			return price;
		}

		public double GetLastPrice (bool raw = false) {
			double price = prices[^1].Value;
			if (Currency == Currency.Tether && !raw)
				price = 1 / price;
			return price;
		}

		public int GetLength () {
			return prices.Count;
		}

		public long GetStartTime () {
			return prices[0].Time;
		}

		public long GetLastTime () {
			return prices[^1].Time;
		}

		public long GetTimeLength () {
			return GetLastTime () - GetStartTime ();
		}

		public void GetMinMaxPriceInRange (long startTime, long endTime, out double min, out double max) {
			BinarySearchTimeIndex (startTime, out int startIndex1, out int startIndex2);
			BinarySearchTimeIndex (endTime, out int endIndex1, out int endIndex2);

			GraphUnit startUnit1 = prices[startIndex1];
			GraphUnit startUnit2 = prices[startIndex2];
			GraphUnit endUnit1 = prices[endIndex1];
			GraphUnit endUnit2 = prices[endIndex2];

			double firstPrice = MoreMath.Lerp (startUnit1.Value, startUnit2.Value, MoreMath.InverseLerp (startUnit1.Time, startUnit2.Time, startTime));
			double lastPrice = MoreMath.Lerp (endUnit1.Value, endUnit2.Value, MoreMath.InverseLerp (endUnit1.Time, endUnit2.Time, endTime));

			min = Math.Min (firstPrice, lastPrice);
			max = Math.Max (firstPrice, lastPrice);

			for (int i = startIndex2; i <= endIndex1; i++) {
				double price = prices[i].Value;
				min = Math.Min (min, price);
				max = Math.Max (max, price);
			}
		}

		public MarketOrder[] GetOptimalTrades (out double totalProfit) {
			return GetOptimalTrades (GetStartTime (), GetLastTime (), out totalProfit);
		}

		public MarketOrder[] GetOptimalTrades (long startTime, long endTime, out double totalProfit) {

			long graphStartTime = GetStartTime ();
			long graphEndTime = GetLastTime ();

			if (startTime < graphStartTime || endTime > graphEndTime || startTime > graphEndTime || endTime < graphStartTime)
				throw new ArgumentException ($"The requested range is invalid ({startTime} - {endTime}) does not fit in ({graphStartTime} - {graphEndTime}).");

			int startIndex = 0, endIndex = prices.Count - 1;

			while (GetTimeByIndex (startIndex) < startTime)
				startIndex++;

			while (GetTimeByIndex (endIndex) > endTime)
				endIndex--;

			List<MarketOrder> orders = new List<MarketOrder> ();

			double fee = PriceWatcher.FeeStatus.MakerCoefficient;
			double backAndForthFee = fee * 2;
			double profit = 1;

			for (int i = startIndex; i < endIndex; i++) {

				double frameStartPrice = GetPriceByIndex (i);
				long frameStartTime = GetTimeByIndex (i);
				int index = i;

				double price = GetPriceByIndex (i) / frameStartPrice;
				double maxPrice = price;
				double minPrice = price;

				int maxPriceIndex = index;
				int minPriceIndex = index;

				while (index < endIndex) {

					price = GetPriceByIndex (index) / frameStartPrice;

					if (price > maxPrice) {
						maxPrice = price;
						maxPriceIndex = index;
					}

					if (price < minPrice) {
						minPrice = price;
						minPriceIndex = index;
					}

					if (maxPrice > 1 + backAndForthFee && price < maxPrice - backAndForthFee) {
						// Buy

						MarketOrder order = new MarketBuyOrder (Currency, 1, frameStartTime);
						orders.Add (order);

						i = maxPriceIndex - 1;

						profit *= 1 - fee;
						break;
					}
					if (minPrice < 1 - backAndForthFee && price > minPrice + backAndForthFee) {
						// Sell

						MarketOrder order = new MarketSellOrder (Currency, 1, frameStartTime);
						orders.Add (order);

						i = minPriceIndex - 1;

						profit *= 1 - fee;
						profit *= maxPrice / minPrice;
						break;
					}

					index++;
				}
				if (index == endIndex)
					break;
			}

			totalProfit = profit;

			return orders.ToArray ();

		}

		public PriceUnit[] ConvertToPriceUnits () {
			PriceUnit[] priceUnits = new PriceUnit[prices.Count];
			int i = 0;
			foreach (GraphUnit pricePoint in prices) {
				priceUnits[i++] = new PriceUnit (Currency, pricePoint.Time, pricePoint.Value);
			}
			return priceUnits;
		}

		public override string ToString () {
			StringBuilder sb = new StringBuilder ();
			sb.Append ($"PriceGraph for {Enum.GetName (typeof (Currency), Currency)}:\n");
			foreach (GraphUnit pricePoint in prices) {
				sb.Append ($"\t{pricePoint.Time} | {pricePoint.Value} \n");
			}
			return sb.ToString ();
		}

		protected readonly struct GraphUnit : IComparable<GraphUnit> {

			internal long Time { get; }
			internal double Value { get; }

			public GraphUnit (long time, double value) {
				Time = time;
				Value = value;
			}

			public int CompareTo (GraphUnit other) {
				return Time.CompareTo (other.Time);
			}

			public override string ToString () {
				return $"{Time} | {Value}";
			}
		}

	}

}
