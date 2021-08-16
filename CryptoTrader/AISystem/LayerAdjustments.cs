using System;
using System.Collections.Generic;

namespace CryptoTrader.AISystem {

	public class LayerAdjustments {

		public int InputSize { private set; get; }
		public int OutputSize { private set; get; }
		private LayerAdjustment adjustments;
		private int totalAdjustments;

		public LayerAdjustments (int inputSize, int outputSize) {
			if (inputSize <= 0)
				throw new ArgumentException ("Input size must be more than 0.");
			if (outputSize <= 0)
				throw new ArgumentException ("Output size must be more than 0.");

			InputSize = inputSize;
			OutputSize = outputSize;

			Clear ();
		}

		public void AddAdjustment (LayerAdjustment layerAdjustment) {
			if (layerAdjustment.InputSize != InputSize || layerAdjustment.OutputSize != OutputSize)
				throw new ArgumentException ("The dimensions of the added layer must match.");

			adjustments += layerAdjustment;
			totalAdjustments++;
		}

		public void Clear () {
			adjustments = new LayerAdjustment (InputSize, OutputSize);
			totalAdjustments = 0;
		}

		public LayerAdjustment GetAverageAdjustment () {
			return adjustments / totalAdjustments;
		}

		public static LayerAdjustments Combine (LayerAdjustments[] layerAdjustments) {
			int inputSize = layerAdjustments[0].InputSize;
			int outputSize = layerAdjustments[0].OutputSize;

			Array.ForEach (layerAdjustments, t => {
				if (t.InputSize != inputSize || t.OutputSize != outputSize)
					throw new ArgumentException ("The dimensions of the layers must match.");
			});

			LayerAdjustment total = new LayerAdjustment(inputSize, outputSize);
			Array.ForEach (layerAdjustments, t => total += t.GetAverageAdjustment ());
			total /= layerAdjustments.Length;
			LayerAdjustments wrapper = new LayerAdjustments (inputSize, outputSize);
			wrapper.AddAdjustment (total);
			return wrapper;
		}

	}

}
