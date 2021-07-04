using CryptoTrader.NicehashAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoTrader.Algorithms {

	public struct LimitOrder {

		public Currency Currency { private set; get; }
		public double Value { private set; get; }
		public double Price { private set; get; }
		public OrderType Type { private set; get; }

		public LimitOrder (Currency currency, double value, double price, OrderType type) {
			Currency = currency;
			Value = value;
			Price = price;
			Type = type;
		}

	}
}
