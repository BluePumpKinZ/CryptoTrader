using CryptoTrader.NicehashAPI.JSONObjects;
using CryptoTrader.NicehashAPI.Utils;

namespace CryptoTrader.NicehashAPI {

	public class Accounting {

		public static Balances GetBalances () {
			return new Balances (NicehashWeb.Get (NicehashURLs.Accounting.balances, true));
		}

	}

	public class ExternalMiner {

	}

	public class HashpowerPrivate {

	}

	public class HashpowerPublic {

	}

	public class MinerPrivate {

	}

	public class Pools {

	}

	public class Public {

	}

	public class ExchangePrivate {

		public static string CreateOrder (Currency currency, OrderType orderType, double value, double price) {
			string market = Currencies.GetPair (currency);
			string side = (orderType == OrderType.BuyMarket || orderType == OrderType.BuyLimit) ? "BUY" : "SELL";
			string type = (orderType == OrderType.BuyMarket || orderType == OrderType.SellMarket) ? "MARKET" : "LIMIT";

			string url = $"{NicehashURLs.ExchangePrivate.createOrder}" +
				$"?market={market}" +
				$"&side={side}" +
				$"&type={type}" +
				$"&quantity={value}" +
				$"&price={price}";
			if (type == "MARKET")
				url += $"&secQuantity={0.9 * value}";
			url.Replace (",",".");
				return NicehashWeb.Post (url, null, NicehashSystem.GetUTCTimeMillis ().ToString (), false);
		}

		public static FeeStatus GetFees () {
			return new FeeStatus (NicehashWeb.Get (NicehashURLs.ExchangePrivate.feeStatus, true));
		}

	}

	public class ExchangePublic {

		public static PriceFrame GetPrices () {
			return new PriceFrame (NicehashWeb.Get (NicehashURLs.ExchangePublic.prices, true));
		}

	}

}
