using connect;
using System;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;
using System.Text;

namespace CryptoTrader.NicehashAPI.JSONObjects {

	public class Balance : IParsable {

		public Currency Currency { private set; get; }
		public double Available { private set; get; }
		public double Pending { private set; get; }
		public double TotalBalance { get { return Available + Pending; } }
		public double BTCRate { private set; get; }

		public Balance (Currency c, double value, double btcRate) {
			Currency = c;
			Available = value;
			BTCRate = btcRate;
		}

		public Balance (string s) {
			Parse (s);
		}

		internal override void ParsePart (string key, string value) {
			switch (key) {
			case "currency":
				Currencies.TryGetCurrencyFromBTCPair (value, out Currency c);
				Currency = c;
				break;
			case "available":
				Available = double.Parse (value);
				break;
			case "pending":
				Pending = double.Parse (value);
				break;
			}
		}

		public double GetBTCValue () {
			return Available * BTCRate;
		}

		public override string ToString () {
			return $"Balance {Currencies.GetCurrencyToken (Currency)}\n\tTotal {TotalBalance}\n\tAvailable {Available}\n\tPending {Pending}";
		}
	}
}
