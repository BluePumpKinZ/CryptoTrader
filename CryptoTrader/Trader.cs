using CryptoTrader.AISystem;
using CryptoTrader.Algorithms;
using CryptoTrader.Keys;
using CryptoTrader.NicehashAPI;
using CryptoTrader.NicehashAPI.JSONObjects;
using CryptoTrader.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
			PriceWatcher.AddToOnPriceUpdate (() => UpdateBalances ());
			PriceWatcher.AddToOnPriceUpdate (() => IterateAlgorithms ());
			Console.WriteLine ("Trader initialized.");
		}

		private static void GetBalancesGuaranteed () {
			try {
				UpdateBalances ();
			} catch (Exception) {
				Thread.Sleep (1000);
				GetBalancesGuaranteed ();
			}
			worthSaving = true;
		}

		public static void UpdateBalances () {
			balances = Accounting.GetBalances ();
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
			Console.WriteLine ("Enabled trading");
			worthSaving = true;
		}

		public static void DisableTrading () {
			IsTrading = false;
			Console.WriteLine ("Disabled trading");
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
			try {
				GetAlgorithmForCurrency (currency).IsTraining = false;
				Console.WriteLine ($"Enabled algorithm for currency '{Currencies.GetCurrencyToken (currency)}'.");
				worthSaving = true;
			} catch (ApplicationException) {
				Console.WriteLine ($"No algorithm for currency '{Currencies.GetCurrencyToken (currency)}' exists.");
			}
		}

		public static void DisableAlgorithm (Currency currency) {
			try {
				GetAlgorithmForCurrency (currency).IsTraining = true;
				Console.WriteLine ($"Disabled algorithm for currency '{Currencies.GetCurrencyToken (currency)}'.");
				worthSaving = true;
			} catch (ApplicationException) {
				Console.WriteLine ($"No algorithm for currency '{Currencies.GetCurrencyToken (currency)}' exists.");
			}
		}

		public static void AssignRatio (Currency currency, double ratio) {

			Algorithm algorithm;
			try {
				algorithm = GetAlgorithmForCurrency (currency);
			} catch (ApplicationException) {
				Console.WriteLine ($"No algorithm for currency '{Currencies.GetCurrencyToken (currency)}' exists.");
				return;
			}

			double oldRatio = algorithm.TotalBalancesRatioAssinged;
			double ratioDiff = ratio - oldRatio;

			double totalRatio = 0;
			for (int i = 0; i < algorithms.Count; i++) {
				totalRatio += algorithms[i].TotalBalancesRatioAssinged;
			}

			double multiplier = Math.Min ((1 - ratioDiff) / totalRatio, 1);

			for (int i = 0; i < algorithms.Count; i++) {
				algorithms[i].TotalBalancesRatioAssinged *= multiplier;
			}

			algorithm.TotalBalancesRatioAssinged = ratio;
			Console.WriteLine ($"Assigned a ratio of {ratio} to currency '{Currencies.GetCurrencyToken (currency)}'.");
			worthSaving = true;
		}

		public static void AddAlgorithm (Algorithm algo) {
			if (algo.PrimaryCurrency == Currency.Null)
				throw new ArgumentException ("Algorithm must have a currency assigned.");
			if (GetIndexForCurrency (algo.PrimaryCurrency) != -1) {
				Console.WriteLine ($"Already an algorithm for currency '{Currencies.GetCurrencyToken (algo.PrimaryCurrency)}'.");
				return;
			}
			algorithms.Add (algo);
			Console.WriteLine ($"Added algorithm for currency '{Currencies.GetCurrencyToken (algo.PrimaryCurrency)}'.");
			worthSaving = true;
		}

		public static void DeleteAlgorithm (Currency currency) {
			if (currency == Currency.Null) {
				Console.WriteLine ("Currency cannot be null.");
				return;
			}
			int index = GetIndexForCurrency (currency);
			if (index == -1) {
				Console.WriteLine ($"No algorithm for currency '{Currencies.GetCurrencyToken (currency)}' could be found.");
				return;
			}
			algorithms.RemoveAt (index);
			Console.WriteLine ($"Deleted algorithm for currency '{Currencies.GetCurrencyToken (currency)}'.");
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

		public static void ImproveImprovableAlgorithm (Currency currency, int epochs, int threads, bool autosave) {
			if (!GetImprovableAlgorithm (currency, out IImprovableAlgorithm algorithm))
				return;
			Console.WriteLine ($"Started algorithm improvement for currency {currency} for {epochs} epochs.");

			AIProcessTaskScheduler.RunOnThread (() => {
				algorithm.Improve (epochs, threads);
				Console.WriteLine ($"Finished {epochs} epochs for algorithm for currency {currency}");
				if (autosave)
					SaveAlgorithms ();
			});
		}

		public static void ImproveImprovableAlgorithmForTime (Currency currency, int durationInMinutes, int threads, bool autosave) {
			if (!GetImprovableAlgorithm (currency, out IImprovableAlgorithm algorithm))
				return;
			string time = $"{durationInMinutes / 60} hours and {durationInMinutes % 60} minutes";
			Console.WriteLine ($"Started algorithm improvement for currency {currency} for {time}.");
			AIProcessTaskScheduler.RunOnThread (() => {
				int totalIterations = 0;
				Stopwatch sw = new Stopwatch ();
				sw.Start ();
				while (sw.Elapsed.TotalMinutes < durationInMinutes) {
					algorithm.Improve (10, threads);
					totalIterations += 10;
				}
				Console.WriteLine ($"Finished {totalIterations} epochs in {time} for algorithm for currency {currency}");
				if (autosave)
					SaveAlgorithms ();
			});
			worthSaving = true;
		}

		public static double GetAlgorithmLoss (Currency currency) {
			if (GetImprovableAlgorithm (currency, out IImprovableAlgorithm improvableAlgorithm))
				return improvableAlgorithm.GetLoss ();
			return double.MaxValue;
		}

		public static void ImportAlgorithm (string path) {
			if (!File.Exists (path)) {
				Console.WriteLine ($"No algorithm could be found at {path}");
				return;
			}
			int index = 0;
			byte[] bytes = File.ReadAllBytes (path);
			Algorithm algorithm = TypeMapping.AlgorithmFromName (TypeMapping.BytesToString (IStorable.GetDataRange (ref index, bytes)));
			algorithm.LoadFromBytes (ref index, bytes);
			AddAlgorithm (algorithm);
			worthSaving = true;
		}

		public static void ExportAlgorithm (Currency currency) {
			int index = GetIndexForCurrency (currency);
			if (index == -1) {
				Console.WriteLine ($"No algorithm to export for currency '{Currencies.GetCurrencyToken (currency)}'.");
				return;
			}
			string[] files = Directory.GetFiles (Directory.GetCurrentDirectory ());
			for (int i = 0; i < files.Length; i++)
				files[i] = files[i].Split (new char[] { '\\', '/' })[^1];
			string filename;
			int counter = 0;
			do {
				filename = $"{Currencies.GetCurrencyToken (currency).ToLower ()}{(counter != 0 ? $"_{counter}" : "")}.algo";
				counter++;
			} while (files.Contains (filename));
			List<byte> bytes = new List<byte> ();
			IStorable.AddData (ref bytes, TypeMapping.NameToBytes (TypeMapping.NameFromAlgorithm (algorithms[index])));
			algorithms[index].SaveToBytes (ref bytes);
			string fullPath = $"{Directory.GetCurrentDirectory ()}{Path.DirectorySeparatorChar}{filename}";
			File.WriteAllBytes (fullPath, bytes.ToArray ());
			Console.WriteLine ($"Algorithm for currency '{Currencies.GetCurrencyToken (currency)}' exported at {fullPath}");
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
			Console.WriteLine ($"Saved algorithms to {algorithmStoragePath}");
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

		public static string ListAlgorithms () {
			StringBuilder sb = new StringBuilder ();
			sb.Append ("Active algorithms:");
			double totalBtc = balances.TotalBalance.ToCurrency (Currency.Tether, PriceWatcher.GetBTCPrice (Currency.Tether)).Total;
			for (int i = 0; i < algorithms.Count; i++)
				if (!algorithms[i].IsTraining)
					sb.Append ($"\n\t{Currencies.GetCurrencyToken (algorithms[i].PrimaryCurrency)}\t | {Math.Round (algorithms[i].TotalBalancesRatioAssinged * totalBtc, 2)} USD");
			sb.Append ("\nInactive algorithms:");
			for (int i = 0; i < algorithms.Count; i++)
				if (algorithms[i].IsTraining)
					sb.Append ($"\n\t{Currencies.GetCurrencyToken (algorithms[i].PrimaryCurrency)}\t | {Math.Round (algorithms[i].TotalBalancesRatioAssinged * totalBtc, 2)} USD");
			return sb.ToString ();
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
			sb.Append ("\n");
			sb.Append (ListAlgorithms ());
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
