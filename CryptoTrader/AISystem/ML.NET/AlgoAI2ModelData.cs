using Microsoft.ML.Data;

namespace CryptoTrader.AISystem.ML.NET {
	public class AlgoAI2ModelData {

		[ColumnName ("PastPrices")]
		[LoadColumn (0, 9999)]
		[VectorType (10000)]
		public float[] PriceData { get; set; }

		[ColumnName ("Confidence")]
		[LoadColumn (10000)]
		public float HoldConfidence { get; set; }

	}
}
