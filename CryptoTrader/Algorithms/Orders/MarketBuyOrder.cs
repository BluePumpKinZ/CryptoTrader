﻿using CryptoTrader.Exceptions;
using CryptoTrader.NicehashAPI;
using CryptoTrader.NicehashAPI.JSONObjects;

namespace CryptoTrader.Algorithms.Orders {

	public class MarketBuyOrder : MarketOrder {

		protected override bool IsBuyOrderInternal () {
			return true;
		}

		protected override bool IsSellOrderInternal () {
			return false;
		}

		protected override bool IsMarketOrderInternal () {
			return true;
		}

		protected override bool IsLimitOrderInternal () {
			return false;
		}

		public override bool HasSufficientFunds (Balances balances) {
			Balance btcBalance = balances.GetBalanceForCurrency (Currency.Bitcoin);
			try {
				Balance balance = balances.GetBalanceForCurrency (Currency);
				return balance.BTCRate * Value <= btcBalance.Available;
			} catch (NoPricesFoundException) {
				// Just returning false because if the balance can't be found there will be no available currency anyway
				return false;
			}
		}

		public MarketBuyOrder (Currency currency, double value) {
			Currency = currency;
			Value = value;
		}
	}

}
