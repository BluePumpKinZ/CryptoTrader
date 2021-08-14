using System;

namespace CryptoTrader.AISystem {

	public class DeepLearningNetwork {

		public NetworkStructure Structure { private set; get; }
		private NetworkLayer[] networkLayers;

		public DeepLearningNetwork (NetworkStructure structure) {
			if (structure == null)
				throw new ArgumentException ("Structure cannot be null.");
			Structure = structure;
			networkLayers = Structure.GetNetworkLayers ();
		}

		public DeepLearningNetwork (NetworkStructure structure, double[][] weights, double[][] biases) {
			Structure = structure;
			networkLayers = Structure.GetNetworkLayers ();
			for (int i = 0; i < networkLayers.Length; i++) {
				networkLayers[i] = new NetworkLayer (Structure[i], Structure[i + 1], weights[i], biases[i]);
			}
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

		public void Train (LayerState[] inputs, LayerState[] desiredOutputs, double step) {
			step = -step;
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
			LayerAdjustments[] layerAdjustments = new LayerAdjustments[Structure.Size - 1];
			for (int i = 0; i < layerAdjustments.Length; i++)
				layerAdjustments[i] = new LayerAdjustments (Structure[i], Structure[i + 1]);
			for (int i = 0; i < inputs.Length; i++) {
				LayerAdjustment[] layerAdjustment = GetNetworkAdjustments (inputs[i], desiredOutputs[i], step);
				for (int j = 0; j < layerAdjustment.Length; j++) {
					layerAdjustments[j].AddAdjustment (layerAdjustment[j]);
				}
			}

			for (int i = 0; i < layerAdjustments.Length; i++) {
				networkLayers[i].ApplyLayerAdjustment (layerAdjustments[i].GetAverageAdjustment ());
			}
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

	}

}