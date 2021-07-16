using CryptoTrader.Exceptions;
using CryptoTrader.NicehashAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoTrader {

	public class PriceGraph {

		public Currency Currency { get; }
		private readonly List<GraphUnit> prices;

		public PriceGraph (Currency currency) {
			prices = new List<GraphUnit> ();
			Currency = currency;
		}

		public void AddPriceValue (long milliTime, double price) {

			int insertionIndex = prices.Count - 1;
			while (insertionIndex >= 0 && prices[insertionIndex].Time > milliTime) {
				insertionIndex--;
			}

			prices.Insert (insertionIndex + 1, new GraphUnit (milliTime, price));
		}

		public double GetPrice (long milliTime, bool raw = false) {

			if (prices.Count == 0)
				throw new NoPricesFoundException ($"Graph for currency \"{Currency}\" is empty.");

			int min = 0, max = prices.Count, searchIndex;
			while (max - min > 1) {
				searchIndex = (min + max) / 2;
				long time = prices[searchIndex].Time;
				if (milliTime < time) {
					max = searchIndex;
				} else {
					min = searchIndex;
				}
			}

			GraphUnit minUnit = prices[min];
			GraphUnit maxUnit = prices[max];
			double t = MoreMath.InverseLerp (minUnit.Time, maxUnit.Time, milliTime);
			t = MoreMath.Clamp01 (t);
			double price = MoreMath.Lerp (minUnit.Value, maxUnit.Value, t);
			if (Currency == Currency.Tether && !raw)
				price = 1 / price;
			return price;
		}

		public long GetTimeByIndex (int i) {
			return prices[i].Time;
		}

		public double GetPriceByIndex (int i, bool raw) {
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
			long min = long.MaxValue;
			long max = 0;

			for (int i = 0; i < prices.Count; i++) {
				long time = prices[i].Time;
				min = Math.Min (min, time);
				max = Math.Max (max, time);
			}

			return max - min;
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

		protected struct GraphUnit : IComparable<GraphUnit> {

			internal long Time { private set; get; }
			internal double Value { private set; get; }

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
