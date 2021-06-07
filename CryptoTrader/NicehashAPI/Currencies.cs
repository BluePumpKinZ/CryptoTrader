﻿using System;
using System.Collections.Generic;

namespace CryptoTrader.NicehashAPI {

	public enum Currency {
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
		Gifto,
		Selfkey,
		KyberNetwork,
		Cred,
		ChainLink,
		Loom,
		Litecoin,
		Polygon,
		Mithril,
		Metal,
		Nexo,
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
			{ Currency.Band_Protocol,      "Band"},
			{ Currency.BAT,                "BAT"},
			{ Currency.Bitcoin_Cash,       "BCH"},
			{ Currency.Bancor,             "BNT"},
			{ Currency.Bitcoin,            "BTC"},
			{ Currency.Bitcoin_Gold,       "BTG"},
			{ Currency.Civic,              "CVC"},
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
			{ Currency.Gifto,              "GTO"},
			{ Currency.Selfkey,            "KEY"},
			{ Currency.KyberNetwork,       "KNC"},
			{ Currency.Cred,               "LBA"},
			{ Currency.ChainLink,          "LINK"},
			{ Currency.Loom,               "LOOM"},
			{ Currency.Litecoin,           "LTC"},
			{ Currency.Polygon,            "MATIC"},
			{ Currency.Mithril,            "MITH"},
			{ Currency.Metal,              "MTL"},
			{ Currency.Nexo,               "NEXO"},
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
			if (pair.Contains ("USDT")) {
				currency = Currency.AaveToken;
				return false;
			}
			Currency[] allCurrencies = Enum.GetValues (typeof (Currency)) as Currency[];
			for (int i = 0; i < coinTokens.Count; i++) {
				if ($"{coinTokens[allCurrencies[i]]}BTC" == pair) {
					currency = allCurrencies[i];
					return true;
				}
				if ($"BTC{coinTokens[allCurrencies[i]]}" == pair) {
					currency = allCurrencies[i];
					return true;
				}
			}
			currency = Currency.AaveToken;
			return false;
		}

		public static Currency GetCurrencyFromToken (string token) {
			Currency c = Currency.Euro;
			Currency[] allCurrencies = Enum.GetValues (typeof (Currency)) as Currency[];
			for (int i = 0; i < coinTokens.Count; i++) {
				if (coinTokens[allCurrencies[i]] == token) {
					return allCurrencies[i];
				}
			}
			return c;
		}

		public static int CountMarkets () {
			return coinTokens.Keys.Count;
		}

	}

}