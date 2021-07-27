using CryptoTrader.Algorithms.Orders;
using CryptoTrader.NicehashAPI;
using CryptoTrader.NicehashAPI.JSONObjects;
using System;
using System.Collections.Generic;

namespace CryptoTrader.Algorithms {

	public abstract class Algorithm {

		private bool isTraining;
		public bool IsTraining { get { return isTraining; } set { SetTrainingMode (value); } }
		internal List<LimitOrder> trainingLimitOrders = new List<LimitOrder> ();
		internal Balances balances = new Balances ();

		private void SetTrainingMode (bool enableTraining) {
			if (enableTraining == isTraining) {
				Console.WriteLine ($"Warning! Training mode was already {(enableTraining ? "enabled" : "disabled")}");
			}
			isTraining = enableTraining;
		}

		internal abstract void IterateInternal (PriceGraph graph, ref Balances balances);

		internal void Iterate (PriceGraph graph, ref Balances balances) {
			ExecuteLimitOrders (balances);
			IterateInternal (graph, ref balances);
		}

		public double ExecuteOnPriceGraph (PriceGraph graph) {
			SetTrainingMode (true);

			const double startWalletValue = 100; // USD
			long startTime = graph.GetTimeByIndex (0);
			long endTime = startTime + graph.GetTimeLength ();

			PriceGraph newGraph = new PriceGraph (graph.Currency);
			balances = new Balances ();
			PriceGraph tetherGraph = PriceWatcher.GetGraphForCurrency (Currency.Tether);
			Balance balance = new Balance (Currency.Tether, startWalletValue, tetherGraph.GetPrice (tetherGraph.GetTimeByIndex (0))).ToCurrency (Currency.Bitcoin, 1);
			double startBtc = balance.Total;
			balances.AddBalance (balance);
			balances.AddEmptyBalance (graph.Currency, graph.GetPrice (startTime));

			for (long time = startTime; time < endTime; time += 60 * 1000) {
				newGraph.AddPriceValue (time, graph.GetPrice (time, true));
				balances.UpdateBTCRateForCurrency (graph.Currency, newGraph.GetLastPrice ());
				Iterate (newGraph, ref balances);
				// Console.Title = $"{10000 * (time - startTime) / (endTime - startTime) / 100.0}% {time}";
			}
			double endBtc = balances.TotalBalance.Total;

			SetTrainingMode (false);
			Console.WriteLine (balances);
			return endBtc / startBtc;
		}

		public void Reset () {
			trainingLimitOrders.Clear ();
			balances = new Balances ();
		}

		private void ExecuteLimitOrders (Balances balances) {
			for (int i = trainingLimitOrders.Count - 1; i >= 0; i--) {
				LimitOrder order = trainingLimitOrders[i];

				if (order.HasPriceBeenReached (balances)) {
					if (CreateOrder (order.ToMarketOrder ()))
						trainingLimitOrders.RemoveAt (i);
				}
			}
		}

		internal bool CreateOrder (Order order) {
			if (!isTraining) {
				bool succes = !ExchangePrivate.CreateOrder (order).Contains ("error");
				if (succes)
					balances.UpdateValuesFromBalances(Accounting.GetBalances ());
				return succes;
			} else {
				if (order.IsMarketOrder) {
					// Market Order

					Balance source, dest;
					if (order.IsBuyOrder) {
						source = balances.GetBalanceForCurrency (Currency.Bitcoin);
						dest = balances.GetBalanceForCurrency (order.Currency);
						if (source.Available < ExchangePrivate.MINIMUM_ORDER_QUANTITY_BTC)
							return false;
					} else {
						source = balances.GetBalanceForCurrency (order.Currency);
						dest = balances.GetBalanceForCurrency (Currency.Bitcoin);
						if (source.ToBTCBalance ().Available < ExchangePrivate.MINIMUM_ORDER_QUANTITY_BTC)
							return false;
					}
					if (source.Available < order.Value)
						return false;

					Balance transactionBalance = new Balance (source.Currency, order.Value, source.BTCRate);
					source.Subtract (transactionBalance);
					transactionBalance.ApplyFee (PriceWatcher.FeeStatus.MakerCoefficient);
					dest.Add (transactionBalance);
					return true;
				} else {
					// Limit Order

					trainingLimitOrders.Add (order as LimitOrder);
					return true;
				}
			}
		}

	}
}
