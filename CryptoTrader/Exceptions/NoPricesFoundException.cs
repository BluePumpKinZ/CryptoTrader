using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoTrader.Exceptions {

	public class NoPricesFoundException : SystemException {

		public NoPricesFoundException () : base () { }

		public NoPricesFoundException (string message) : base (message) { }

	}

}
