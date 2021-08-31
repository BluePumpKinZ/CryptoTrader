using CryptoTrader.AISystem;
using CryptoTrader.Algorithms.Orders;
using CryptoTrader.NicehashAPI.JSONObjects;
using CryptoTrader.Utils;
using System;
using System.Collections.Generic;
using System.Threading;

namespace CryptoTrader.Algorithms {

	public class Algo80s : Algorithm, IImprovableAlgorithm {

		private double minBound = 0.2;
		private double maxBound = 0.8;
		private double transactionAmount = 0.5;
		private long timeframe = 1000 * 60 * 30;

		private protected override void IterateInternal (PriceGraph graph, ref Balances balances) {

			if (graph.GetTimeLength () < timeframe)
				return;

			long time = graph.GetLastTime ();
			graph.GetMinMaxPriceInRange (time - timeframe, time, out double minPriceInRange, out double maxPriceInRange);

			double price = graph.GetLastPrice ();

			double valuebtc = GetAvailableBTC (balances);
			double valueCoin = balances.GetBalanceForCurrency (graph.Currency).Available;

			if (price < MoreMath.Lerp (minPriceInRange, maxPriceInRange, minBound) - PriceWatcher.FeeStatus.MakerCoefficient * price) {
				MarketBuyOrder order = new MarketBuyOrder (graph.Currency, valuebtc * transactionAmount, time);
				if (balances.CanExecute (order)) {
					bool succes = CreateOrder (order, ref balances);
					if (!IsTraining)
						Console.WriteLine ("Buy: " + succes + " | " + order);
				}
			}

			if (price > MoreMath.Lerp (minPriceInRange, maxPriceInRange, maxBound) + PriceWatcher.FeeStatus.MakerCoefficient * price) {
				MarketSellOrder order = new MarketSellOrder (graph.Currency, valueCoin * transactionAmount, time);
				if (balances.CanExecute (order)) {
					bool succes = CreateOrder (order, ref balances);
					if (!IsTraining)
						Console.WriteLine ("Sell: " + succes + " | " + order);
				}
			}
		}

		public override void LoadFromBytes (ref int index, byte[] data) {
			base.LoadFromBytes (ref index, data);
			minBound = BitConverter.ToDouble (IStorable.GetDataRange (ref index, data));
			maxBound = BitConverter.ToDouble (IStorable.GetDataRange (ref index, data));
			transactionAmount = BitConverter.ToDouble (IStorable.GetDataRange (ref index, data));
			timeframe = BitConverter.ToInt64 (IStorable.GetDataRange (ref index, data));
		}

		public override void SaveToBytes (ref List<byte> datalist) {
			base.SaveToBytes (ref datalist);
			IStorable.AddData (ref datalist, BitConverter.GetBytes (minBound));
			IStorable.AddData (ref datalist, BitConverter.GetBytes (maxBound));
			IStorable.AddData (ref datalist, BitConverter.GetBytes (transactionAmount));
			IStorable.AddData (ref datalist, BitConverter.GetBytes (timeframe));
		}

		public override ICopyable Copy () {
			Algo80s algorithm = new Algo80s ();
			algorithm.minBound = minBound;
			algorithm.maxBound = maxBound;
			algorithm.transactionAmount = transactionAmount;
			algorithm.timeframe = timeframe;
			return CopyAbstractValues (algorithm);
		}

		private Algo80s[] trainingAlgos;
		private double[] trainingAlgoLosses;
		private double trainingLastLoss;

		public void Improve (int epochs, int threads) {

			if (trainingAlgos != null || trainingAlgoLosses != null)
				throw new OperationCanceledException ("Conflicting threads!");

			Random random = new Random ();

			trainingLastLoss = GetLoss ();

			for (int i = 0; i < epochs; i++) {

				double currentLoss = trainingLastLoss;
				trainingAlgos = new Algo80s[threads];
				trainingAlgoLosses = new double[threads];

				for (int j = 0; j < threads; j++) {
					int k = j;
					AIProcessTaskScheduler.AddTask (() => {
						Algo80s algo = (Algo80s)Copy ();
						switch (random.Next (4)) {
						case 0:
							algo.minBound = minBound * GetRandomAdjustment (ref random);
							break;
						case 1:
							algo.maxBound = maxBound * GetRandomAdjustment (ref random);
							break;
						case 2:
							algo.transactionAmount = transactionAmount * GetRandomAdjustment (ref random);
							break;
						case 3:
							algo.timeframe = (long)(timeframe * GetRandomAdjustment (ref random));
							break;
						}
						trainingAlgos[k] = algo;
						trainingAlgoLosses[k] = algo.GetLoss ();
					});
				}

				bool canContinue;
				do {
					canContinue = true;
					for (int j = 0; j < threads; j++)
						if (trainingAlgoLosses[j] == 0)
							canContinue = false;
					Thread.Sleep (1);
				} while (!canContinue);

				int bestIndex = 0;
				for (int j = 1; j < threads; j++)
					if (trainingAlgoLosses[j] < trainingAlgoLosses[bestIndex])
						bestIndex = j;

				if (trainingAlgoLosses[bestIndex] > currentLoss) {
					Console.WriteLine ($"Skipped a step: currentloss = {currentLoss}, bestloss = {trainingAlgoLosses[bestIndex]}");
					continue;
				}

				minBound = trainingAlgos[bestIndex].minBound;
				maxBound = trainingAlgos[bestIndex].maxBound;
				transactionAmount = trainingAlgos[bestIndex].transactionAmount;
				timeframe = trainingAlgos[bestIndex].timeframe;
				trainingLastLoss = trainingAlgoLosses[bestIndex];
			}

			trainingAlgos = null;
			trainingAlgoLosses = null;
		}

		public double GetLoss () {
			return 1 / ExecuteOnPriceGraph (PriceWatcher.GetGraphForCurrency (PrimaryCurrency));
		}

		private double GetRandomAdjustment (ref Random random) {
			return 0.0035 * (2 * random.NextDouble () - 1) + 1;
		}
	}
}
