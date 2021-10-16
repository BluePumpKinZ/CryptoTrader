using Microsoft.ML.Data;

namespace CryptoTrader.AISystem.ML.NET {

	public class AlgoAI2ModelOutput {

		[ColumnName ("HoldConfidence")]
		public float HoldConfidence { get; set; }

	}

}