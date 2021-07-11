using CryptoTrader.Exceptions;
using CryptoTrader.NicehashAPI;
using CryptoTrader.NicehashAPI.JSONObjects;
using CryptoTrader.NicehashAPI.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace CryptoTrader {

	public class PriceWatcher {

		private static readonly List<PriceGraph> graphs = new List<PriceGraph> ();
		private static string priceStoragePath;
		private static Thread priceWatchThread;
		private static bool stopPriceWatchThread = false;
		public static bool HasPrices { get { return graphs.Count != 0; } }
		public static FeeStatus FeeStatus { private set; get; }

		public static void SetPath (string path) {
			priceStoragePath = path;
			string folderPath = Path.GetDirectoryName (priceStoragePath);
			if (!Directory.Exists (folderPath))
				throw new DirectoryNotFoundException ($"The directory '{folderPath}' could not be found");
		}

		public static void Start () {
			UpdateFeeStatusGuaranteed ();
			if (string.IsNullOrEmpty (priceStoragePath))
				throw new FileLoadException ("The path given loading price data has not been set. Use SetPath () for assigning the path used for price data storage.");
			if (File.Exists (priceStoragePath)) {
				LoadPrices ();
			} else {
				Console.WriteLine ("No file found for reading price data, starting fresh.");
			}
			priceWatchThread = new Thread (new ThreadStart (() => PriceWatchThread ()));
			priceWatchThread.Start ();
		}

		private static void PriceWatchThread () {

			int min = DateTime.Now.Minute;
			int hour = DateTime.Now.Hour;
			int day = DateTime.Now.Day;
			bool failing = false;
			for (; ; ) {
				try {

					// Record prices every minute
					if (min != DateTime.Now.Minute) {
						AddPriceSlice (ExchangePublic.GetPrices ());
						min = DateTime.Now.Minute;
					}

					// Save price data every hour
					if (hour != DateTime.Now.Hour) {
						SavePrices ();
						hour = DateTime.Now.Hour;
					}

					// Update fees every day
					if (day != DateTime.Now.Day) {
						UpdateFeeStatus ();
						day = DateTime.Now.Day;
					}

					failing = false;

					// Stop thread when stopPriceThread is set to true
					if (stopPriceWatchThread)
						return;

					Thread.Sleep (50);
				} catch (Exception) {
					if (!failing) {
						Console.Write ("Error ");
						failing = true;
					}
					Console.Write ("|");
					Thread.Sleep (1000);
				}
			}
		}

		private static void UpdateFeeStatusGuaranteed () {
			try {
				UpdateFeeStatus ();
			} catch (Exception) {
				Thread.Sleep (1000);
				UpdateFeeStatusGuaranteed ();
			}
		}

		public static Currency[] GetMonitoredCurrencies () {
			Currency[] currencies = new Currency[graphs.Count];
			for (int i = 0; i < graphs.Count; i++) {
				currencies[i] = graphs[i].Currency;
			}
			return currencies;
		}

		public static bool IsMonitoring () {
			return priceWatchThread != null;
		}

		public static void Stop () {
			stopPriceWatchThread = true;
			if (priceWatchThread != null)
				priceWatchThread.Join ();
			priceWatchThread = null;
		}

		public static void StopAndSave () {
			Stop ();
			SavePrices ();
		}

		private static void AddPriceSlice (PriceFrame priceFrame) {
			foreach (KeyValuePair<string, double> kvp in priceFrame.Prices) {
				if (!Currencies.TryGetCurrencyFromBTCPair (kvp.Key, out Currency c))
					continue;
				AddPriceUnit (new PriceUnit (c, NicehashSystem.GetUTCTimeMillis (), kvp.Value));
			}
		}

		private static void AddPriceUnit (PriceUnit priceUnit) {
			GetGraphForCurrency (priceUnit.Currency).AddPriceValue (priceUnit.MilliTime, priceUnit.Price);
		}

		public static Balance ConvertToCurrency (Balance balance, Currency currency) {

			double btcPrice;
			if (balance.BTCRate != 0) {
				btcPrice = balance.BTCRate;
			} else {
				btcPrice = GetBTCPrice (balance.Currency);
			}
			double btcAvailableValue = balance.Available / btcPrice;
			double btcPendingValue = balance.Pending / btcPrice;
			double btcRate = GetBTCPrice (currency);
			return new Balance (currency, btcAvailableValue * btcRate, btcPendingValue * btcRate, btcRate);
		}

		public static double GetBTCPrice (Currency c, bool raw = false) {
			if (c == Currency.Bitcoin)
				return 1;
			return GetGraphForCurrency (c).GetLastPrice (raw);
		}

		public static PriceGraph GetGraphForCurrency (Currency currency) {
			if (graphs == null)
				throw new NoPricesFoundException ($"Graph for currency \"{currency}\" does not exist.");
			for (int i = 0; i < graphs.Count; i++) {
				if (graphs[i].Currency == currency)
					return graphs[i];
			}
			PriceGraph graph = new PriceGraph (currency);
			graphs.Add (graph);
			return graph;
		}

		public static void UpdateFeeStatus () {
			FeeStatus = ExchangePrivate.GetFees ();
		}

		// Currency (4 bytes)
		// Time (8 bytes)
		// Price (8 bytes)
		public static void LoadPrices () {
			byte[] data = File.ReadAllBytes (priceStoragePath);
			uint hash;
			long milliTime;
			double price;
			for (int i = 0; i < data.LongLength; i += 20) {
				hash = BitConverter.ToUInt32 (data, i);
				milliTime = BitConverter.ToInt64 (data, i + 4);
				price = BitConverter.ToDouble (data, i + 12);

				AddPriceUnit (new PriceUnit (GetCurrencyFromHash (hash), milliTime, price));
			}
			Console.WriteLine ($"Loaded prices from {priceStoragePath}");
		}

		public static void SavePrices () {

			if (!HasPrices) {
				Console.WriteLine ("Did not save prices because there was nothing to save");
				return;
			}

			List<PriceUnit> priceUnits = new List<PriceUnit> ();
			for (int i = 0; i < graphs.Count; i++) {
				priceUnits.AddRange (graphs[i].ConvertToPriceUnits ());
			}

			byte[] data = new byte[20 * priceUnits.Count];
			for (int i = 0; i < priceUnits.Count; i++) {
				byte[] currencyBytes = BitConverter.GetBytes (GetCurrencyTokenHash (priceUnits[i].Currency));
				byte[] timeBytes = BitConverter.GetBytes (priceUnits[i].MilliTime);
				byte[] priceBytes = BitConverter.GetBytes (priceUnits[i].Price);

				// Console.WriteLine ($"Saved {priceUnits[i].Currency} | {priceUnits[i].MilliTime} | {priceUnits[i].Price}");

				Array.Copy (currencyBytes, 0, data, 20 * i + 0, 4);
				Array.Copy (timeBytes, 0, data, 20 * i + 4, 8);
				Array.Copy (priceBytes, 0, data, 20 * i + 12, 8);
			}

			File.WriteAllBytes (priceStoragePath, data);
			Console.WriteLine ($"Saved prices to {priceStoragePath}");
		}

		private static uint GetCurrencyTokenHash (Currency currency) {
			string token = Currencies.GetCurrencyToken (currency);
			using HashAlgorithm algorithm = SHA256.Create ();
			byte[] fullHashbytes = algorithm.ComputeHash (Encoding.UTF8.GetBytes (token));
			byte[] shortenedHashBytes = new byte[4];
			for (byte i = 0; i < fullHashbytes.Length; i++) {
				shortenedHashBytes[i % 4] ^= fullHashbytes[i];
			}
			return BitConverter.ToUInt32 (shortenedHashBytes);
		}

		private static Dictionary<uint, Currency> currencyHashesLookUpTable;
		private static Currency GetCurrencyFromHash (uint hash) {
			if (currencyHashesLookUpTable == null)
				currencyHashesLookUpTable = new Dictionary<uint, Currency> ();
			if (currencyHashesLookUpTable.TryGetValue (hash, out Currency currency))
				return currency;

			Currency[] currencies = Enum.GetValues (typeof (Currency)) as Currency[];
			for (int i = 0; i < currencies.Length; i++) {
				uint newHash = GetCurrencyTokenHash (currencies[i]);
				if (newHash == hash) {
					currencyHashesLookUpTable.Add (hash, currencies[i]);
					return currencies[i];
				}
			}
			throw new HashNotFoundException ($"No currency could be found for hash {hash}", hash);
		}

	}

}
