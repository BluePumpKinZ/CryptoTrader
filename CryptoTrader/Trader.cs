using CryptoTrader.Keys;
using CryptoTrader.NicehashAPI;
using System;
using System.Text;

namespace CryptoTrader {

	public class Trader {

		public static Trader Instance { private set; get; }

		public void Initialize () {
			Instance = this;
			AppDomain.CurrentDomain.ProcessExit += new EventHandler (AutoSavePrices);
		}

		public void Start () {
			PriceWatcher.Start ();
		}

		public void Stop () {
			PriceWatcher.Stop ();
		}

		public void Save () {
			PriceWatcher.SavePrices ();
		}

		public void StopAndSave () {
			Save ();
			Stop ();
		}

		public void SetPriceWatcherPath (string path) {
			PriceWatcher.SetPath (path);
		}

		public void ReadKeysFromPath (string path) {
			KeyValues.SetPath (path);
			KeyValues.ReadKeys ();
		}

		public void SetKeySet (string setName) {
			KeyValues.SelectKeySet (setName);
		}

		public string GetStatusPrintOut () {
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

		protected void AutoSavePrices (object sender = null, EventArgs e = null) {
			if (PriceWatcher.HasPrices)
				PriceWatcher.SavePrices ();
			else
				Console.WriteLine ("Autosave cancelled because the pricewatcher was never started.");
		}

	}
}
