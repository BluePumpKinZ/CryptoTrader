using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

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
			int weightLength = output.WeightSize;
			int biasLength = output.BiasSize;
			for (int i = 0; i < weightLength; i++)
				output.weightAdjustment[i] = left.weightAdjustment[i] + right.weightAdjustment[i];
			for (int i = 0; i < biasLength; i++)
				output.biasAdjustment[i] = left.biasAdjustment[i] + right.biasAdjustment[i];
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

		public unsafe void AddSelf (LayerAdjustment other) {

			if (WeightSize != other.WeightSize || BiasSize != other.BiasSize)
				throw new ArgumentException ("The dimensions of the added layer must match.");

			int weightSize = WeightSize;
			int biasSize = BiasSize;

			if (!Avx.IsSupported) {
				for (int i = 0; i < weightSize; i++)
					weightAdjustment[i] += other.weightAdjustment[i];
				for (int i = 0; i < biasSize; i++)
					biasAdjustment[i] += other.biasAdjustment[i];
			} else {

				int newWeightSize = weightSize & 0x7FFF_FFFC;
				int newBiasSize = biasSize & 0x7FFF_FFFC;

				fixed (double* fixedSelfWeightPtr = weightAdjustment) {
					fixed (double* fixedOtherWeightPtr = other.weightAdjustment) {
						fixed (double* fixedSelfBiasPtr = biasAdjustment) {
							fixed (double* fixedOtherBiasPtr = other.biasAdjustment) {
								
								double* selfWeightPtr = fixedSelfWeightPtr;
								double* otherWeightPtr = fixedOtherWeightPtr;
								double* selfBiasPtr = fixedSelfBiasPtr;
								double* otherBiasPtr = fixedOtherBiasPtr;

								Vector256<double> selfWeightVector, otherWeightVector, selfBiasVector, otherBiasVector;

								for (int i = 0; i < newWeightSize; i += 4) {
									selfWeightVector = Avx.LoadVector256 (selfWeightPtr);
									otherWeightVector = Avx.LoadVector256 (otherWeightPtr);
									selfWeightVector = Avx.Add (selfWeightVector, otherWeightVector);
									Avx.Store (selfWeightPtr, selfWeightVector);
									selfWeightPtr += 4;
									otherWeightPtr += 4;
								}

								for (int i = 0; i < newBiasSize; i += 4) {
									selfBiasVector = Avx.LoadVector256 (selfBiasPtr);
									otherBiasVector = Avx.LoadVector256 (otherBiasPtr);
									selfBiasVector = Avx.Add (selfBiasVector, otherBiasVector);
									Avx.Store (selfBiasPtr, selfBiasVector);
									selfBiasPtr += 4;
									otherBiasPtr += 4;
								}

								for (int i = newWeightSize; i < weightSize; i++)
									weightAdjustment[i] += other.weightAdjustment[i];
								for (int i = newBiasSize; i < biasSize; i++)
									biasAdjustment[i] += other.biasAdjustment[i];
							}
						}
					}
				}
			}
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
