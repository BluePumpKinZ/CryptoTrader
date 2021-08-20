﻿using CryptoTrader.AISystem;
using CryptoTrader.Algorithms.Orders;
using CryptoTrader.NicehashAPI;
using CryptoTrader.NicehashAPI.JSONObjects;
using CryptoTrader.NicehashAPI.Utils;
using CryptoTrader.Utils;
using System;
using System.Collections.Generic;

namespace CryptoTrader.Algorithms {

	public class AlgoAI : Algorithm, IImprovableAlgorithm {

		public DeepLearningNetwork network;

		private AlgoAI (DeepLearningNetwork network) {
			this.network = network;
		}

		public AlgoAI () {
			NetworkStructure structure = new NetworkStructure (new int[] { AIDataConversion.INPUT_LAYER_SAMPLES, 100, 16, 1 });
			network = new DeepLearningNetwork (structure);
			network.Randomize ();
			TotalBalancesRatioAssinged = 0;
		}

		private protected override void IterateInternal (PriceGraph graph, ref Balances balances) {

			if (!IsTraining && graph.Currency != PrimaryCurrency)
				throw new ArgumentException ("Graph must have the same currency as PrimaryCurrency when running is live mode.", "graph");

			long timeframe = AIDataConversion.TIMEFRAME;

			if (graph.GetTimeLength () < timeframe)
				return;

			double[] networkInput = AIDataConversion.GetNetworkInputFromPriceGraph (graph, timeframe);
			LayerState networkOutput = network.Iterate (new LayerState (networkInput));
			double networkSuggestion = networkOutput[0];

			double totalBTC = balances.TotalBalance.Total;
			double totalAvailableBTC = totalBTC * TotalBalancesRatioAssinged;
			double soldBTC = balances.GetBalanceForCurrency (PrimaryCurrency).ToBTCBalance ().Total;
			double soldRatio = soldBTC / totalAvailableBTC;
			double soldBtcSuggestion = networkSuggestion * totalAvailableBTC;

			double btcDiff = Math.Abs (soldBTC - soldBtcSuggestion);

			if (btcDiff < ExchangePrivate.MINIMUM_ORDER_QUANTITY_BTC) {
				Console.Write ($"\rDiff: {btcDiff}BTC");
				return;
			}

			if (networkSuggestion >= soldRatio) {
				Order order = new MarketBuyOrder (PrimaryCurrency, totalAvailableBTC - btcDiff, NicehashSystem.GetUTCTimeMillis ());
				CreateOrder (order, ref balances);
			}

			if (networkSuggestion <= soldRatio) {
				Order order = new MarketSellOrder (PrimaryCurrency, (totalAvailableBTC - btcDiff) / graph.GetLastPrice (), NicehashSystem.GetUTCTimeMillis ());
				CreateOrder (order, ref balances);
			}

		}

		public override void LoadFromBytes (ref int index, byte[] data) {
			base.LoadFromBytes (ref index, data);
			network.LoadFromBytes (ref index, data);
		}

		public override void SaveToBytes (ref List<byte> datalist) {
			base.SaveToBytes (ref datalist);
			network.SaveToBytes (ref datalist);

		}

		public void Improve (int epochs, int threads) {

			Console.WriteLine ($"Started algorithm improvement for currency {PrimaryCurrency} for {epochs} epochs.");

			AIProcessTaskScheduler.RunOnThread (() => {

				PriceGraph graph = PriceWatcher.GetGraphForCurrency (PrimaryCurrency);
				int examples = graph.GetLength () / 100;
				long timeframe = AIDataConversion.TIMEFRAME;

				for (int i = 0; i < epochs; i++) {
					AIDataConversion.GetTrainingDataBatch (graph, examples, timeframe, out double[][] inputArrays, out double[][] outputArrays);
					LayerState[] inputs = AIDataConversion.ConvertToLayerStates (ref inputArrays);
					LayerState[] outputs = AIDataConversion.ConvertToLayerStates (ref outputArrays);
					network.TrainThreaded (inputs, outputs, 0.0002, threads);
				}

				Console.WriteLine ($"Finished {epochs} epochs for algorithm for currency {PrimaryCurrency}");

			});
		}

		public double GetLoss () {

			double totalLoss = 0;

			PriceGraph graph = PriceWatcher.GetGraphForCurrency (PrimaryCurrency);
			int examples = 100;
			long timeframe = AIDataConversion.TIMEFRAME;

			AIDataConversion.GetTrainingDataBatch (graph, examples, timeframe, out double[][] inputArrays, out double[][] outputArrays);
			LayerState[] inputs = AIDataConversion.ConvertToLayerStates (ref inputArrays);
			LayerState[] outputs = AIDataConversion.ConvertToLayerStates (ref outputArrays);

			for (int i = 0; i < examples; i++)
				totalLoss += network.CalculateLossOnInputs (inputs[i], outputs[i]);

			return totalLoss / examples;
		}

		public override ICopyable Copy () {
			return CopyAbstractValues (new AlgoAI ((DeepLearningNetwork)network.Copy ()));
		}
	}
}
