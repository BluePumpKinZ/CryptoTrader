using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoTrader.Exceptions {

	public class OutOfArgumentsException : SystemException {

		public OutOfArgumentsException () : base () { }

		public OutOfArgumentsException (string message) : base (message) { }

	}

}
