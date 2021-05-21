using System;
using System.Collections.Generic;
using System.Text;
using CryptoTrader.NicehashAPI;

namespace CryptoTrader {

	public enum OrderType { Market, Limit }
	public class Trader {

		private PriceWatcher priceWatcher;

		public void Initialize () {
			priceWatcher = new PriceWatcher ();
		}

		public void Start () {
			priceWatcher.Start ();
		}

		public void StopAndSave () {
			priceWatcher.StopAndSave ();
		}

		public void SetPriceWatcherPath (string path) {
			priceWatcher.SetPath (path);
		}

		public string GetStatusPrintOut () {
			Currency[] monitoredCurrencies = priceWatcher.GetMonitoredCurrencies ();
			StringBuilder sb = new StringBuilder ();
			sb.Append ("Monitoring: ");
			if (priceWatcher.IsMonitoring ()) {
				sb.Append ("Enabled\n");
			} else {
				sb.Append ("Disabled\n");
			}
			sb.Append ("Monitored currencies: ");
			for (int i = 0; i < monitoredCurrencies.Length; i++) {
				sb.Append (Currencies.GetCurrencyToken (monitoredCurrencies[i]) + ", ");
			}
			sb.Append ("\n");
			return sb.ToString ();
		}

		public void SavePrices () {
			priceWatcher.SavePrices ();
		}

	}
}
