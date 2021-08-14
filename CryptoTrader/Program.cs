using CryptoTrader.AISystem;
using CryptoTrader.Algorithms;
using CryptoTrader.Algorithms.Orders;
using CryptoTrader.Keys;
using CryptoTrader.NicehashAPI;
using System;
using System.IO;

namespace CryptoTrader {

	public class Program {

		static void Main () {

			Trader.SetAlgorithmPath (Directory.GetCurrentDirectory () + "\\algorithms.data");
			Trader.ReadKeysFromPath ("C:/Users/jonas/OneDrive/Crypto/NicehashKeys.keys");
			Trader.SetPriceWatcherPath (Directory.GetCurrentDirectory () + "\\pricehistory.data");
			Trader.SetKeySet ("Company");

			Trader.Initialize ();
			Console.WriteLine ("Trader initialized.");

			PriceWatcher.LoadPrices ();
			PriceWatcher.UpdateFeeStatusGuaranteed ();

			Currency currency = Currency.Ether;

			PriceGraph graph = PriceWatcher.GetGraphForCurrency (currency);

			AlgoAI ai = new AlgoAI ();
			ai.SetPrimaryCurrency (currency);

			int batchSize = 500;
			for (int i = 0; i < 200; i++) {
				AIDataConversion.GetTrainingDataBatch (graph, batchSize, AIDataConversion.TIMEFRAME, out double[][] ainput, out double[][] desiredOutput);
				LayerState[] binput = AIDataConversion.ConvertToLayerStates (ref ainput);
				LayerState[] bdesiredOutput = AIDataConversion.ConvertToLayerStates (ref desiredOutput);
				ai.TrainNetwork (binput, bdesiredOutput, 0.002);

				double lossTotal = 0;
				for (int j = 0; j < batchSize; j++) {
					lossTotal += ai.GetLoss (binput[j], bdesiredOutput[j]);
				}
				Console.WriteLine ($"Epoch {i + 1} | Loss: {lossTotal / batchSize}");
			}
			
			Console.WriteLine (ai.ExecuteOnPriceGraph (graph));

			while (true) {
				try {
					Console.ForegroundColor = ConsoleColor.Green;
					Console.Write (" # ");
					Console.ResetColor ();
					string input = Console.ReadLine ();
					string[] split = input.Split (" ", 2);
					string command = split[0];

					switch (command) {
					case "setpricewatchpath":
						Trader.SetPriceWatcherPath (split[1]);
						break;
					case "start":
						Trader.Start ();
						break;
					case "stop":
						Trader.Stop ();
						Environment.Exit (0);
						break;
					case "save":
						Trader.Save ();
						break;
					case "stopandsave":
						Trader.StopAndSave ();
						return;
					case "status":
						Console.WriteLine (Trader.GetStatusPrintOut ());
						break;
					case "readprices":
						Console.WriteLine (ExchangePublic.GetPrices ());
						break;
					case "readbalances":
						Console.WriteLine (Accounting.GetBalances ());
						break;
					case "readkeys":
						try {
							KeyValues.SelectKeySet (split[1]);
							Console.WriteLine ($"Read keyset \"{split[1]}\"");
						} catch (ArgumentException) {
							Console.WriteLine ($"No set by the name \"{split[1]}\" could be found.");
						}
						break;
					case "":
						break;
					default:
						Console.WriteLine ($"Command \"{command}\" not recognized.");
						break;
					}

				} catch (Exception e) {
					Console.WriteLine (e);
				}

			}

		}
	}
}
