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
		private static bool forceStopped = false;
		private static bool worthSaving = false;

		public static void Initialize (Config config) {

			SetAlgorithmPath (config.AlgorithmPath);
			ReadKeysFromPath (config.KeyPath);
			SetPriceWatcherPath (config.PricewatcherPath);
			SetKeySet (config.KeySet);
			AIProcessTaskScheduler.SetThreadCount (config.MaxThreads);
			AIProcessTaskScheduler.StartExecuting ();

			AppDomain.CurrentDomain.ProcessExit += new EventHandler (AutoSave);
			Currencies.GenerateLookUpTables ();
			LoadAlgorithms ();
			PriceWatcher.LoadPrices ();
			PriceWatcher.AddToOnPriceUpdate (() => IterateAlgorithms ());
			Console.WriteLine ("Trader initialized.");
		}

		private static void GetBalancesGuaranteed () {
			try {
				balances = Accounting.GetBalances ();
			} catch (Exception) {
				Thread.Sleep (1000);
				GetBalancesGuaranteed ();
			}
			worthSaving = true;
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
			worthSaving = true;
		}

		public static void EnableTrading () {
			IsTrading = true;
			worthSaving = true;
		}

		public static void DisableTrading () {
			IsTrading = false;
			worthSaving = true;
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
			worthSaving = true;
		}

		public static void DisableAlgorithm (Currency currency) {
			GetAlgorithmForCurrency (currency).IsTraining = true;
			worthSaving = true;
		}

		public static void SetAlgorithm (Algorithm algo) {
			if (algo.PrimaryCurrency == Currency.Null)
				throw new ArgumentException ("Algorithm mus have a currency assigned.");
			algorithms.Add (algo);
			worthSaving = true;
		}

		public static bool GetImprovableAlgorithm (Currency currency, out IImprovableAlgorithm improvableAlgorithm) {
			Algorithm algorithm = GetAlgorithmForCurrency (currency);
			if (!(algorithm is IImprovableAlgorithm)) {
				Console.WriteLine ($"Algorithm for currency '{currency}' is not improvable.");
				improvableAlgorithm = null;
				return false;
			}
			improvableAlgorithm = (IImprovableAlgorithm)algorithm;
			return true;
		}

		public static void LaunchAlgorithmImprovement (Currency currency, int epochs) {
			if (GetImprovableAlgorithm (currency, out IImprovableAlgorithm improvableAlgorithm))
				improvableAlgorithm.Improve (epochs, AIProcessTaskScheduler.ThreadCount);
		}

		public static void GetAlgorithmLoss (Currency currency) {
			if (GetImprovableAlgorithm (currency, out IImprovableAlgorithm improvableAlgorithm))
				Console.WriteLine ($"{improvableAlgorithm.GetLoss ()}");
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
				Algorithm algorithm = TypeMapping.AlgorithmFromName (TypeMapping.BytesToString (IStorable.GetDataRange (ref index, bytes)));
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
				IStorable.AddData (ref bytes, TypeMapping.NameToBytes (TypeMapping.NameFromAlgorithm (algorithms[i])));
				algorithms[i].SaveToBytes (ref bytes);
			}

			balances.SaveToBytes (ref bytes);

			File.WriteAllBytes (algorithmStoragePath, bytes.ToArray ());
			worthSaving = false;
		}

		public static void Start () {
			GetBalancesGuaranteed ();
			PriceWatcher.Start ();
			EnableTrading ();
		}

		public static void Stop () {
			PriceWatcher.Stop ();
			AIProcessTaskScheduler.StopExecuting ();
			forceStopped = true;
			Environment.Exit (0);
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
			sb.Append ("\nTrading: ");
			if (IsTrading) {
				sb.Append ("Enabled");
			} else {
				sb.Append ("Disabled");
			}
			sb.Append ("\nActive algorithms:");
			double totalBtc = balances.TotalBalance.ToCurrency (Currency.Tether, PriceWatcher.GetBTCPrice (Currency.Tether)).Total;
			for (int i = 0; i < algorithms.Count; i++)
				if (!algorithms[i].IsTraining)
					sb.Append ($"\n\t{algorithms[i].PrimaryCurrency} | {Math.Round (algorithms[i].TotalBalancesRatioAssinged * totalBtc, 2)} USD");
			sb.Append ("\nInactive algorithms:");
			for (int i = 0; i < algorithms.Count; i++)
				if (algorithms[i].IsTraining)
					sb.Append ($"\n\t{algorithms[i].PrimaryCurrency} | {Math.Round (algorithms[i].TotalBalancesRatioAssinged * totalBtc, 2)} USD");

			return sb.ToString ();
		}

		private static void AutoSave (object sender = null, EventArgs e = null) {
			if (forceStopped)
				return;
			AutoSavePrices ();
			AutoSaveAlgorithms ();
		}

		private static void AutoSavePrices () {
			if (PriceWatcher.HasAddedPrices)
				PriceWatcher.SavePrices ();
			else
				Console.WriteLine ("Autosave cancelled because the pricewatcher was never started.");
		}

		private static void AutoSaveAlgorithms () {
			if (worthSaving)
				SaveAlgorithms ();
			else
				Console.WriteLine ("Autosave cancelled because no trades where with the algorithms.");
		}

	}
}
