using System;

namespace CryptoTrader.NicehashAPI.JSONObjects {

	public enum MarketState {
		Unknown, Closed, Trading, Readonly, Unavailable, Removed
	}

	public class MarketStatus : IParsable {

		public Currency Currency { private set; get; }
		public MarketState State { private set; get; }
		public int BaseAssetPrecision { private set; get; }
		public int QuoteAssetPrecision { private set; get; }
		public int PriceAssetPrecision { private set; get; }
		public int PriceStep { private set; get; }
		public double PriceMinAmount { private set; get; }
		public double PriceMaxAmount { private set; get; }
		public double SecMinAmount { private set; get; }
		public double SecMaxAmount { private set; get; }
		public double MinPrice { private set; get; }
		public double MaxPrice { private set; get; }

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
				PriceMinAmount = double.Parse (value);
				break;
			case "priMaxAmount":
				PriceMaxAmount = double.Parse (value);
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
