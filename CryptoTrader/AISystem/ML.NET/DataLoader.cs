using System;
using System.Linq;

namespace CryptoTrader.AISystem.ML.NET {

	public class DataLoader {

		public static AlgoAI2ModelData[] ConvertToModelData(double[][] inputs, double[][] outputs) {

			if (inputs.Length != outputs.Length)
				throw new ArgumentException ("Argument dimensions do not match.");

			int length = outputs.Length;

			AlgoAI2ModelData[] output = new AlgoAI2ModelData[length];
			for (int i = 0; i < length; i++) {
				AlgoAI2ModelData dataItem = new AlgoAI2ModelData ();
				dataItem.PriceData = inputs[i].Cast<float> ().ToArray ();
				dataItem.HoldConfidence = (float)outputs[i][0];
			}
			return output;

		}

	}

}