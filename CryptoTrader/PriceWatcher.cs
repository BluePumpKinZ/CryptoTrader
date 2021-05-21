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

		private readonly List<PriceGraph> graphs = new List<PriceGraph> ();
		private string priceStoragePath;
		private Thread priceWatchThread;
		private bool stopPriceWatchThread = false;

		public void SetPath (string path) {
			priceStoragePath = path;
			string folderPath = Path.GetDirectoryName (priceStoragePath);
			if (!Directory.Exists (folderPath))
				throw new DirectoryNotFoundException ($"The directory '{folderPath}' could not be found");
		}

		public void Start () {
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

		private void PriceWatchThread () {
			int samplesTaken = 0;
			for (; ; ) {
				try {
					AddPriceSlice (ExchangePublic.GetPrices ());
					UpdateFeeStatus ();

					// Save price data every hour
					samplesTaken++;
					if (samplesTaken % 60 == 0)
						SavePrices ();

					int min = DateTime.Now.Minute;
					while (min == DateTime.Now.Minute) {
						if (stopPriceWatchThread)
							return;
						Thread.Sleep (50);
					}
				} catch (Exception e) {
					Console.WriteLine (e);
					Thread.Sleep (1000);
				}
			}
		}

		public Currency[] GetMonitoredCurrencies () {
			Currency[] currencies = new Currency[graphs.Count];
			for (int i = 0; i < graphs.Count; i++) {
				currencies[i] = graphs[i].Currency;
			}
			return currencies;
		}

		public bool IsMonitoring () {
			return priceWatchThread != null;
		}

		public void StopAndSave () {
			stopPriceWatchThread = true;
			priceWatchThread.Join ();
			priceWatchThread = null;
			SavePrices ();
		}

		private void AddPriceSlice (PriceFrame priceFrame) {
			foreach (KeyValuePair<string, double> kvp in priceFrame.Prices) {
				if (!Currencies.TryGetCurrencyFromBTCPair (kvp.Key, out Currency c))
					continue;
				AddPriceUnit (new PriceUnit (c, NicehashSystem.GetUTCTimeMillis (), kvp.Value));
			}
		}

		private void AddPriceUnit (PriceUnit priceUnit) {
			GetGraphForCurrency (priceUnit.Currency).AddPriceValue (priceUnit.MilliTime, priceUnit.Price);
		}

		public Balance ConvertToCurrency (Balance balance, Currency currency) {

			double btcPrice;
			if (balance.BTCRate != 0) {
				btcPrice = balance.BTCRate;
			} else {
				btcPrice = GetBTCPrice (balance.Currency);
			}
			double btcValue = balance.TotalBalance / btcPrice;
			double btcRate = GetBTCPrice (currency);
			return new Balance (currency, btcValue * btcRate, btcRate);
		}

		public double GetBTCPrice (Currency c) {
			return GetGraphForCurrency (c).GetLastPrice ();
		}

		public PriceGraph GetGraphForCurrency (Currency currency) {
			if (graphs == null)
				return null;
			for (int i = 0; i < graphs.Count; i++) {
				if (graphs[i].Currency == currency)
					return graphs[i];
			}
			PriceGraph graph = new PriceGraph (currency);
			graphs.Add (graph);
			return graph;
		}

		public FeeStatus FeeStatus { private set; get; }

		public void UpdateFeeStatus () {
			FeeStatus = ExchangePrivate.GetFees ();
		}

		// Currency (4 bytes)
		// Time (8 bytes)
		// Price (8 bytes)
		private void LoadPrices () {
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

		public void SavePrices () {

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

			Currency[] currencies = Enum.GetValues (typeof(Currency)) as Currency[];
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
