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
					weights[i][j] = random.NextDouble () * 2 - 1;
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

		public static DeepLearningNetwork Load (string path) {

			byte[] bytes = File.ReadAllBytes (path);

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

		public void Save (string path) {

			List<byte> bytes = new List<byte> ();
			bytes.AddRange (BitConverter.GetBytes (structure.Length));
			Array.ForEach (structure, (t) => bytes.AddRange (BitConverter.GetBytes (t)));

			Array.ForEach (weights, (t) => Array.ForEach (t, (u) => bytes.AddRange (BitConverter.GetBytes (u))));

			File.WriteAllBytes (path, bytes.ToArray ());
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
