using CryptoTrader.NicehashAPI;
using System;

namespace CryptoTrader.Inputs {

	internal class ExchangePublicInput : IInput {

		private protected override bool Process (ref string input) {

			switch (GetNextSegment (ref input)) {
			case "read":

				switch (GetNextSegment (ref input)) {
				case "prices":
					function = () => Console.WriteLine (ExchangePublic.GetPrices ());
					return true;
				}
				return false;


			}

			return false;
		}

	}

}
