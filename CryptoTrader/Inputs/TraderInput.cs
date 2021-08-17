namespace CryptoTrader.Inputs {

	internal class TraderInput : IInput {

		private protected override bool Process (ref string input) {

			switch (GetNextSegment (ref input)) {
			case "start":
				function = () => Trader.Start ();
				return true;
			case "stop":
				function = () => Trader.Stop ();
				return true;
			case "save":
				function = () => Trader.Save ();
				return true;
			case "stopandsave":
				function = () => Trader.StopAndSave ();
				return true;
			case "status":
				function = () => Trader.GetStatusPrintOut ();
				return true;
			}

			return false;
		}

	}

}
