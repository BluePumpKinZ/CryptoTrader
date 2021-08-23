using CryptoTrader.Exceptions;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace CryptoTrader.NicehashAPI {

	public enum Currency {
		Null,
		AaveToken,
		Aergo,
		Aragon,
		Aurora,
		AirSwap,
		Band_Protocol,
		BAT,
		Bitcoin_Cash,
		Bancor,
		Bitcoin,
		Bitcoin_Gold,
		Civic,
		Chiliz,
		Curve_DAO,
		Dash,
		Streamr_DATAcoin,
		Dogecoin,
		ELF,
		Enjin,
		EOS,
		Ether,
		Euro,
		Fetch,
		Fantom,
		The_Graph,
		Gifto,
		HOT,
		Selfkey,
		KyberNetwork,
		Cred,
		ChainLink,
		Loom,
		Litecoin,
		Polygon,
		Mithril,
		Maker,
		Metal,
		Nexo,
		Ocean_Protocol,
		OmiseGO,
		One_Inch,
		Polymath,
		PowerLedger,
		Populous,
		Raiden,
		Augur,
		Ravencoin,
		Status,
		Storj,
		Sushi,
		Swipe,
		Uniswap,
		Tether,
		Wrapped_BTC,
		Stellar,
		Monero,
		Ripple,
		Yearn_Finance,
		Zcash,
		ZRX
	}

	public class Currencies {

		private static readonly SortedDictionary<Currency, string> coinTokens = new SortedDictionary<Currency, string> () {
			{ Currency.AaveToken,          "AAVE"},
			{ Currency.Aergo,              "AERGO"},
			{ Currency.Aragon,             "ANT"},
			{ Currency.Aurora,             "AOA"},
			{ Currency.AirSwap,            "AST"},
			{ Currency.Band_Protocol,      "BAND"},
			{ Currency.BAT,                "BAT"},
			{ Currency.Bitcoin_Cash,       "BCH"},
			{ Currency.Bancor,             "BNT"},
			{ Currency.Bitcoin,            "BTC"},
			{ Currency.Bitcoin_Gold,       "BTG"},
			{ Currency.Civic,              "CVC"},
			{ Currency.Chiliz,             "CHZ"},
			{ Currency.Curve_DAO,          "CRV"},
			{ Currency.Dash,               "DASH"},
			{ Currency.Streamr_DATAcoin,   "DATA"},
			{ Currency.Dogecoin,           "DOGE"},
			{ Currency.ELF,                "ELF"},
			{ Currency.Enjin,              "ENJ"},
			{ Currency.EOS,                "EOS"},
			{ Currency.Ether,              "ETH"},
			{ Currency.Euro,               "EUR"},
			{ Currency.Fetch,              "FET"},
			{ Currency.Fantom,             "FTM"},
			{ Currency.The_Graph,          "GRT"},
			{ Currency.Gifto,              "GTO"},
			{ Currency.HOT,                "HOT"},
			{ Currency.Selfkey,            "KEY"},
			{ Currency.KyberNetwork,       "KNC"},
			{ Currency.Cred,               "LBA"},
			{ Currency.ChainLink,          "LINK"},
			{ Currency.Loom,               "LOOM"},
			{ Currency.Litecoin,           "LTC"},
			{ Currency.Polygon,            "MATIC"},
			{ Currency.Mithril,            "MITH"},
			{ Currency.Maker,              "MKR"},
			{ Currency.Metal,              "MTL"},
			{ Currency.Nexo,               "NEXO"},
			{ Currency.Ocean_Protocol,     "OCEAN"},
			{ Currency.OmiseGO,            "OMG"},
			{ Currency.One_Inch,           "ONEINCH"},
			{ Currency.Polymath,           "POLY"},
			{ Currency.PowerLedger,        "POWR"},
			{ Currency.Populous,           "PPT"},
			{ Currency.Raiden,             "RDN"},
			{ Currency.Augur,              "REP"},
			{ Currency.Ravencoin,          "RVN"},
			{ Currency.Status,             "SNT"},
			{ Currency.Storj,              "STORJ"},
			{ Currency.Sushi,              "SUSHI"},
			{ Currency.Swipe,              "SXP"},
			{ Currency.Uniswap,            "UNI"},
			{ Currency.Tether,             "USDT" },
			{ Currency.Wrapped_BTC,        "WBTC"},
			{ Currency.Stellar,            "XLM"},
			{ Currency.Monero,             "XMR"},
			{ Currency.Ripple,             "XRP"},
			{ Currency.Yearn_Finance,      "YFI"},
			{ Currency.Zcash,              "ZEC"},
			{ Currency.ZRX,                "ZRX"}
		};

		public static string GetCurrencyToken (Currency c) {
			if (coinTokens.TryGetValue (c, out string s)) {
				return s;
			} else {
				throw new ArgumentException ("The given currency does not have corresponding coinname");
			}
		}

		public static bool TryGetCurrencyFromBTCPair (string pair, out Currency currency) {
			if (pair == "BTCUSDT") {
				currency = Currency.Tether;
				return true;
			}
			else if (pair.Contains ("USDT")) {
				currency = Currency.Null;
				return false;
			}
			Currency[] allCurrencies = Enum.GetValues (typeof (Currency)) as Currency[];
			for (int i = 0; i < coinTokens.Count; i++) {
				if (allCurrencies[i] == Currency.Null)
					continue;
				if ($"{coinTokens[allCurrencies[i]]}BTC" == pair) {
					currency = allCurrencies[i];
					return true;
				}
				if ($"BTC{coinTokens[allCurrencies[i]]}" == pair) {
					currency = allCurrencies[i];
					return true;
				}
			}
			currency = Currency.Null;
			return false;
		}

		public static Currency GetCurrencyFromToken (string token) {
			Currency c = Currency.Null;
			Currency[] allCurrencies = Enum.GetValues (typeof (Currency)) as Currency[];
			for (int i = 0; i < allCurrencies.Length; i++) {
				if (allCurrencies[i] == Currency.Null)
					continue;
				if (coinTokens[allCurrencies[i]] == token) {
					return allCurrencies[i];
				}
			}
			return c;
		}

		public static string GetPair (Currency c) {
			if (c == Currency.Bitcoin)
				return GetCurrencyToken(c) + GetCurrencyToken(Currency.Tether);
			return GetCurrencyToken (c) + "BTC";
		}

		public static int CountMarkets () {
			return coinTokens.Keys.Count - 1;
		}

		private static Dictionary<Currency, uint> currencyToHashLookUpTable;
		private static Dictionary<uint, Currency> hashToCurrencyLookUpTable;

		public static void GenerateLookUpTables () {
			currencyToHashLookUpTable = new Dictionary<Currency, uint> ();
			hashToCurrencyLookUpTable = new Dictionary<uint, Currency> ();
			Currency[] allCurrencies = Enum.GetValues (typeof (Currency)) as Currency[];
			HashAlgorithm hashAlgorithm = SHA256.Create ();
			for (int i = 0; i < allCurrencies.Length; i++) {
				if (allCurrencies[i] == Currency.Null)
					continue;
				string token = GetCurrencyToken (allCurrencies[i]);
				byte[] fullHashbytes = hashAlgorithm.ComputeHash (Encoding.UTF8.GetBytes (token));
				byte[] shortenedHashBytes = new byte[4];
				for (byte j = 0; j < fullHashbytes.Length; j++) {
					shortenedHashBytes[j % 4] ^= fullHashbytes[j];
				}
				uint hash = BitConverter.ToUInt32 (shortenedHashBytes);
				currencyToHashLookUpTable.Add (allCurrencies[i], hash);
				hashToCurrencyLookUpTable.Add (hash, allCurrencies[i]);
			}
			hashToCurrencyLookUpTable.Add (0, Currency.Null);
			currencyToHashLookUpTable.Add (Currency.Null, 0);
		}

		public static uint GetCurrencyTokenHash (Currency currency) {
			if (currency == Currency.Null)
				throw new ArgumentException ("The currency \"Null\" does not have a token");
			return currencyToHashLookUpTable[currency];
		}

		public static Currency GetCurrencyFromHash (uint hash) {
			if (hashToCurrencyLookUpTable.TryGetValue (hash, out Currency currency))
				return currency;
			throw new HashNotFoundException ($"No currency could be found for hash {hash}", hash);
		}

	}

}
