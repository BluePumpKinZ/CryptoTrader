using CryptoTrader.NicehashAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoTrader {

	public struct PriceUnit {

		public Currency Currency { get; }
		public long MilliTime { get; }
		public double Price { get; }

		public PriceUnit (Currency currency, long milliTime, double price) {
			Currency = currency;
			MilliTime = milliTime;
			Price = price;
		}

		public override string ToString () {
			return $"Currency: {Currency} | Price {Price} | MilliTime {MilliTime}";
		}
	}
}
