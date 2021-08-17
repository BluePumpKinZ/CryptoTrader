using CryptoTrader.AISystem;
using CryptoTrader.Algorithms;
using CryptoTrader.Keys;
using CryptoTrader.NicehashAPI;
using CryptoTrader.NicehashAPI.JSONObjects;
using CryptoTrader.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace CryptoTrader {

	public static class Trader {

		private static readonly List<Algorithm> algorithms = new List<Algorithm> ();
		private static string algorithmStoragePath;
		public static bool IsTrading { private set; get; } = false;
		private static Balances balances = new Balances ();

		public static void Initialize (Config config) {

			SetAlgorithmPath (config.AlgorithmPath);
			ReadKeysFromPath (config.KeyPath);
			SetPriceWatcherPath (config.PricewatcherPath);
			SetKeySet (config.KeySet);
			AIProcessTaskScheduler.SetThreadCount (config.MaxThreads);

			AppDomain.CurrentDomain.ProcessExit += new EventHandler (AutoSavePrices); // TODO add trader saving to autosave
			Currencies.GenerateLookUpTables ();
			LoadAlgorithms ();
			PriceWatcher.LoadPrices ();
			PriceWatcher.AddToOnPriceUpdate (() => IterateAlgorithms());
			Console.WriteLine ("Trader initialized.");
		}

		private static void GetBalancesGuaranteed () {
			try {
				balances = Accounting.GetBalances ();
			} catch (Exception) {
				Thread.Sleep (1000);
				GetBalancesGuaranteed ();
			}
		}

		private static void IterateAlgorithms () {
			if (!IsTrading)
				return;
			for (int i = 0; i < algorithms.Count; i++) {
				if (algorithms[i].PrimaryCurrency == Currency.Null)
					continue;
				if (algorithms[i].IsTraining)
					continue;
				algorithms[i].Iterate (PriceWatcher.GetGraphForCurrency (algorithms[i].PrimaryCurrency), ref balances);
			}
		}

		public static void EnableTrading () {
			IsTrading = true;
		}

		public static void DisableTrading () {
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
			if (folderPath == "")
				folderPath = Directory.GetCurrentDirectory ();
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

			int index = 0;
			int algoCount = BitConverter.ToInt32 (IStorable.GetDataRange (ref index, bytes));
			algorithms.Clear ();
			for (int i = 0; i < algoCount; i++) {
				Algorithm algorithm = TypeMapping.AlgorithmFromName (TypeMapping.BytesToString(IStorable.GetDataRange (ref index, bytes)));
				algorithm.LoadFromBytes (ref index, bytes);
				algorithms.Add (algorithm);
			}

			balances = new Balances ();
			balances.LoadFromBytes (ref index, bytes);
			Console.WriteLine ($"Algorithms loaded from {algorithmStoragePath}");
		}

		public static void SaveAlgorithms () {
			List<byte> bytes = new List<byte> ();

			int algoCount = algorithms.Count;
			IStorable.AddData (ref bytes, BitConverter.GetBytes (algoCount));
			for (int i = 0; i < algoCount; i++) {
				IStorable.AddData (ref bytes, TypeMapping.NameToBytes (TypeMapping.NameFromAlgorithm(algorithms[i])));
				algorithms[i].SaveToBytes (ref bytes);
			}

			balances.SaveToBytes (ref bytes);

			File.WriteAllBytes (algorithmStoragePath, bytes.ToArray ());
		}

		public static void Start () {
			AIProcessTaskScheduler.StartExecuting ();
			GetBalancesGuaranteed ();
			PriceWatcher.Start ();
			EnableTrading ();
		}

		public static void Stop () {
			PriceWatcher.Stop ();
			AIProcessTaskScheduler.StopExecuting ();
		}

		public static void Save () {
			PriceWatcher.SavePrices ();
			SaveAlgorithms ();
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

		public static string GetStatusPrintOut () { // TODO add active algorithms
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
