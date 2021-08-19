using System;

namespace CryptoTrader.AISystem {

	public class LayerAdjustment {

		public int InputSize { private set; get; }
		public int OutputSize { private set; get; }
		public int WeightSize { get { return InputSize * OutputSize; } }
		public int BiasSize { get { return OutputSize; } }
		internal protected double[] weightAdjustment;
		internal protected double[] biasAdjustment;

		public LayerAdjustment (int inputSize, int outputSize) {
			if (inputSize <= 0)
				throw new ArgumentException ("Input size must be more than 0.");
			if (outputSize <= 0)
				throw new ArgumentException ("Output size must be more than 0.");

			InputSize = inputSize;
			OutputSize = outputSize;

			weightAdjustment = new double[inputSize * outputSize];
			biasAdjustment = new double[outputSize];
		}

		public static LayerAdjustment operator + (LayerAdjustment left, LayerAdjustment right) {
			if (left.WeightSize != right.WeightSize || left.BiasSize != right.BiasSize)
				throw new ArgumentException ("Both left and right operands must have the same size.");
			LayerAdjustment output = new LayerAdjustment (left.InputSize, left.OutputSize);
			for (int i = 0; i < output.WeightSize; i++)
				output.SetWeight (i, left.GetWeight (i) + right.GetWeight (i));
			for (int i = 0; i < output.BiasSize; i++)
				output.SetBias (i, left.GetBias (i) + right.GetBias (i));
			return output;
		}

		public static LayerAdjustment operator / (LayerAdjustment left, double right) {
			LayerAdjustment output = new LayerAdjustment (left.InputSize, left.OutputSize);
			for (int i = 0; i < output.WeightSize; i++)
				output.SetWeight (i, left.GetWeight (i) / right);
			for (int i = 0; i < output.BiasSize; i++)
				output.SetBias (i, left.GetBias (i) / right);
			return output;
		}

		public double GetWeight (int index) {
			return weightAdjustment[index];
		}

		public void SetWeight (int index, double value) {
			weightAdjustment[index] = value;
		}

		public double GetBias (int index) {
			return biasAdjustment[index];
		}

		public void SetBias (int index, double value) {
			biasAdjustment[index] = value;
		}

	}

}
