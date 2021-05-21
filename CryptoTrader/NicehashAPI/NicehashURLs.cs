using NLog.LayoutRenderers;

namespace CryptoTrader.NicehashAPI.Utils {

	public static class NicehashURLs {

		// public const string baseURL = "https://api-test.nicehash.com";
		public const string root = "https://api2.nicehash.com";

		public static class Accounting {
			public static string GetBalanceURL (string currency) { return $"/main/api/v2/accounting/account2/{currency}"; }
			public const string balances = "/main/api/v2/accounting/accounts2";
			public static string GetActivityURL (string currency) { return $"/main/api/v2/accounting/activity{currency}"; }
			public const string depositAddresses = "/main/api/v2/accounting/depositAddresses";
			public static string GetDeposits (string currency) { return $"/main/api/v2/accounting/deposits/{currency}"; }
			public static string GetDeposit (string currency, string id) { return $"/main/api/v2/accounting/deposits2/{currency}/{id}"; }
			public static string GetExchangeOrderTransactions (string id) { return $"/main/api/v2/accounting/exchange/{id}/trades"; }
			public static string GetHashpowerOrderTransactions (string id) { return $"/main/api/v2/accounting/hashpower/{id}/transactions"; }
			public static string GetHashpowerEarnings (string currency) { return $"/main/api/v2/accounting/hashpowerEarnings/{currency}"; }
			public static string GetTransaction (string currency, string transactionID) { return $"/main/api/v2/accounting/transaction/{currency}/{transactionID}"; }
			public static string GetTransactions (string currency) { return $"/main/api/v2/accounting/transactions/{currency}"; }
			public const string createWithdrawal = "/main/api/v2/accounting/withdrawal";
			public static string GetCancelWithdrawal (string currency, string id) { return $"/main/api/v2/accounting/withdrawal/{currency}/{id}"; }
			public static string GetWithdrawal (string currency, string id) { return $"/main/api/v2/accounting/withdrawal2/{currency}/{id}"; }
			public static string GetWithdrawalAddress (string id) { return $"/main/api/v2/accounting/withdrawalAddress/{id}"; }
			public const string withdrawalAddresses = "/main/api/v2/accounting/withdrawalAddresses";
			public static string GetWithdrawalCurrency (string currency) { return $"/main/api/v2/accounting/withdrawal/{currency}"; }
		}

		public static class ExternalMiner {
			public static string GetActiveWorkers (string btcAddress) { return $"/main/api/v2/mining/external/{btcAddress}/rigs/activeWorkers"; }
			public static string GetMinerAlgoStatistics (string btcAddress) { return $"/main/api/v2/mining/external/{btcAddress}/rigs/stats/algo"; }
			public static string GetMinerUnpaidStatistics (string btcAddress) { return $"/main/api/v2/mining/external/{btcAddress}/rigs/stats/unpaid"; }
			public static string GetWithdrawals (string btcAddress) { return $"/main/api/v2/mining/external/{btcAddress}/rigs/withdrawals"; }
			public static string GetRigs (string btcAddress) { return $"/main/api/v2/mining/external/{btcAddress}/rigs2"; }
		}

		public static class HashpowerPrivate {
			public const string myOrders = "/main/api/v2/hashpower/myOrders";
			public const string createOrder = "/main/api/v2/hashpower/order";
			public static string GetOrderDetails (string id) { return $"/main/api/v2/hashpower/order/{id}"; }
			public static string GetCancelOrder (string id) { return $"/main/api/v2/hashpower/order/{id}"; }
			public static string GetRefillOrder (string id) { return $"/main/api/v2/hashpower/order/{id}"; }
			public static string GetOrderStatistics (string id) { return $"/main/api/v2/hashpower/order/{id}"; }
			public static string GetUpdatePriceAndLimit (string id) { return $"/main/api/v2/hashpower/order/{id}"; }
			public const string estimateOrderDuration = "/main/api/v2/hashpower/calculateEstimateDuration";
		}

		public static class HashpowerPublic {
			public const string orderBook = "/main/api/v2/hashpower/orderBook";
			public const string fixedOrderPrice = "/main/api/v2/hashpower/orders/fixedPrice";
			public const string orderSummaries = "/main/api/v2/hashpower/orders/summaries";
			public const string orderSummary = "/main/api/v2/hashpower/orders/summary";
			public const string algorithmHistory = "/main/api/v2/public/algo/history";
			public const string buyInfo = "/main/api/v2/public/buy/info";
			public const string orders = "/main/api/v2/public/orders";
			public const string simpleStatus = "/main/api/v2/public/simplemultialgo/info";
			public const string statistics24h = "/main/api/v2/public/stats/global/24h";
			public const string currentStatistics = "/main/api/v2/public/stats/global/current";
		}

		public static class MinerPrivate {
			public const string groups = "/main/api/v2/mining/groups/list";
			public const string miningAddress = "/main/api/v2/mining/miningAddress";
			public const string rigAlgoStatistics = "/main/api/v2/mining/rig/stats/algo";
			public const string rigUnpaidStatistics = "/main/api/v2/mining/rig/stats/unpaid";
			public static string GetRiDetails (string rigID) { return $"/main/api/v2/mining/rig2{rigID}"; }
			public const string activeWorkers = "/main/api/v2/mining/rigs/activeWorkers";
			public const string payouts = "/main/api/v2/mining/rigs/payouts";
			public const string minerAlgoStatistics = "/main/api/v2/mining/rigs/stats/algo";
			public const string minerUnpaidStatistics = "/main/api/v2/mining/rigs/stats/unpaid";
			public const string rigAction = "/main/api/v2/mining/rigs/status2";
			public const string rigs = "/main/api/v2/mining/rigs2";
		}

		public static class Pools {
			public const string createOrEditPool = "/main/api/v2/pool";
			public static string GetPoolDetails (string poolId) { return $"/main/api/v2/pool/{poolId}"; }
			public static string DeletePool (string poolId) { return $"/main/api/v2/pool/{poolId}"; }
			public const string poolList = "/main/api/v2/pools";
			public const string verifyPool = "/main/api/v2/pools/verify";
		}

		public static class Public {
			public const string algorithms = "/main/api/v2/mining/algorithms";
			public const string markets = "/main/api/v2/mining/markets";
			public const string currencies = "/main/api/v2/public/currencies";
			public const string feeRules = "/main/api/v2/public/service/fee/info";
			public const string countries = "/api/v2/enum/countries";
			public const string kmCountries = "/api/v2/enum/kmCountries";
			public const string permissions = "/api/v2/enum/permissions";
			public const string xchCountries = "/api/v2/enum/xchCountries";
			public const string apiFlags = "/api/v2/system/flags";
			public const string time = "/api/v2/time";
		}

		public static class ExchangePrivate {
			public const string cancelAllOrders = "/exchange/api/v2/info/cancelOrders";
			public const string feeStatus = "/exchange/api/v2/info/fees/status";
			public const string myOrder = "/exchange/api/v2/info/myOrder";
			public const string myOrders = "/exchange/api/v2/info/myOrders";
			public const string myTrades = "/exchange/api/v2/info/myTrades";
			public const string orderTrades = "/exchange/api/v2/info/orderTrades";
			public const string createOrder = "/exchange/api/v2/order";
			public const string cancelOrder = "/exchange/api/v2/order";
		}

		public static class ExchangePublic {
			public const string candleSticks = "/exchange/api/v2/info/candlesticks";
			public const string marketStats = "/exchange/api/v2/info/marketStats";
			public const string prices = "/exchange/api/v2/info/prices";
			public const string status = "/exchange/api/v2/info/status";
			public const string trades = "/exchange/api/v2/info/trades";
			public const string orderbook = "/exchange/api/v2/orderbook";
		}

	}
}
