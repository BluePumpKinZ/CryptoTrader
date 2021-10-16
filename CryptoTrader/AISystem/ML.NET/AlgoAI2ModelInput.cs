using Microsoft.ML.Data;
using CryptoTrader.Algorithms;
using System;

namespace CryptoTrader.AISystem.ML.NET {
	public class AlgoAI2ModelInput {

		[ColumnName ("PastPrices")]
		[LoadColumn (0, AIDataConversion.INPUT_LAYER_SAMPLES - 1)]
		[VectorType (AIDataConversion.INPUT_LAYER_SAMPLES)]
		public float[] PriceData { get; set; }

		[ColumnName ("HoldConfidence")]
		[LoadColumn (AIDataConversion.INPUT_LAYER_SAMPLES)]
		public float HoldConfidence { get; set; }

		public static AlgoAI2ModelInput FromDoubleArray (double[] values) {
			if (values is null || values.Length != AIDataConversion.INPUT_LAYER_SAMPLES)
				throw new ArgumentException ($"Values array cannot be null and must be have a size of {AIDataConversion.INPUT_LAYER_SAMPLES}.", nameof (values));
			AlgoAI2ModelInput toReturn = new AlgoAI2ModelInput ();
			toReturn.PriceData = new float[values.Length];
			for (int i = 0; i < values.Length; i++) {
				toReturn.PriceData[i] = (float)values[i];
			}
			return toReturn;
		}

	}
}
