using CryptoTrader.Utils;
using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace CryptoTrader.AISystem {

	public class NetworkLayer : IStorable {

		public int InputSize { private set; get; }
		public int OutputSize { private set; get; }
		private double[] weights;
		private double[] biases;

		public NetworkLayer () {

		}

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

		public unsafe LayerState Iterate (LayerState inputState) {
			if (inputState.Size != InputSize)
				throw new ArgumentException ("Size of input layerstate does match input size.");

			if (!Avx.IsSupported) {

				LayerState outputState = new LayerState (OutputSize);
				for (int outputIndex = 0; outputIndex < OutputSize; outputIndex++) {
					double nodeSum = 0;
					for (int inputIndex = 0; inputIndex < InputSize; inputIndex++) {
						int weightIndex = GetWeightIndex (inputIndex, outputIndex);
						nodeSum += inputState.GetNode (inputIndex) * weights[weightIndex];
					}
					double nodeValue = MoreMath.Sigmoid (nodeSum + biases[outputIndex]);
					outputState[outputIndex] = nodeValue;
				}
				return outputState;

			} else {

				LayerState outputState = new LayerState (OutputSize);

				int newOutputSize = OutputSize & 0x7FFF_FFFC;
				int newInputSize = InputSize & 0x7FFF_FFFC;
				int weightSkip = InputSize - newInputSize;

				fixed (double* fixedInputPtr = inputState.data) {
					fixed (double* fixedOutputPtr = outputState.data) {
						fixed (double* fixedWeightPtr = weights) {
							fixed (double* fixedBiasPtr = biases) {

								double* inputPtr = fixedInputPtr;
								double* outputPtr = fixedOutputPtr;
								double* weightPtr = fixedWeightPtr;
								double* biasPtr = fixedBiasPtr;

								Vector256<double> inputVector, outputVector, weightsVector, biasVector, destVector;
								for (int outputIndex = 0; outputIndex < newOutputSize; outputIndex += 4) {

									outputVector = Avx.LoadVector256 (outputPtr);
									for (int inputIndex = 0; inputIndex < newInputSize; inputIndex += 4) {


										inputVector = Avx.LoadVector256 (inputPtr);
										weightsVector = Avx.LoadVector256 (weightPtr);

										destVector = Avx.Multiply (inputVector, weightsVector);

										outputVector = Avx.Add (outputVector, destVector);

										inputPtr += 4;
										weightPtr += 4;
									}
									biasVector = Avx.LoadVector256 (biasPtr);
									outputVector = Avx.Add (outputVector, biasVector);
									Avx.Store (outputPtr, outputVector);

									outputPtr += 4;
									biasPtr += 4;
									weightPtr += weightSkip;
									inputPtr = fixedInputPtr;
								}
							}
						}
					}

				}

				for (int outputIndex = 0; outputIndex < newOutputSize; outputIndex++) {
					double nodeSum = 0;
					for (int inputIndex = newInputSize; inputIndex < InputSize; inputIndex++) {
						int weightIndex = GetWeightIndex (inputIndex, outputIndex);
						nodeSum += inputState[inputIndex] * weights[weightIndex];
					}
					outputState[outputIndex] += nodeSum;
				}

				for (int outputIndex = newOutputSize; outputIndex < OutputSize; outputIndex++) {
					double nodeSum = 0;
					for (int inputIndex = 0; inputIndex < InputSize; inputIndex++) {
						int weightIndex = GetWeightIndex (inputIndex, outputIndex);
						nodeSum += inputState[inputIndex] * weights[weightIndex];
					}
					outputState[outputIndex] = nodeSum + biases[outputIndex];
				}

				for (int i = 0; i < OutputSize; i++) {
					outputState[i] = MoreMath.Sigmoid (outputState[i]);
				}

				return outputState;
			}
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

		public unsafe LayerAdjustment GetLayerAdjustment (LayerState inputs, LayerState outputs, LayerState outputDerivatives, out LayerState activationDerivatives, double step) {

			if (inputs.Size != InputSize)
				throw new ArgumentException ("The size of inputs must match the layer dimensions.");
			if (outputs.Size != OutputSize)
				throw new ArgumentException ("The size of outputs must match the layer dimensions.");
			// if (costs.Size != OutputSize)
			//	throw new ArgumentException ("The size of costs must match the layer dimensions.");

			LayerAdjustment layerAdjustment = new LayerAdjustment (InputSize, OutputSize);
			double[] activationDerivativesData = new double[InputSize];

			if (!Avx.IsSupported) {

				for (int outputIndex = 0; outputIndex < OutputSize; outputIndex++) {

					double output = outputs[outputIndex];
					double baseDerivative = MoreMath.Sigmoid_Derivative (output) * outputDerivatives[outputIndex];
					layerAdjustment.SetBias (outputIndex, baseDerivative * step);

					for (int inputIndex = 0; inputIndex < InputSize; inputIndex++) {

						int weightIndex = GetWeightIndex (inputIndex, outputIndex);

						double input = inputs[inputIndex];
						double weight = weights[weightIndex];

						double weightDerivative = input * baseDerivative;
						double activationDerivative = weight * baseDerivative;

						layerAdjustment.SetWeight (weightIndex, weightDerivative * step);

						activationDerivativesData[inputIndex] += activationDerivative;
					}
				}

			} else {

				int newOutputSize = OutputSize & 0x7FFF_FFFC;
				int newInputSize = InputSize & 0x7FFF_FFFC;
				int weightSkip = InputSize - newInputSize;

				double[] stepArray = new double[] { step, step, step, step };
				double[] baseDerivatives = new double[OutputSize];

				for (int i = 0; i < OutputSize; i++)
					baseDerivatives[i] = MoreMath.Sigmoid_Derivative (outputs[i]) * outputDerivatives[i];

				fixed (double* stepPtr = stepArray) {
					fixed (double* fixedWeightAdjustmentPtr = layerAdjustment.weightAdjustment) {
						fixed (double* fixedBiasAdjustmentPtr = layerAdjustment.biasAdjustment) {
							fixed (double* fixedInputPtr = inputs.data) {
								fixed (double* fixedOutputPtr = outputs.data) {
									fixed (double* fixedWeightPtr = weights) {
										fixed (double* fixedActivationsPtr = activationDerivativesData) {
											fixed (double* fixedBaseDerivativesPtr = baseDerivatives) {

												double* weightAdjustmentPtr = fixedWeightAdjustmentPtr;
												double* biasAdjustmentPtr = fixedBiasAdjustmentPtr;
												double* inputPtr = fixedInputPtr;
												double* outputPtr = fixedOutputPtr;
												double* weightPtr = fixedWeightPtr;
												double* activationPtr = fixedActivationsPtr;
												double* baseDerivatesPtr = fixedBaseDerivativesPtr;

												Vector256<double> stepVector = Avx.LoadVector256 (stepPtr);

												Vector256<double> weightAdjustmentVector;
												Vector256<double> biasAdjustmentVector;
												Vector256<double> inputVector;
												Vector256<double> outputVector;
												Vector256<double> weightVector;
												Vector256<double> activationVector;
												Vector256<double> baseDerivativesVector;

												for (int outputIndex = 0; outputIndex < newOutputSize; outputIndex += 4) {
													outputVector = Avx.LoadVector256 (outputPtr);
													activationVector = Avx.LoadVector256 (activationPtr);
													baseDerivativesVector = Avx.LoadVector256 (baseDerivatesPtr);

													biasAdjustmentVector = Avx.Multiply (baseDerivativesVector, stepVector);

													for (int inputIndex = 0; inputIndex < newInputSize; inputIndex += 4) {

														inputVector = Avx.LoadVector256 (inputPtr);
														weightVector = Avx.LoadVector256 (weightPtr);

														Vector256<double> weightDerivativeVector = Avx.Multiply (inputVector, baseDerivativesVector);
														activationVector = Avx.Add (activationVector, Avx.Multiply (weightVector, baseDerivativesVector));

														weightAdjustmentVector = Avx.Multiply (weightDerivativeVector, stepVector);

														Avx.Store (weightAdjustmentPtr, weightAdjustmentVector);
														Avx.Store (activationPtr, activationVector);
														weightAdjustmentPtr += 4;
														inputPtr += 4;
														weightPtr += 4;
														activationPtr += 4;
													}

													Avx.Store (biasAdjustmentPtr, biasAdjustmentVector);
													weightAdjustmentPtr += weightSkip;
													biasAdjustmentPtr += 4;
													inputPtr = fixedInputPtr;
													outputPtr += 4;
													weightPtr += weightSkip;
													activationPtr = fixedActivationsPtr;
												}
											}
										}
									}
								}
							}
						}
					}
				}

				for (int outputIndex = newOutputSize; outputIndex < OutputSize; outputIndex++) {
					double output = outputs[outputIndex];
					double baseDerivative = baseDerivatives[outputIndex];
					layerAdjustment.SetBias (outputIndex, baseDerivative * step);

					for (int inputIndex = 0; inputIndex < InputSize; inputIndex++) {

						int weightIndex = GetWeightIndex (inputIndex, outputIndex);
						double input = inputs[inputIndex];
						double weight = weights[weightIndex];
						double weightDerivative = input * baseDerivative;
						double activationDerivative = weight * baseDerivative;

						layerAdjustment.SetWeight (weightIndex, weightDerivative * step);
						activationDerivativesData[inputIndex] += activationDerivative;
					}
				}

				for (int outputIndex = newOutputSize; outputIndex < OutputSize; outputIndex++) {
					double output = outputs[outputIndex];
					double baseDerivative = baseDerivatives[outputIndex];

					for (int inputIndex = 0; inputIndex < InputSize; inputIndex++) {

						int weightIndex = GetWeightIndex (inputIndex, outputIndex);
						double input = inputs[inputIndex];
						double weight = weights[weightIndex];
						double weightDerivative = input * baseDerivative;
						double activationDerivative = weight * baseDerivative;

						layerAdjustment.SetWeight (weightIndex, weightDerivative * step);
						activationDerivativesData[inputIndex] += activationDerivative;
					}
				}
			}

			activationDerivatives = new LayerState (activationDerivativesData);
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

		public void LoadFromBytes (ref int index, byte[] data) {
			InputSize = BitConverter.ToInt32 (IStorable.GetDataRange (ref index, data));
			OutputSize = BitConverter.ToInt32 (IStorable.GetDataRange (ref index, data));

			byte[] weightBytes = IStorable.GetDataRange (ref index, data);
			byte[] biasBytes = IStorable.GetDataRange (ref index, data);
			weights = new double[InputSize * OutputSize];
			biases = new double[OutputSize];
			for (int i = 0; i < weights.Length; i++)
				weights[i] = BitConverter.ToDouble (weightBytes, i * 8);
			for (int i = 0; i < biases.Length; i++)
				biases[i] = BitConverter.ToDouble (biasBytes, i * 8);
		}

		public void SaveToBytes (ref List<byte> datalist) {
			IStorable.AddData (ref datalist, BitConverter.GetBytes (InputSize));
			IStorable.AddData (ref datalist, BitConverter.GetBytes (OutputSize));
			byte[] weightBytes = new byte[weights.Length * 8];
			byte[] biasBytes = new byte[biases.Length * 8];
			for (int i = 0; i < weights.Length; i++)
				BitConverter.GetBytes (weights[i]).CopyTo (weightBytes, i * 8);
			for (int i = 0; i < biases.Length; i++)
				BitConverter.GetBytes (biases[i]).CopyTo (biasBytes, i * 8);
			IStorable.AddData (ref datalist, weightBytes);
			IStorable.AddData (ref datalist, biasBytes);
		}
	}

}
