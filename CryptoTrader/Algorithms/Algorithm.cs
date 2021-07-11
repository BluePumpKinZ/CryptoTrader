using CryptoTrader.NicehashAPI;
using CryptoTrader.NicehashAPI.JSONObjects;
using System;
using System.Collections.Generic;

namespace CryptoTrader.Algorithms {
	
	public abstract class Algorithm {

		internal bool isTraining;
		public bool IsTraining { get { return isTraining; } set { SetTrainingMode (value); } }
		internal List<LimitOrder> trainingLimitOrders = new List<LimitOrder> ();
		internal Balances balances = new Balances ();

		private void SetTrainingMode (bool enableTraining) {
			if (enableTraining == isTraining) {
				Console.WriteLine ($"Warning! Training mode was already {(enableTraining ? "enabled" : "disabled")}");
			}
			isTraining = enableTraining;
		}

		internal abstract void Iterate (PriceGraph graph, ref Balances balance);

		public double ExecuteOnPriceGraph (PriceGraph graph) {

			SetTrainingMode (true);

			const double startWalletValue = 100; // USD
			long startTime = graph.GetTimeByIndex (0);
			long endTime = startTime + graph.GetTimeLength ();

			PriceGraph newGraph = new PriceGraph (graph.Currency);
			balances = new Balances ();
			PriceGraph tetherGraph = PriceWatcher.GetGraphForCurrency (Currency.Tether);
			Balance balance = new Balance (Currency.Tether, startWalletValue, tetherGraph.GetPrice (startTime)).ToCurrency (Currency.Bitcoin, 1);
			double startBtc = balance.Total;
			balances.AddBalance (balance);
			balances.AddEmptyBalance (graph.Currency, graph.GetPrice (startTime));
			
			for (long time = startTime; time < endTime; time += 60 * 10000) {
				newGraph.AddPriceValue (time, graph.GetPrice (time, true));
				balances.UpdateBTCRateForCurrency (graph.Currency, newGraph.GetLastPrice ());
				Iterate (newGraph, ref balances);
				Console.Title = $"{10000 * (time - startTime) / (endTime - startTime) / 100.0}% {time}";
			}
			double endBtc = balances.TotalBalance.Total;

			SetTrainingMode (false);
			Console.WriteLine ($"From: {startBtc} BTC to {endBtc} BTC.");
			return endBtc / startBtc;
		}

		public void Reset () {
			trainingLimitOrders.Clear ();
			balances = new Balances ();
		}

		internal void BuyMarket (Currency currency, double value) {
			CreateOrder (currency, value, 0, OrderType.BuyMarket);
		}

		internal void BuyLimit (Currency currency, double value, double price) {
			CreateOrder (currency, value, price, OrderType.BuyLimit);
		}

		internal void SellMarket (Currency currency, double value) {
			CreateOrder (currency, value, 0, OrderType.SellMarket);
		}

		internal void SellLimit (Currency currency, double value, double price) {
			CreateOrder (currency, value, price, OrderType.SellLimit);
		}

		internal void ExecuteLimitOrders () {
			for (int i = trainingLimitOrders.Count - 1; i >= 0; i--) {
				LimitOrder order = trainingLimitOrders[i];
				if (order.Type == OrderType.BuyLimit && balances.GetBalanceForCurrency (order.Currency).BTCRate <= order.Price) {
					CreateOrder (order.Currency, order.Value, 0, OrderType.BuyMarket);
					trainingLimitOrders.RemoveAt (i);
					continue;
				}
				if (order.Type == OrderType.SellLimit && balances.GetBalanceForCurrency (order.Currency).BTCRate >= order.Price) {
					CreateOrder (order.Currency, order.Value, 0, OrderType.SellMarket);
					trainingLimitOrders.RemoveAt (i);
					continue;
				}
			}
		}

		private bool CreateOrder (Currency currency, double value, double price, OrderType type) {
			if (!isTraining) {
				return !ExchangePrivate.CreateOrder (currency, type, value, price).Contains ("error");
			} else {
				if (type == OrderType.BuyMarket || type == OrderType.SellMarket) {
					// Market Order

					Balance source, dest;
					if (type == OrderType.BuyMarket) {
						source = balances.GetBalanceForCurrency (Currency.Bitcoin);
						dest = balances.GetBalanceForCurrency (currency);
					} else {
						source = balances.GetBalanceForCurrency (currency);
						dest = balances.GetBalanceForCurrency (Currency.Bitcoin);
					}
					if (source.Available < value)
						return false;

					Balance transactionBalance = new Balance (source.Currency, value, source.BTCRate);
					source.Subtract (transactionBalance);
					transactionBalance.ApplyFee (PriceWatcher.FeeStatus.MakerCoefficient);
					dest.Add (transactionBalance);
					return true;
				} else {
					// Limit Order

					LimitOrder order = new LimitOrder (currency, value, price, type);
					trainingLimitOrders.Add (order);
					return true;
				}
			}
		}

	}
}
