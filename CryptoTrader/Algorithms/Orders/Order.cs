using CryptoTrader.NicehashAPI;
using CryptoTrader.NicehashAPI.JSONObjects;
using CryptoTrader.Utils;

namespace CryptoTrader.Algorithms.Orders {

	public abstract class Order : ICopyableAbstract {

		public Currency Currency { protected set; get; }
		public double Value { protected set; get; }
		public long Time { protected set; get; }

		public bool IsBuyOrder { get { return IsBuyOrderInternal (); } }
		public bool IsSellOrder { get { return IsSellOrderInternal (); } }
		public bool IsMarketOrder { get { return IsMarketOrderInternal (); } }
		public bool IsLimitOrder { get { return IsLimitOrderInternal (); } }

		private protected abstract bool IsBuyOrderInternal ();
		private protected abstract bool IsSellOrderInternal ();
		private protected abstract bool IsMarketOrderInternal ();
		private protected abstract bool IsLimitOrderInternal ();

		public abstract bool HasSufficientFunds (Balances balances);

		public virtual string GetOrderUrl () {
			string market = Currencies.GetPair (Currency);
			string side = IsBuyOrder ? "BUY" : "SELL";
			string type = IsMarketOrder ? "MARKET" : "LIMIT";
			return $"?market={market}&side={side}&type={type}&quantity={NumberFormatting.FormatAmount (Value, Currency)}";
		}

		public ICopyable CopyAbstractValues (ICopyable copy) {
			Order order = (Order)copy;
			order.Currency = Currency;
			order.Value = Value;
			order.Time = Time;
			return order;
		}

		public abstract ICopyable Copy ();
	}

}
