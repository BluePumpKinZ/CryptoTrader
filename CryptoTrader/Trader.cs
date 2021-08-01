using CryptoTrader.Algorithms;
using CryptoTrader.Keys;
using CryptoTrader.NicehashAPI;
using CryptoTrader.NicehashAPI.JSONObjects;
using CryptoTrader.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CryptoTrader {

	public static class Trader {

		private static readonly List<Algorithm> algorithms = new List<Algorithm> ();
		private static string algorithmStoragePath;
		public static bool IsTrading { private set; get; } = false;
		private static Balances balances;

		public static void Initialize () {
			AppDomain.CurrentDomain.ProcessExit += new EventHandler (AutoSavePrices);
			Currencies.GenerateLookUpTables ();
			LoadAlgorithms ();
			PriceWatcher.AddToOnPriceUpdate (() => IterateAlgorithms());
			GetBalancesGuaranteed ();
		}

		private static void GetBalancesGuaranteed () {
			balances = Accounting.GetBalances ();
		}

		private static void IterateAlgorithms () {
			if (!IsTrading)
				return;
			for (int i = 0; i < algorithms.Count; i++) {
				if (algorithms[i].PrimaryCurrency == Currency.Null)
					continue;
				algorithms[i].Iterate (PriceWatcher.GetGraphForCurrency (algorithms[i].PrimaryCurrency), ref balances);
			}
		}

		private static void EnableTrading () {
			IsTrading = true;
		}

		private static void DisableTrading () {
			IsTrading = false;
		}

		private static int GetIndexForCurrency (Currency currency) {
			for (int i = 0; i < algorithms.Count; i++) {
				if (algorithms[i].PrimaryCurrency == currency)
					return i;
			}
			return -1;
		}

		public static Algorithm GetAlgorithmForCurrency (Currency currency) {
			int index = GetIndexForCurrency (currency);
			if (index == -1)
				throw new ApplicationException ($"Algorithm for currency \"{currency}\" does not exist.");
			return algorithms[index];
		}

		public static void SetAlgorithmPath (string path) {
			algorithmStoragePath = path;
			string folderPath = Path.GetDirectoryName (algorithmStoragePath);
			if (!Directory.Exists (folderPath))
				throw new DirectoryNotFoundException ($"The directory '{folderPath}' could not be found");
		}

		public static void EnableAlgorithm (Currency currency) {
			GetAlgorithmForCurrency (currency).IsTraining = false;
		}

		public static void DisableAlgorithm (Currency currency) {
			GetAlgorithmForCurrency (currency).IsTraining = true;
		}

		public static void SetAlgorithm (Algorithm algo) {
			if (algo.PrimaryCurrency == Currency.Null)
				throw new ArgumentException ("Algorithm mus have a currency assigned.");
			algorithms.Add (algo);
		}

		public static void LoadAlgorithms () {
			if (!File.Exists (algorithmStoragePath)) {
				Console.WriteLine ("Trader algorithms could not be loaded, make sure the given path is valid");
				return;
			}

			byte[] bytes = File.ReadAllBytes (algorithmStoragePath);
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

		public static void SaveAlgorithms () {
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
			File.WriteAllBytes (algorithmStoragePath, bytes.ToArray ());
		}

		public static void Start () {
			PriceWatcher.Start ();
			EnableTrading ();
		}

		public static void Pause () {
			DisableTrading ();
		}

		public static void UnPause () {
			EnableTrading ();
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
