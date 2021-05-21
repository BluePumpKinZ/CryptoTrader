using CryptoTrader.NicehashAPI.JSONObjects;
using CryptoTrader.NicehashAPI.Utils;

namespace CryptoTrader.NicehashAPI {

	public class Accounting {

		public static Balances GetBalances () {
			return new Balances (NicehashURLs.Accounting.balances);
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
