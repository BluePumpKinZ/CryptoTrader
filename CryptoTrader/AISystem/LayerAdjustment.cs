using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoTrader.AISystem {

	public class LayerAdjustment {

		public int InputSize { private set; get; }
		public int OutputSize { private set; get; }
		public int WeightSize { get { return InputSize * OutputSize; } }
		public int BiasSize { get { return OutputSize; } }
		private double[] weightAdjustment;
		private double[] biasAdjustment;

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
