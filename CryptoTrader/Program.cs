using CryptoTrader.Inputs;
using System;

namespace CryptoTrader {

	public class Program {

		static void Main () {

			try {
				Config config = new Config ();
				Trader.Initialize (config);
			} catch (Exception) {
				Console.WriteLine ("Config could not be loaded");
				Environment.Exit (-1);
			}

			while (true) {
				try {
					Console.ForegroundColor = ConsoleColor.Green;
					Console.Write (" # ");
					Console.ResetColor ();
					string input = Console.ReadLine ();

					Input.ProcessInput (input);

					/*string[] split = input.Split (" ", 2);
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
					}*/

				} catch (Exception e) {
					Console.WriteLine (e);
				}

			}

		}
	}
}
