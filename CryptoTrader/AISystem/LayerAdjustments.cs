using System;

namespace CryptoTrader.AISystem {

	public class LayerAdjustments {

		public int InputSize { private set; get; }
		public int OutputSize { private set; get; }
		private LayerAdjustment adjustments;
		private int totalAdjustments;

		private LayerAdjustments (int inputSize, int outputSize, LayerAdjustment adjustments, int totalAdjustments) : this(inputSize, outputSize) {
			this.adjustments = adjustments;
			this.totalAdjustments = totalAdjustments;
		}

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
			adjustments.AddSelf (layerAdjustment);
			totalAdjustments++;
		}

		public void Clear () {
			adjustments = new LayerAdjustment (InputSize, OutputSize);
			totalAdjustments = 0;
		}

		public LayerAdjustment GetAverageAdjustment () {
			return adjustments / totalAdjustments;
		}

		public static LayerAdjustments operator + (LayerAdjustments left, LayerAdjustments right) {

			if (left.InputSize != right.InputSize || left.OutputSize != right.OutputSize)
				throw new ArgumentException ("The dimensions of the added layer must match.");

			LayerAdjustment sum = left.adjustments + right.adjustments;
			int total = left.totalAdjustments + right.totalAdjustments;
			return new LayerAdjustments (left.InputSize, left.OutputSize, sum, total);
		}

	}

}
