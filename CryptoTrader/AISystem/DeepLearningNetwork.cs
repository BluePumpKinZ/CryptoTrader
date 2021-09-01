using CryptoTrader.Utils;
using System;
using System.Collections.Generic;
using System.Threading;

namespace CryptoTrader.AISystem {

	public class DeepLearningNetwork : IStorable, ICopyable {

		public NetworkStructure Structure { private set; get; }
		private NetworkLayer[] networkLayers;
		private LayerAdjustments[][] threadedAdjustments;

		public DeepLearningNetwork (NetworkStructure structure) {
			if (structure == null)
				throw new ArgumentException ("Structure cannot be null.");
			Structure = structure;
			networkLayers = Structure.GetNetworkLayers ();
		}

		public DeepLearningNetwork (NetworkStructure structure, NetworkLayer[] networkLayers) {
			Structure = structure;
			for (int i = 0; i < networkLayers.Length; i++) {
				if (networkLayers[i].InputSize != structure[i] || networkLayers[i].OutputSize != structure[i + 1])
					throw new ArgumentException ("The networklayers do not match the structure.");
			}
			this.networkLayers = networkLayers;
		}

		public void Randomize () {
			for (int i = 0; i < networkLayers.Length; i++) {
				networkLayers[i].Randomize ();
			}
		}

		public LayerState Iterate (LayerState layerState) {
			if (layerState.Size != Structure.GetSizeAtLayer (0))
				throw new ArgumentException ("Layerstate size does not match network structure.");
			LayerState interLayer = layerState;
			for (int layer = 0; layer < Structure.Size - 1; layer++) {
				interLayer = networkLayers[layer].Iterate (interLayer);
			}
			return interLayer;
		}

		private LayerAdjustment[] GetNetworkAdjustments (LayerState inputs, LayerState desiredOutputs, double step) {

			LayerAdjustment[] layerAdjustments = new LayerAdjustment[networkLayers.Length];
			LayerState[] layerStates = new LayerState[Structure.Size];
			layerStates[0] = inputs;

			for (int layer = 0; layer < networkLayers.Length; layer++)
				layerStates[layer + 1] = networkLayers[layer].Iterate (layerStates[layer]);

			LayerState outputs = layerStates[^1];
			LayerState interLayerDerivative = GetFinalLayerCostDerivative (outputs, desiredOutputs);

			for (int i = networkLayers.Length - 1; i >= 0; i--)
				layerAdjustments[i] = networkLayers[i].GetLayerAdjustment (layerStates[i], layerStates[i + 1], interLayerDerivative, out interLayerDerivative, step);

			return layerAdjustments;
		}

		private static LayerState GetFinalLayerCostDerivative (LayerState outputs, LayerState desiredOutputs) {
			if (outputs.Size != desiredOutputs.Size)
				throw new ArgumentException ("outputs and desiredoutputs should have the same size");
			LayerState output = new LayerState (outputs.Size);
			for (int i = 0; i < output.Size; i++) {
				output[i] = 2 * (outputs[i] - desiredOutputs[i]);
			}
			return output;
		}

		private void CheckTrainErrors (LayerState[] inputs, LayerState[] desiredOutputs) {
			if (inputs.Length != desiredOutputs.Length)
				throw new ArgumentException ("Amount of input arrays does not match the output.");

			int inputLayerSize = Structure[0];
			Array.ForEach (inputs, (t) => {
				if (t.Size != inputLayerSize)
					throw new ArgumentException ("One or more of the input layers do not match the network structure.");
			});
			int outputLayerSize = Structure[^1];
			Array.ForEach (desiredOutputs, (t) => {
				if (t.Size != outputLayerSize)
					throw new ArgumentException ("One or more of the output layers do not match the network structure.");
			});
		}

		private LayerAdjustments[] GetNetworkAdjustmentsBatch (LayerState[] inputs, LayerState[] desiredOutputs, double step) {
			LayerAdjustments[] layerAdjustments = new LayerAdjustments[Structure.Size - 1];
			for (int i = 0; i < layerAdjustments.Length; i++)
				layerAdjustments[i] = new LayerAdjustments (Structure[i], Structure[i + 1]);
			for (int i = 0; i < inputs.Length; i++) {
				LayerAdjustment[] layerAdjustment = GetNetworkAdjustments (inputs[i], desiredOutputs[i], step);
				for (int j = 0; j < layerAdjustment.Length; j++) {
					layerAdjustments[j].AddAdjustment (layerAdjustment[j]);
				}
			}
			return layerAdjustments;
		}

		private void ApplyNetworkAdjustments (LayerAdjustments[] networkAdjustments) {
			for (int i = 0; i < networkAdjustments.Length; i++) {
				networkLayers[i].ApplyLayerAdjustment (networkAdjustments[i].GetAverageAdjustment ());
			}
		}

		public void Train (LayerState[] inputs, LayerState[] desiredOutputs, double step) {

			CheckTrainErrors (inputs, desiredOutputs);

			LayerAdjustments[] networkAdjustments = GetNetworkAdjustmentsBatch (inputs, desiredOutputs, step);
			ApplyNetworkAdjustments (networkAdjustments);
		}

		public void TrainThreaded (LayerState[] inputs, LayerState[] desiredOutputs, double step, int threads) {

			if (threadedAdjustments != null)
				throw new InvalidOperationException ("Threaded training is already active. Cannot train the same network twice at the same time.");

			CheckTrainErrors (inputs, desiredOutputs);

			Batching.GetBatchSplits (inputs.Length, threads, out int[] markers, out int[] sizes);

			threadedAdjustments = new LayerAdjustments[threads][];

			for (int i = 0; i < threads; i++) {
				int j = i;
				AIProcessTaskScheduler.AddTask (() => {
					LayerState[] batchInputs = inputs.GetRange (markers[j], sizes[j]);
					LayerState[] batchDesiredOutputs = desiredOutputs.GetRange (markers[j], sizes[j]);
					threadedAdjustments[j] = GetNetworkAdjustmentsBatch (batchInputs, batchDesiredOutputs, step);
				});
			}
			bool allFinished;
			do {
				allFinished = true;
				for (int i = 0; i < threads; i++)
					if (threadedAdjustments[i] == null)
						allFinished = false;
				Thread.Sleep (1);
			} while (!allFinished);

			LayerAdjustments[] finalAdjustments = new LayerAdjustments[networkLayers.Length];
			for (int i = 0; i < finalAdjustments.Length; i++) {
				finalAdjustments[i] = new LayerAdjustments (threadedAdjustments[0][i].InputSize, threadedAdjustments[0][i].OutputSize);
				for (int j = 0; j < threads; j++)
					finalAdjustments[i].AddSelf (threadedAdjustments[j][i]);
			}
			ApplyNetworkAdjustments (finalAdjustments);
			threadedAdjustments = null;
		}

		public static double CalculateLossOnOutputs (LayerState outputs, LayerState desiredOutputs) {
			return outputs.CalculateCost (desiredOutputs);
		}

		public double CalculateLossOnInputs (LayerState inputs, LayerState desiredOutputs) {
			if (inputs.Size != Structure[0])
				throw new ArgumentException ("Inputs must match the structure of the network.");
			if (desiredOutputs.Size != Structure[^1])
				throw new ArgumentException ("DesiredOutputs must match the structure of the network.");
			LayerState outputs = Iterate (inputs);
			return CalculateLossOnOutputs (outputs, desiredOutputs);
		}

		public void LoadFromBytes (ref int index, byte[] data) {
			Structure = new NetworkStructure ();
			Structure.LoadFromBytes (ref index, data);
			int length = BitConverter.ToInt32 (IStorable.GetDataRange (ref index, data));
			networkLayers = new NetworkLayer[length];
			for (int i = 0; i < length; i++) {
				networkLayers[i] = new NetworkLayer ();
				networkLayers[i].LoadFromBytes (ref index, data);
			}
		}

		public void SaveToBytes (ref List<byte> datalist) {
			Structure.SaveToBytes (ref datalist);
			IStorable.AddData (ref datalist, BitConverter.GetBytes (networkLayers.Length));
			for (int i = 0; i < networkLayers.Length; i++)
				networkLayers[i].SaveToBytes (ref datalist);
		}

		public ICopyable Copy () {
			return new DeepLearningNetwork ((NetworkStructure)Structure.Copy (), networkLayers.CopyMembers ());
		}
	}

}