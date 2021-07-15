using CryptoTrader.NicehashAPI;
using CryptoTrader.NicehashAPI.JSONObjects;

namespace CryptoTrader.Algorithms.Orders {

	public abstract class Order {

		public Currency Currency { protected set; get; }
		public double Value { protected set; get; }

		public bool IsBuyOrder { get { return IsBuyOrderInternal (); } }
		public bool IsSellOrder { get { return IsSellOrderInternal (); } }
		public bool IsMarketOrder { get { return IsMarketOrderInternal (); } }
		public bool IsLimitOrder { get { return IsLimitOrderInternal (); } }

		protected abstract bool IsBuyOrderInternal ();
		protected abstract bool IsSellOrderInternal ();
		protected abstract bool IsMarketOrderInternal ();
		protected abstract bool IsLimitOrderInternal ();

		public abstract bool HasSufficientFunds (Balances balances);

		public virtual string GetOrderUrl () {
			string market = Currencies.GetPair (Currency);
			string side = IsBuyOrder ? "BUY" : "SELL";
			string type = IsMarketOrder ? "MARKET" : "LIMIT";
			return $"?market={market}&side={side}&type={type}&quantity={Value}";
		}

	}

}
