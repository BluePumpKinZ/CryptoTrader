using CryptoTrader.Algorithms.Orders;
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

		public const double MINIMUM_ORDER_QUANTITY_BTC = 0.00010000;

		public static string CreateOrder (Order order) {
			string url = order.GetOrderUrl ();
			url = url.Replace (",", ".");
			return NicehashWeb.Post (NicehashURLs.ExchangePrivate.createOrder + url, null, NicehashSystem.GetUTCTimeMillis ().ToString (), false);
		}

		public static FeeStatus GetFees () {
			return new FeeStatus (NicehashWeb.Get (NicehashURLs.ExchangePrivate.feeStatus, true));
		}

	}

	public class ExchangePublic {

		public static PriceFrame GetPrices () {
			return new PriceFrame (NicehashWeb.Get (NicehashURLs.ExchangePublic.prices, false));
		}

		public static ExchangeStatus GetExchangeStatus () {
			return new ExchangeStatus (NicehashWeb.Get (NicehashURLs.ExchangePublic.status, false));
		}

	}

}
