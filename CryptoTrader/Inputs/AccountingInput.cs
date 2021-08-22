using CryptoTrader.NicehashAPI;
using System;

namespace CryptoTrader.Inputs {

	internal class AccountingInput : IInput {

		private protected override bool Process (ref string input) {

			switch (GetNextSegment (ref input)) {
			case "read":
				switch (GetNextSegment (ref input)) {
				case "balances":
					function = () => Console.WriteLine (Accounting.GetBalances ());
					return true;
				}
				return false;
			case "update":
				switch (GetNextSegment (ref input)) {
				case "balances":
					function = () => Trader.UpdateBalances ();
					return true;
				}
				return false;
			}
			return false;
		}

	}

}
