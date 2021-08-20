using CryptoTrader.NicehashAPI.JSONObjects;
using CryptoTrader.Utils;

namespace CryptoTrader.Algorithms {

	public class AlgoMedVar : Algorithm {

		private protected override void IterateInternal (PriceGraph graph, ref Balances balances) {

			long time = graph.GetLastTime ();
			long startTime = graph.GetStartTime ();

			long minimumTime = 1000L * 60 * 60 * 24 * 60; // 60 days

			if (time - startTime < minimumTime) {
				return;
			}

		}

		public override ICopyable Copy () {
			return CopyAbstractValues (new AlgoMedVar ());
		}

	}
}
