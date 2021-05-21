using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoTrader.Exceptions {

	public class HashNotFoundException : ArgumentException {

		public uint Hash { get; }

		public HashNotFoundException () : base () { }

		public HashNotFoundException (string message) : base (message) { }

		public HashNotFoundException (uint hash) : base () { Hash = hash; }

		public HashNotFoundException (string message, uint hash) : base (message) { Hash = hash; }

	}
}
