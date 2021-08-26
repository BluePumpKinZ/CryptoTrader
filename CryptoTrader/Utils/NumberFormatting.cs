using CryptoTrader.NicehashAPI;
using CryptoTrader.NicehashAPI.JSONObjects;

namespace CryptoTrader.Utils {

	public static class NumberFormatting {

		public static string FormatAmount (double value, Currency currency) {

			MarketStatus marketStatus = PriceWatcher.ExchangeStatus.GetStatusForCurrency (currency);
			return value.ToString ($"F{marketStatus.PriceAssetPrecision}");

		}

	}

}
