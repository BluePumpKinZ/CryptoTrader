using System;

namespace CryptoTrader.NicehashAPI.JSONObjects {

	public enum MarketState {
		Unknown, Closed, Trading, Readonly, Unavailable, Removed
	}

	public class MarketStatus : IParsable {

		public Currency Currency { private set; get; } = Currency.Null;
		public MarketState State { private set; get; } = MarketState.Trading;
		public int BaseAssetPrecision { private set; get; } = 8;
		public int QuoteAssetPrecision { private set; get; } = 8;
		public int PriceAssetPrecision { private set; get; } = 8;
		public int PriceStep { private set; get; } = 8;
		public double PriMinAmount { private set; get; } = 0.00010000;
		public double PriMaxAmount { private set; get; } = 1e6;
		public double SecMinAmount { private set; get; } = 0.00010000;
		public double SecMaxAmount { private set; get; } = 1e6;
		public double MinPrice { private set; get; } = 1e-8;
		public double MaxPrice { private set; get; } = 1e6;

		public MarketStatus () {

		}

		public MarketStatus (string s) {
			Parse (s);
		}

		internal override void ParsePart (string key, string value) {
			switch (key) {
			case "symbol":
				Currencies.TryGetCurrencyFromBTCPair (value, out Currency currency);
				Currency = currency;
				break;
			case "status":
				State = StringToMarketState (value);
				break;
			case "baseAssetPrecision":
				BaseAssetPrecision = int.Parse (value);
				break;
			case "quoteAssetPresicion":
				QuoteAssetPrecision = int.Parse (value);
				break;
			case "priceAssetPrecision":
				PriceAssetPrecision = int.Parse (value);
				break;
			case "priceStep":
				PriceStep = int.Parse (value);
				break;
			case "priMinAmount":
				PriMinAmount = double.Parse (value);
				break;
			case "priMaxAmount":
				PriMaxAmount = double.Parse (value);
				break;
			case "secMinAmount":
				SecMinAmount = double.Parse (value);
				break;
			case "secMaxAmount":
				SecMaxAmount = double.Parse (value);
				break;
			case "minPrice":
				MinPrice = double.Parse (value);
				break;
			case "maxPrice":
				MaxPrice = double.Parse (value);
				break;
			}
		}

		private MarketState StringToMarketState (string state) {
			return state switch {
				"UNKNOWN" => MarketState.Unknown,
				"CLOSED" => MarketState.Closed,
				"TRADING" => MarketState.Trading,
				"READONLY" => MarketState.Readonly,
				"UNAVAILABLE" => MarketState.Unavailable,
				"REMOVED" => MarketState.Removed,
				_ => throw new ArgumentException ($"'{state}' is not a valid marketstatus.")
			};
		}

	}

}
