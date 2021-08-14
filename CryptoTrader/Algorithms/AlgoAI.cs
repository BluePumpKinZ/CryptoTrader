using CryptoTrader.AISystem;
using CryptoTrader.Algorithms.Orders;
using CryptoTrader.NicehashAPI;
using CryptoTrader.NicehashAPI.JSONObjects;
using CryptoTrader.NicehashAPI.Utils;
using CryptoTrader.Utils;
using NLog.Targets;
using System;
using System.Collections.Generic;

namespace CryptoTrader.Algorithms {

	public class AlgoAI : Algorithm {

		public DeepLearningNetwork network;
		private double totalBalancesRatioAssinged;
		public double TotalBalancesRatioAssinged { private set { totalBalancesRatioAssinged = MoreMath.Clamp01 (value); } get { return totalBalancesRatioAssinged; } }

		public AlgoAI () {
			NetworkStructure structure = new NetworkStructure (new int[] { AIDataConversion.INPUT_LAYER_SAMPLES, 100, 16, 1 });
			network = new DeepLearningNetwork (structure);
			network.Randomize ();
			totalBalancesRatioAssinged = 0;
		}

		/* 
		 * double	| totalBalancesPercentAssinged
		 * int		| algoLength
		 * byte[]	| algoBytes
		 */

		/*public override void LoadFromBytes (byte[] bytes) {
			if (bytes.Length == 0)
				return;
			int byteIndex = 0;
			totalBalancesRatioAssinged = BitConverter.ToDouble (bytes, byteIndex);
			byteIndex += 8;
			int algoLength = BitConverter.ToInt32 (bytes, byteIndex);
			byteIndex += 4;
			network = DeepLearningNetwork.Load (bytes.GetRange (byteIndex, algoLength));
		}

		public override byte[] SaveToBytes () {
			List<byte> bytes = new List<byte> ();
			bytes.AddRange (BitConverter.GetBytes (totalBalancesRatioAssinged));
			byte[] algoBytes = network.Save ();
			bytes.AddRange (BitConverter.GetBytes (algoBytes.Length));
			bytes.AddRange (algoBytes);
			return bytes.ToArray ();
		}*/

		public void TrainNetwork (LayerState[] input, LayerState[] desiredOutput, double step) {
			network.Train (input, desiredOutput, step);
		}

		public double GetLoss (LayerState input, LayerState desiredOutput) {
			return network.CalculateLossOnInputs (input, desiredOutput);
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
			double totalAvailableBTC = totalBTC * totalBalancesRatioAssinged;
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

	}
}
