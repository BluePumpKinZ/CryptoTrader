using CryptoTrader.NicehashAPI;
using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;

namespace CryptoTrader {

	public class Program {

		static void Main () {

			Trader trader = new Trader ();
			trader.Initialize ();
			Console.WriteLine ("Trader initialized");
			// trader.SetPriceWatcherPath ("C:/Users/jonas/Documents/Crypto/pricehistory.data");
			trader.SetPriceWatcherPath (Directory.GetCurrentDirectory() + "\\pricehistory.data");

			while (true) {
				try {
					string input = Console.ReadLine ();
					string[] split = input.Split (" ", 2);
					string command = split[0];

					switch (command) {
					case "setpricewatchpath":
						trader.SetPriceWatcherPath (split[1]);
						break;
					case "start":
						trader.Start ();
						break;
					case "stop":
						trader.StopAndSave ();
						return;
					case "status":
						Console.WriteLine (trader.GetStatusPrintOut ());
						break;
					case "readbalances":
						Console.WriteLine (Accounting.GetBalances ());
						break;
					case "save":
						trader.SavePrices ();
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
