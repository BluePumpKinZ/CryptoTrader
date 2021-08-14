using System;
using System.Collections.Generic;

namespace CryptoTrader.AISystem {

	public class LayerAdjustments {

		public int InputSize { private set; get; }
		public int OutputSize { private set; get; }
		private readonly List<LayerAdjustment> adjustments;

		public LayerAdjustments (int inputSize, int outputSize) {
			if (inputSize <= 0)
				throw new ArgumentException ("Input size must be more than 0.");
			if (outputSize <= 0)
				throw new ArgumentException ("Output size must be more than 0.");

			InputSize = inputSize;
			OutputSize = outputSize;

			adjustments = new List<LayerAdjustment> ();
		}

		public void AddAdjustment (LayerAdjustment layerAdjustment) {
			if (layerAdjustment.InputSize != InputSize || layerAdjustment.OutputSize != OutputSize)
				throw new ArgumentException ("The dimensions of the added layer must match.");

			adjustments.Add (layerAdjustment);
		}

		public void Clear () {
			adjustments.Clear ();
		}

		public LayerAdjustment GetAverageAdjustment () {
			LayerAdjustment layerAdjustment = new LayerAdjustment (InputSize, OutputSize);
			for (int weightIndex = 0; weightIndex < layerAdjustment.WeightSize; weightIndex++) {
				double nodeSum = 0;
				for (int i = 0; i < adjustments.Count; i++) {
					nodeSum += adjustments[i].GetWeight (weightIndex);
				}
				double nodeAverage = nodeSum / adjustments.Count;
				layerAdjustment.SetWeight (weightIndex, nodeAverage);
			}
			for (int biasIndex = 0; biasIndex < layerAdjustment.BiasSize; biasIndex++) {
				double nodeSum = 0;
				for (int i = 0; i < adjustments.Count; i++) {
					nodeSum += adjustments[i].GetBias (biasIndex);
				}
				double nodeAverage = nodeSum / adjustments.Count;
				layerAdjustment.SetBias (biasIndex, nodeAverage);
			}
			return layerAdjustment;
		}

	}

}
