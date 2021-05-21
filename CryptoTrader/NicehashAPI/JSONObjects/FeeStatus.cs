namespace CryptoTrader.NicehashAPI.JSONObjects {
	public class FeeStatus : IParsable {

		public double MakerCoefficient { private set; get; }
		public double TakerCoefficient { private set; get; }
		public double Sum { private set; get; }
		public FeeLimits MakerLimits { private set; get; }
		public FeeLimits TakerLimits { private set; get; }
		public Currency Currency { private set; get; }

		public FeeStatus (string s) {
			Parse (s);
		}

		internal override void ParsePart (string key, string value) {
			switch (key) {
			case "makerCoefficient":
				MakerCoefficient = double.Parse (value);
				break;
			case "takerCoefficient":
				TakerCoefficient = double.Parse (value);
				break;
			case "sum":
				Sum = double.Parse (value);
				break;
			case "makerLimits":
				MakerLimits = new FeeLimits (value);
				break;
			case "takerLimits":
				TakerLimits = new FeeLimits (value);
				break;
			case "currency":
				Currency = Currencies.GetCurrencyFromToken (value);
				break;
			}
		}
	}
}
