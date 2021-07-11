using System;

namespace CryptoTrader.Exceptions {

	public class NoPricesFoundException : SystemException {

		public NoPricesFoundException () : base () { }

		public NoPricesFoundException (string message) : base (message) { }

	}

}
