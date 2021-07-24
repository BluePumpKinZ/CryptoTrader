﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
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

		public void Randomize () {

			RandomNumberGenerator rng = RandomNumberGenerator.Create ();
			for (int i = 0; i < weights.Length; i++) {
				int length = weights[i].Length;

				byte[] bytes = new byte[length * 8];
				rng.GetBytes (bytes);
				for (int j = 0; j < weights[i].Length; j++) {
					weights[i][j] = BitConverter.ToDouble (bytes, j * 8);
				}

			}
			rng.Dispose ();
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

			Array.ForEach (weights, (t) => Array.ForEach (t, (u) => bytes.AddRange(BitConverter.GetBytes (u))));

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
					sb.Append($" | {weights[i][j]}");
				}
			}
			return sb.ToString ();
		}

	}
}
