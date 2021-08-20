using CryptoTrader.Algorithms.Orders;
using CryptoTrader.NicehashAPI;
using CryptoTrader.NicehashAPI.JSONObjects;
using CryptoTrader.Utils;
using System;
using System.Collections.Generic;

namespace CryptoTrader.Algorithms {

	public abstract class Algorithm : IStorable, ICopyableAbstract {

		private bool isTraining;
		public bool IsTraining { get { return isTraining; } set { SetTrainingMode (value); } }
		internal List<LimitOrder> trainingLimitOrders = new List<LimitOrder> ();
		private Balances trainingModeBalances = new Balances ();
		public Currency PrimaryCurrency { private set; get; } = Currency.Null;
		private double totalBalancesRatioAssinged;
		public double TotalBalancesRatioAssinged { private protected set { totalBalancesRatioAssinged = MoreMath.Clamp01 (value); } get { return totalBalancesRatioAssinged; } }

		public void SetPrimaryCurrency (Currency currency) {
			if (PrimaryCurrency == Currency.Null)
				PrimaryCurrency = currency;
			else
				throw new ApplicationException ("The primary currency was already set and cannot be changed afterwards.");
		}

		private void SetTrainingMode (bool enableTraining) {
			if (enableTraining == isTraining) {
				Console.WriteLine ($"Warning! Training mode was already {(enableTraining ? "enabled" : "disabled")}");
			}
			isTraining = enableTraining;
		}

		public virtual void LoadFromBytes (ref int index, byte[] data) {
			PrimaryCurrency = Currencies.GetCurrencyFromHash (BitConverter.ToUInt32 (IStorable.GetDataRange (ref index, data)));
			totalBalancesRatioAssinged = BitConverter.ToDouble (IStorable.GetDataRange (ref index, data));
			IsTraining = BitConverter.ToBoolean (IStorable.GetDataRange (ref index, data));
		}

		public virtual void SaveToBytes (ref List<byte> datalist) {
			IStorable.AddData (ref datalist, BitConverter.GetBytes (Currencies.GetCurrencyTokenHash (PrimaryCurrency)));
			IStorable.AddData (ref datalist, BitConverter.GetBytes (totalBalancesRatioAssinged));
			IStorable.AddData (ref datalist, BitConverter.GetBytes (isTraining));
		}

		private protected abstract void IterateInternal (PriceGraph graph, ref Balances balances);

		public void Iterate (PriceGraph graph, ref Balances balances) {
			ExecuteLimitOrders (balances);
			IterateInternal (graph, ref balances);
		}

		public double ExecuteOnPriceGraph (PriceGraph graph) {
			SetTrainingMode (true);

			const double startWalletValue = 10000; // USD
			long startTime = graph.GetTimeByIndex (0);
			long endTime = startTime + graph.GetTimeLength ();

			PriceGraph newGraph = new PriceGraph (graph.Currency);
			trainingModeBalances = new Balances ();
			PriceGraph tetherGraph = PriceWatcher.GetGraphForCurrency (Currency.Tether);
			Balance balance = new Balance (Currency.Tether, startWalletValue, tetherGraph.GetPrice (tetherGraph.GetTimeByIndex (0))).ToCurrency (Currency.Bitcoin, 1);
			double startBtc = balance.Total;
			trainingModeBalances.AddBalance (balance);
			trainingModeBalances.AddEmptyBalance (graph.Currency, graph.GetPrice (startTime));

			for (long time = startTime; time < endTime; time += 60 * 1000) {
				newGraph.AddPriceValue (time, graph.GetPrice (time, true));
				trainingModeBalances.UpdateBTCRateForCurrency (graph.Currency, newGraph.GetLastPrice ());
				Iterate (newGraph, ref trainingModeBalances);
				Console.Title = $"{10000 * (time - startTime) / (endTime - startTime) / 100.0}% {time}";
			}
			double endBtc = trainingModeBalances.TotalBalance.Total;

			SetTrainingMode (false);
			Console.WriteLine (trainingModeBalances);
			return endBtc / startBtc;
		}

		public void Reset () {
			trainingLimitOrders.Clear ();
			trainingModeBalances = new Balances ();
		}

		private void ExecuteLimitOrders (Balances balances) {
			for (int i = trainingLimitOrders.Count - 1; i >= 0; i--) {
				LimitOrder order = trainingLimitOrders[i];

				if (order.HasPriceBeenReached (balances)) {
					if (CreateOrder (order.ToMarketOrder (), ref trainingModeBalances))
						trainingLimitOrders.RemoveAt (i);
				}
			}
		}

		internal bool CreateOrder (Order order, ref Balances balances) {
			if (!isTraining) {
				bool succes = !ExchangePrivate.CreateOrder (order).Contains ("error");
				if (!succes)
					return succes;
			}
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
			} else {
				// Limit Order
				trainingLimitOrders.Add (order as LimitOrder);
			}
			return true;
		}

		public ICopyable CopyAbstractValues (ICopyable copy) {
			Algorithm algo = (Algorithm)copy;
			algo.IsTraining = isTraining;
			algo.PrimaryCurrency = PrimaryCurrency;
			algo.trainingModeBalances = trainingModeBalances;
			algo.trainingLimitOrders = trainingLimitOrders.CopyMembers ();
			algo.totalBalancesRatioAssinged = totalBalancesRatioAssinged;
			return algo;
		}

		public abstract ICopyable Copy ();
	}
}
