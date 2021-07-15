using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoTrader.Algorithms.Orders {

	public abstract class MarketOrder : Order {

		new public string GetOrderUrl () {
			return base.GetOrderUrl () + $"&secQuantity={0.9 * Value}";
		}

	}
}
