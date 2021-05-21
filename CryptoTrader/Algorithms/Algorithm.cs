using connect;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoTrader.Algorithms {
	
	public abstract class Algorithm {

		internal bool isTraining;
		public bool IsTraining { get { return isTraining; } set { SetTrainingMode (value); } }
		internal List<object> marketOrders = new List<object> ();
		internal List<object> limitOrders = new List<object> ();

		private void SetTrainingMode (bool enableTraining) {
			if (enableTraining == isTraining) {
				Console.WriteLine ($"Warning! Training mode was already {(enableTraining ? "enabled" : "disabled")}");
			}
			isTraining = enableTraining;
			if (enableTraining)
				EnableTrainingMode ();
			else
				DisableTrainingMode ();
		}

		internal abstract void EnableTrainingMode ();

		internal abstract void DisableTrainingMode ();

		public abstract double ExecuteOnPriceGraph (PriceGraph graph);

		public void Reset () {
			marketOrders.Clear ();
			limitOrders.Clear ();
		}

		internal void Buy (Currency currency, double value, double price, OrderType orderType) {
			switch (orderType) {
			case OrderType.Market:

				break;
			}
		}

		internal void BuyMarket (Currency currency, double value) {
			Buy (currency, value, 0, OrderType.Market);
		}

	}
}
