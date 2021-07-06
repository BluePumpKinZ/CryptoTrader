﻿using System;
using System.Collections.Generic;
using System.Text;
using CryptoTrader.NicehashAPI;
using CryptoTrader.Keys;

namespace CryptoTrader {

	public enum OrderType { BuyMarket, BuyLimit, SellMarket, SellLimit }
	public class Trader {

		public static Trader Instance { private set; get; }

		public void Initialize () {
			Instance = this;
			AppDomain.CurrentDomain.ProcessExit += new EventHandler (SavePrices);
		}

		public void Start () {
			PriceWatcher.Start ();
		}

		public void StopAndSave () {
			PriceWatcher.StopAndSave ();
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

		public void SavePrices (object sender = null, EventArgs e = null) {
			if (PriceWatcher.HasPrices)
				PriceWatcher.SavePrices ();
			else
				Console.WriteLine ("Autosave cancelled because the pricewatcher was never started.");
		}

	}
}
