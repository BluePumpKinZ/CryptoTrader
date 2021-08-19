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

				} catch (Exception e) {
					Console.WriteLine (e);
				}

			}

		}
	}
}
