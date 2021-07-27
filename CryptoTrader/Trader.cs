using CryptoTrader.Keys;
using CryptoTrader.NicehashAPI;
using System;
using System.Text;

namespace CryptoTrader {

	public static class Trader {

		public static void Initialize () {
			AppDomain.CurrentDomain.ProcessExit += new EventHandler (AutoSavePrices);
		}

		public static void Start () {
			PriceWatcher.Start ();
		}

		public static void Stop () {
			PriceWatcher.Stop ();
		}

		public static void Save () {
			PriceWatcher.SavePrices ();
		}

		public static void StopAndSave () {
			Save ();
			Stop ();
		}

		public static void SetPriceWatcherPath (string path) {
			PriceWatcher.SetPath (path);
		}

		public static void ReadKeysFromPath (string path) {
			KeyValues.SetPath (path);
			KeyValues.ReadKeys ();
		}

		public static void SetKeySet (string setName) {
			KeyValues.SelectKeySet (setName);
		}

		public static string GetStatusPrintOut () {
			Currency[] monitoredCurrencies = PriceWatcher.GetMonitoredCurrencies ();
			StringBuilder sb = new StringBuilder ();
			sb.Append ("Price monitoring: ");
			if (PriceWatcher.IsMonitoring ()) {
				sb.Append ("Enabled\n");
				sb.Append ("Monitored currencies: ");
				for (int i = 0; i < monitoredCurrencies.Length; i++) {
					sb.Append (Currencies.GetCurrencyToken (monitoredCurrencies[i]) + ", ");
				}
			} else {
				sb.Append ("Disabled");
			}
			return sb.ToString ();
		}

		private static void AutoSavePrices (object sender = null, EventArgs e = null) {
			if (PriceWatcher.HasAddedPrices)
				PriceWatcher.SavePrices ();
			else
				Console.WriteLine ("Autosave cancelled because the pricewatcher was never started.");
		}

	}
}
