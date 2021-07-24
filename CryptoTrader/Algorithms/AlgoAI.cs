using CryptoTrader.AISystem;
using CryptoTrader.NicehashAPI.JSONObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoTrader.Algorithms {

	public class AlgoAI : Algorithm {

		public DeepLearningNetwork[] networks;

		internal override void IterateInternal (PriceGraph graph, ref Balances balances) {
			throw new NotImplementedException ();
		}

	}
}
