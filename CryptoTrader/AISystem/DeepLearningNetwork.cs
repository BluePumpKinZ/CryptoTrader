using CryptoTrader.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CryptoTrader.AISystem {

	public class DeepLearningNetwork {

		private int[] structure;
		private double[][] weights;

		public DeepLearningNetwork (int[] structure) {

			if (structure == null || structure.Length < 2)
				throw new ArgumentException ("The structure cannot be null or be smaller than 2 layers.");

			Array.ForEach (structure, (t) => {
				if (t <= 0)
					throw new ArgumentException ("The structure supplied has negative values. Negative layer sizes are not possible.");
			});

			this.structure = structure;

			weights = GenerateEmptyWeights (this.structure);
		}

		private DeepLearningNetwork (int[] structure, double[][] weights) : this (structure) {
			int[] correctDimensions = GetWeightArrayDimensions (structure);
			if (correctDimensions.Length != weights.Length)
				throw new ArgumentException ("The weights do not fit the structure.");
			for (int i = 0; i < weights.Length; i++) {
				if (correctDimensions[i] != weights[i].Length)
					throw new ArgumentException ("The weights do not fit the structure.");
			}
			this.weights = weights;
		}

		private static int[] GetWeightArrayDimensions (int[] structure) {
			int[] dimensions = new int[structure.Length - 1];

			for (int i = 0; i < dimensions.Length; i++) {
				dimensions[i] = structure[i] * structure[i + 1];
			}
			return dimensions;
		}

		private static double[][] GenerateEmptyWeights (int[] structure) {
			int[] dimensions = GetWeightArrayDimensions (structure);
			double[][] weights = new double[dimensions.Length][];
			for (int i = 0; i < dimensions.Length; i++) {
				weights[i] = new double[structure[i] * structure[i + 1]];
			}
			return weights;
		}

		public void RandomizeWeights () {

			Random random = new Random ();
			for (int i = 0; i < weights.Length; i++) {
				for (int j = 0; j < weights[i].Length; j++) {
					weights[i][j] = 2 * random.NextDouble () - 1;
				}

			}
		}

		private void IterateLayer (ref double[] input, ref double[] output, int layer) {

			if (layer > structure.Length - 1 || input.Length != structure[layer] || output.Length != structure[layer + 1])
				throw new ArgumentException ("The supplied input data is not valid.");
			
			for (int i = 0; i < output.Length; i++) {

				double total = 0;
				for (int j = 0; j < input.Length; j++) {
					int weightIndex = i * input.Length + j;
					total += weights[layer][weightIndex] * input[j];
				}
				output[i] = MoreMath.Sigmoid (total);
			}
		}

		public double[] Iterate (double[] input) {
			double[] layerIn = input;
			double[] layerOut = null;

			for (int i = 0; i < weights.Length; i++) {
				layerOut = new double[structure[i + 1]];
				IterateLayer (ref layerIn, ref layerOut, i);
				layerIn = layerOut;
			}

			return layerOut;
		}

		public static double CalculateLoss (ref double[] output, ref double[] desiredOutput) {
			if (output.Length != desiredOutput.Length)
				throw new ArgumentException ("Output and desiredOutput must be the same size.");

			double totalLoss = 0;
			for (int i = 0; i < output.Length; i++) {
				totalLoss += MoreMath.Square (output[i] - desiredOutput[i]);
			}
			return totalLoss;
		}

		private void ApplyWeightAdjustmentsToModel (double[][] adjustments, double step) {
			ArgumentException err = new ArgumentException ("The given adjustments do not match the dimensions of the weights of this model.");
			if (adjustments.Length != weights.Length)
				throw err;
			for (int i = 0; i < adjustments.Length; i++) {
				if (adjustments[i].Length != weights[i].Length)
					throw err;
			}

			for (int i = 0; i < adjustments.Length; i++) {
				for (int j = 0; j < adjustments[i].Length; j++)
					weights[i][j] = MoreMath.Lerp (weights[i][j], adjustments[i][j], step);
			}
		}

		private double[] CalculateLayerWeightAdjustment (double[] layerIn, double[] requestedBias, out double[] nextBias, int layer) {
			double[] weights = this.weights[layer];
			double[] newWeights = new double[weights.Length];
			double[] inputWeightAverage = new double[layerIn.Length];

			int inputCount = structure[layer];
			// Loop through output layer
			for (int i = 0; i < structure[layer + 1]; i++) {
				// Loop through input layer
				for (int j = 0; j < inputCount; j++) {
					int weightIndex = i * inputCount + j;
					double output = layerIn[j] * weights[weightIndex];
					double requestedOutput = output + requestedBias[i];
					// Maybe use MoreMath.MaxAmp
					double requestedWeight = requestedOutput / layerIn[j];

					// Nancheck to prevent nan propagation
					if (requestedWeight == double.NaN || !double.IsFinite(requestedWeight))
						requestedWeight = weights[weightIndex];

					double bias = requestedWeight - weights[weightIndex];
					newWeights[weightIndex] = requestedWeight;
					// newWeights[weightIndex] = bias;
					inputWeightAverage[j] += bias;
				}
			}

			for (int i = 0; i < inputWeightAverage.Length; i++)
				inputWeightAverage[i] /= inputCount;

			nextBias = inputWeightAverage;
			return newWeights;
		}

		private void CalculateWeightAdjustments (ref double[] input, ref double[] desiredOutput, ref double[][] addedWeightAdjustment) {

			// Fill addedWeightAdjustment with zeros
			for (int i = 0; i < addedWeightAdjustment.Length; i++) {
				if (addedWeightAdjustment[i] == null)
					continue;
				for (int j = 0; j < addedWeightAdjustment[i].Length; j++) {
					addedWeightAdjustment[i][j] = 0;
				}
			}

			List<double[]> interLayers = new List<double[]> ();
			double[] inLayer = input;
			double[] outLayer = null;
			int layers = structure.Length - 1;
			for (int i = 0; i < layers; i++) {
				outLayer = new double[structure[i + 1]];
				IterateLayer (ref inLayer, ref outLayer, i);
				interLayers.Add (inLayer);
				inLayer = outLayer;
			}
			interLayers.Add (outLayer);

			// Calculate initial bias
			double[] changeBias = new double[desiredOutput.Length];
			for (int i = 0; i < changeBias.Length; i++) {
				changeBias[i] = desiredOutput[i] - interLayers[^1][i];
			}

			double[] bias = changeBias;
			for (int i = layers - 1; i >= 0; i--) {

				double[] weightAdjustment = CalculateLayerWeightAdjustment (interLayers[i], bias, out double[] nextBias, i);
				bias = nextBias;

				addedWeightAdjustment[i] = weightAdjustment;
			}
		}

		public void Train (ref double[][] input, ref double[][] desiredOutput, double step) {
			if (input.Length != desiredOutput.Length)
				throw new ArgumentException ("Amount of input arrays does not match the output.");

			int inputLayerSize = structure[0];
			Array.ForEach (input, (t) => {
				if (t.Length != inputLayerSize)
					throw new ArgumentException ("One or more of the input arrays do not match the network structure.");
			});
			int outputLayerSize = structure[^1];
			Array.ForEach (desiredOutput, (t) => {
				if (t.Length != outputLayerSize)
					throw new ArgumentException ("One or more of the output arrays do not match the network structure.");
			});

			// Gather all adjustment values
			double[][] weightAdjustments = new double[weights.Length][];
			double[][] addedWeightAdjustment = new double[weights.Length][];

			for (int i = 0; i < weightAdjustments.Length; i++) {
				weightAdjustments[i] = new double[structure[i] * structure[i + 1]];
			}

			for (int i = 0; i < input.Length; i++) {
				CalculateWeightAdjustments (ref input[i], ref desiredOutput[i], ref addedWeightAdjustment);
				for (int j = 0; j < weightAdjustments.Length; j++) {
					for (int k = 0; k < weightAdjustments[j].Length; k++) {
						weightAdjustments[j][k] += addedWeightAdjustment[j][k];
					}
				}
			}

			// Divide adjustment values for average
			int amountOfAdjustments = input.Length;
			for (int i = 0; i < weightAdjustments.Length; i++) {
				for (int j = 0; j < weightAdjustments[i].Length; j++) {
					weightAdjustments[i][j] /= amountOfAdjustments;
				}
			}
			addedWeightAdjustment = null;

			ApplyWeightAdjustmentsToModel (weightAdjustments, step);
		}

		public static DeepLearningNetwork Load (byte[] bytes) {

			int index = 0;
			int length = BitConverter.ToInt32 (bytes, 0);
			index += sizeof (int);

			int[] structure = new int[length];
			for (int i = 0; i < length; i++) {
				structure[i] = BitConverter.ToInt32 (bytes, index);
				index += sizeof (int);
			}

			double[][] weights = GenerateEmptyWeights (structure);
			for (int i = 0; i < weights.Length; i++) {
				for (int j = 0; j < weights[i].Length; j++) {
					weights[i][j] = BitConverter.ToDouble (bytes, index);
					index += sizeof (double);
				}
			}

			return new DeepLearningNetwork (structure, weights);
		}

		public byte[] Save () {

			List<byte> bytes = new List<byte> ();
			bytes.AddRange (BitConverter.GetBytes (structure.Length));
			Array.ForEach (structure, (t) => bytes.AddRange (BitConverter.GetBytes (t)));

			Array.ForEach (weights, (t) => Array.ForEach (t, (u) => bytes.AddRange (BitConverter.GetBytes (u))));
			return bytes.ToArray ();
		}

		public override string ToString () {
			StringBuilder sb = new StringBuilder ();
			sb.Append ("Structure: ");
			sb.Append (structure[0]);
			for (int i = 1; i < structure.Length; i++) {
				sb.Append ($" | {structure[i]}");
			}
			sb.Append ("\nWeights");
			for (int i = 0; i < weights.Length; i++) {
				sb.Append ($"\nLayer {i} {weights[i][0]}");
				for (int j = 1; j < weights[i].Length; j++) {
					sb.Append ($" | {weights[i][j]}");
				}
			}
			return sb.ToString ();
		}

	}
}
