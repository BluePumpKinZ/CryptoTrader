using CryptoTrader.Algorithms;
using CryptoTrader.Keys;
using CryptoTrader.NicehashAPI;
using CryptoTrader.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CryptoTrader {

	public static class Trader {

		private static List<Algorithm> algorithms = new List<Algorithm> ();

		public static void Initialize () {
			AppDomain.CurrentDomain.ProcessExit += new EventHandler (AutoSavePrices);
			// LoadAlgorithms ();
		}

		public static void LoadAlgorithms (string path) {
			if (!File.Exists (path)) {
				Console.WriteLine ("Trader algorithms could not be loaded, make sure the given path is valid");
			}

			byte[] bytes = File.ReadAllBytes (path);
			algorithms.Clear ();

			int byteIndex = 0;
			int algoCount = BitConverter.ToInt32 (bytes, byteIndex);
			byteIndex += 4;
			for (int i = 0; i < algoCount; i++) {
				int algoNameLength = BitConverter.ToInt32 (bytes, byteIndex);
				byteIndex += 4;

				byte[] algoNameBytes = new byte[algoNameLength];
				Array.Copy (bytes, byteIndex, algoNameBytes, 0, algoNameLength);
				byteIndex += algoNameLength;

				int algoLength = BitConverter.ToInt32 (bytes, byteIndex);
				byteIndex += 4;

				byte[] algoBytes = new byte[algoLength];
				Array.Copy (bytes, byteIndex, algoNameBytes, 0, algoLength);
				byteIndex += algoLength;

				Algorithm algo = TypeMapping.AlgorithmFromName (TypeMapping.BytesToString (algoNameBytes));
				algo.LoadFromBytes (algoBytes);
				algorithms.Add (algo);
			}
		}

		public static void SaveAlgorithms (string path) {
			List<byte> bytes = new List<byte> ();
			bytes.AddRange (BitConverter.GetBytes(algorithms.Count));
			algorithms.ForEach ((t) => {
				byte[] algoNameBytes = TypeMapping.NameToBytes (TypeMapping.NameFromAlgorithm (t));
				byte[] algoBytes = t.SaveToBytes ();
				bytes.AddRange (BitConverter.GetBytes (algoNameBytes.Length));
				bytes.AddRange (algoNameBytes);
				bytes.AddRange (BitConverter.GetBytes (algoBytes.Length));
				bytes.AddRange (algoBytes);
			});
			File.WriteAllBytes (path, bytes.ToArray ());
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
