﻿using CryptoTrader.NicehashAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptoTrader {

	public class PriceGraph {

		public Currency Currency { get; }
		private readonly SortedList<long, double> prices;

		public PriceGraph (Currency currency) {
			prices = new SortedList<long, double> ();
			Currency = currency;
		}

		public void AddPriceValue (long milliTime, double price) {
			prices.Add (milliTime, price);
		}

		public double GetPrice (long milliTime, bool raw = false) {

			int i = 0;
			KeyValuePair<long, double> lastPair = prices.ElementAt (0);
			foreach (KeyValuePair<long, double> pair in prices) {
				if (i++ == 0)
					continue;

				if (pair.Key >= milliTime) {
					double progress = MoreMath.InverseLerp (lastPair.Key, pair.Key, milliTime);
					double interpolatedPrice = MoreMath.Lerp (lastPair.Value, lastPair.Value, progress);
					if (Currency == Currency.Tether && !raw)
						interpolatedPrice = 1 / interpolatedPrice;
					return interpolatedPrice;
				}

				lastPair = pair;
			}
			return -1;
		}

		public long GetTimeByIndex (int i) {
			return prices.ElementAt (i).Key;
		}

		public double GetPriceByIndex (int i, bool raw) {
			double price = prices.ElementAt (i).Value;
			if (Currency == Currency.Tether && !raw)
				price = 1 / price;
			return price;
		}

		public double GetLastPrice (bool raw = false) {
			double price = prices.Last ().Value;
			if (Currency == Currency.Tether && !raw)
				price = 1 / price;
			return price;
		}

		public int GetLength () {
			return prices.Count;
		}

		public long GetTimeLength () {
			long min = prices.Keys.Min ();
			long max = prices.Keys.Max ();
			return max - min;
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
			sb.Append ($"PriceGraph for {Enum.GetName (typeof (Currency), Currency)}:\n");
			foreach (KeyValuePair<long, double> pricePoint in prices) {
				sb.Append ($"\t{pricePoint.Key} | {pricePoint.Value} \n");
			}
			return sb.ToString ();
		}

	}

}
