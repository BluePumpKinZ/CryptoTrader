using CryptoTrader.NicehashAPI;
using CryptoTrader.NicehashAPI.JSONObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoTrader.Algorithms {

	public class Algo80s : Algorithm {

		internal override void Iterate (PriceGraph graph, ref Balances balance) {

			/*Random r = new Random ();
			double d = r.NextDouble ();
			if (d < 0.01) {
				if (balances.CanBuy (Currency.Bitcoin, 0.000005)) {
					BuyMarket (Currency.Mithril, 0.0005);
					Console.WriteLine ("Bought");
				}
			}
			if (d > 0.99) {
				if (balances.CanSell (Currency.Mithril, 200)) {
					SellMarket (Currency.Mithril, 200);
					Console.WriteLine ("Sold");
				}
			}*/
		}
	}
}
