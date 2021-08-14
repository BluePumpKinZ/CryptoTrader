using CryptoTrader.Utils;
using System;

namespace CryptoTrader.AISystem {

	public class NetworkLayer {

		public int InputSize { private set; get; }
		public int OutputSize { private set; get; }
		private double[] weights;
		private double[] biases;

		public NetworkLayer (int inputSize, int outputSize) {
			if (inputSize <= 0)
				throw new ArgumentException ("Input size must be more than 0.");
			if (outputSize <= 0)
				throw new ArgumentException ("Output size must be more than 0.");

			InputSize = inputSize;
			OutputSize = outputSize;

			weights = new double[inputSize * outputSize];
			biases = new double[outputSize];
		}

		public NetworkLayer (int inputSize, int outputSize, double[] weights, double[] biases) : this (inputSize, outputSize) {
			this.weights = weights;
			this.biases = biases;
		}

		public void Randomize () {
			double[] stdRandom = MoreMath.GetStandardDistribution (weights.Length);
			double modifier = Math.Sqrt (InputSize);
			for (int i = 0; i < stdRandom.Length; i++) {
				stdRandom[i] /= modifier;
			}
			weights = stdRandom;
			biases = MoreMath.GetStandardDistribution (biases.Length);
		}

		public LayerState Iterate (LayerState inputState) {
			if (inputState.Size != InputSize)
				throw new ArgumentException ("Size of input layerstate does match input size.");

			LayerState outputState = new LayerState (OutputSize);
			for (int outputIndex = 0; outputIndex < OutputSize; outputIndex++) {
				double nodeSum = 0;
				for (int inputIndex = 0; inputIndex < InputSize; inputIndex++) {
					int weightIndex = GetWeightIndex (inputIndex, outputIndex);
					nodeSum += inputState.GetNode (inputIndex) * weights[weightIndex];
				}
				double nodeValue = MoreMath.Sigmoid (nodeSum + biases[outputIndex]);
				outputState.SetNode (outputIndex, nodeValue);
			}
			return outputState;
		}

		public void ApplyLayerAdjustment (LayerAdjustment layerAdjustment) {
			if (layerAdjustment.WeightSize != weights.Length || layerAdjustment.BiasSize != biases.Length)
				throw new ArgumentException ("The dimensions of the given layer adjustment do not fit the networklayer.");

			for (int i = 0; i < layerAdjustment.WeightSize; i++) {
				weights[i] += layerAdjustment.GetWeight (i);
			}
			for (int i = 0; i < layerAdjustment.BiasSize; i++) {
				biases[i] += layerAdjustment.GetBias (i);
			}
		}

		public LayerAdjustment GetLayerAdjustment (LayerState inputs, LayerState outputs, LayerState outputDerivatives, out LayerState activationDerivatives, double step) {

			if (inputs.Size != InputSize)
				throw new ArgumentException ("The size of inputs must match the layer dimensions.");
			if (outputs.Size != OutputSize)
				throw new ArgumentException ("The size of outputs must match the layer dimensions.");
			// if (costs.Size != OutputSize)
			//	throw new ArgumentException ("The size of costs must match the layer dimensions.");

			LayerAdjustment layerAdjustment = new LayerAdjustment (InputSize, OutputSize);
			double[] activationDerivativesSum = new double[InputSize];

			for (int outputIndex = 0; outputIndex < OutputSize; outputIndex++) {

				for (int inputIndex = 0; inputIndex < InputSize; inputIndex++) {

					int weightIndex = GetWeightIndex (inputIndex, outputIndex);

					double input = inputs[inputIndex];
					double weight = weights[weightIndex];
					double output = outputs[outputIndex];
					double baseDerivative = MoreMath.Sigmoid_Derivative (output) * outputDerivatives[outputIndex];
					double weightDerivative = input * baseDerivative;
					double biasDerivative = baseDerivative;
					double activationDerivative = weight * baseDerivative;

					layerAdjustment.SetWeight (weightIndex, weightDerivative * step);


					layerAdjustment.SetBias (outputIndex, biasDerivative * step);
					activationDerivativesSum[inputIndex] += activationDerivative;
				}
			}

			activationDerivatives = new LayerState (InputSize);
			for (int i = 0; i < activationDerivatives.Size; i++)
				activationDerivatives[i] = activationDerivativesSum[i]; // Get Average
			return layerAdjustment;
		}

		private int GetWeightIndex (int inputIndex, int outputIndex) {
			if (inputIndex < 0 || inputIndex >= InputSize)
				throw new ArgumentException ("InputIndex must be positive and less than size of the collection");
			return inputIndex + InputSize * outputIndex;
		}

		public static double GetInfluence (double value, double max) {
			// https://www.desmos.com/calculator/yebysmqg0d?lang=nl
			return (value >= 0) ? (max / (1 + (max - 1) * value)) : (-max / (1 + (max - 1) * -value));
		}

		public static double GetInfluenceAbs (double value, double max) {
			return max / (1 + (max - 1) * Math.Abs (value));
		}

	}

}
