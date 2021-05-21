using CryptoTrader.NicehashAPI;
using CryptoTrader.NicehashAPI.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CryptoTrader {

	public class PriceGraph {

		public Currency Currency { get; }
		private readonly SortedDictionary<long, double> prices;

		public PriceGraph (Currency currency) {
			prices = new SortedDictionary<long, double> ();
			Currency = currency;
		}

		public void AddPriceValue (long milliTime, double price) {
			prices.Add (milliTime, price);
		}

		public double GetPrice (long milliTime) {

			int i = 1;
			KeyValuePair<long, double> lastPair = prices.ElementAt (0);
			while (i < prices.Count) {

				KeyValuePair<long, double> pair = prices.ElementAt (i);
				if (pair.Key >= milliTime) {
					double progress = MoreMath.InverseLerp (lastPair.Key, pair.Key, milliTime);
					return MoreMath.Lerp (lastPair.Value, lastPair.Value, progress);
				}

				lastPair = pair;
				i++;
			}
			return -1;
		}

		public double GetLastPrice () {
			return prices.Last ().Value;
		}

		public PriceUnit[] ConvertToPriceUnits () {
			PriceUnit[] priceUnits = new PriceUnit[prices.Count];
			int i = 0;
			foreach (KeyValuePair<long, double> pricePoint in prices) {
				priceUnits[i++] = new PriceUnit (Currency, pricePoint.Key, pricePoint.Value);
			}
			return priceUnits;
		}

		public override string ToString () {
			StringBuilder sb = new StringBuilder ();
			sb.Append ($"PriceGraph for {Enum.GetName(typeof(Currency), Currency)}:\n");
			foreach (KeyValuePair<long, double> pricePoint in prices) {
				sb.Append ($"\t{pricePoint.Key} | {pricePoint.Value} \n");
			}
			return sb.ToString ();
		}

	}

}
