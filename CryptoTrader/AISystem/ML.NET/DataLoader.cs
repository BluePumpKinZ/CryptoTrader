using CryptoTrader.NicehashAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CryptoTrader.AISystem.ML.NET {

	public class DataLoader {

		public static AlgoAI2ModelInput[] ConvertToModelData(double[][] inputs, double[][] outputs) {

			if (inputs.Length != outputs.Length)
				throw new ArgumentException ("Argument dimensions do not match.");

			int length = outputs.Length;

			AlgoAI2ModelInput[] output = new AlgoAI2ModelInput[length];
			for (int i = 0; i < length; i++) {
				AlgoAI2ModelInput dataItem = new AlgoAI2ModelInput ();
				dataItem.PriceData = new float[inputs[i].Length];
				for (int j = 0; j < inputs[i].Length; j++)
					dataItem.PriceData[j] = (float)inputs[i][j];
				dataItem.HoldConfidence = (float)outputs[i][0];
				output[i] = dataItem;
			}
			return output;

		}

		public static void LoadAllDataToDataSet (PriceGraph[] graphs, string path, long fileSize = 0) {
			if (graphs is null) throw new ArgumentException ($"{nameof(graphs)} cannot be null.");
			if (graphs.Length == 0) throw new ArgumentException ($"{nameof (graphs)} cannot be empty.");

			if (File.Exists (path))
				File.Delete (path);

			int amountOfLines = (int)(fileSize / 52000);
			int[] distributions = new int[graphs.Length];
			int[] totalPossible = new int[graphs.Length];

			if (fileSize == 0)
				goto SkipDistribution;

			for (int i = 0; i < graphs.Length; i++) {
				long startTime = graphs[i].GetStartTime ();
				const long minTimeFrame = AIDataConversion.TIMEFRAME;
				int lastJ = 0;
				for (int j = 0; j < graphs[i].GetLength (); j++) {
					lastJ = j;
					if (graphs[i].GetTimeByIndex (j) > startTime + minTimeFrame)
						break;
				}
				totalPossible[i] = graphs[i].GetLength () - lastJ;
			}

			int total = totalPossible.Sum ();
			double factor = (double)total / amountOfLines;
			for (int i = 0; i < graphs.Length; i++) {
				distributions[i] = (int)(totalPossible[i] / factor);
			}

			SkipDistribution:

			StreamWriter writer = new StreamWriter (path);

			StringBuilder header = new StringBuilder ();
			for (int i = 0; i < AIDataConversion.INPUT_LAYER_SAMPLES; i++) {
				header.Append ("PastPrices");
				header.Append (";");
			}
			header.Append ("Confidence");

			writer.WriteLine (header.ToString ());

			for (int i = 0; i < graphs.Length; i++) {
				double[][] inputs, outputs;
				if (fileSize == 0) {
					AIDataConversion.GetAllTrainingData (graphs[i], AIDataConversion.TIMEFRAME, out inputs, out outputs);
				} else {
					AIDataConversion.GetTrainingDataBatchThreaded (graphs[i], distributions[i], AIProcessTaskScheduler.ThreadCount, AIDataConversion.TIMEFRAME, out inputs, out outputs);
				}

				AlgoAI2ModelInput[] graphData = ConvertToModelData (inputs, outputs);
				StringBuilder graphSamples = new StringBuilder ();

				for (int j = 0; j < graphData.Length; j++) {

					for (int k = 0; k < graphData[j].PriceData.Length; k++) {
						graphSamples.Append (graphData[j].PriceData[k]);
						graphSamples.Append (';');
					}
					graphSamples.Append (graphData[j].HoldConfidence);
					graphSamples.Append ('\n');

					if ((j + 1) % 1000 == 0) {
						writer.Write (graphSamples.ToString ());
						writer.Flush ();
						graphSamples.Clear ();
					}

				}

				writer.Write (graphSamples.ToString ());
				graphSamples.Clear ();

				Console.WriteLine ($"Put currency {Currencies.GetCurrencyToken (graphs[i].Currency)} in dataset.");

			}
			writer.Close ();
		}

	}

}