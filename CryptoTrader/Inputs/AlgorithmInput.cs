using CryptoTrader.AISystem;
using CryptoTrader.Algorithms;
using CryptoTrader.Exceptions;
using CryptoTrader.NicehashAPI;
using CryptoTrader.Utils;
using System;

namespace CryptoTrader.Inputs {

	internal class AlgorithmInput : IInput {

		private Currency algorithmCurrency = Currency.Null;
		private int epochs = 0;
		private int threads = 0;
		private bool autosaveAfterImprovement = false;

		private protected override bool Process (ref string input) {
			switch (GetNextSegment (ref input)) {
			case "list":
				switch (GetNextSegment (ref input)) {
				case "":
					function = () => Console.WriteLine (Trader.ListAlgorithms ());
					return true;
				case "types":
					function = () => {
						string[] algorithms = TypeMapping.GetAllDerivedTypes (typeof (Algorithm));
						Console.WriteLine ("Algorithm types:");
						foreach (string algo in algorithms) {
							bool isImprovable = typeof (IImprovableAlgorithm).IsAssignableFrom (Type.GetType ($"CryptoTrader.Algorithms.{algo}"));
							Console.WriteLine ($"  {algo}{(isImprovable ? " (Improvable)" : "")}");
						}
					};
					return true;
				}
				return false;
			case "test":
				if (ProcessArguments (ref input)) {
					if (algorithmCurrency == Currency.Null) {
						function = () => Console.WriteLine ("Please specify a currency for which to test the algorithm.");
						return false;
					}
					function = () => {
						Algorithm algoCopy = (Algorithm)Trader.GetAlgorithmForCurrency (algorithmCurrency).Copy ();
						PriceGraph graph = PriceWatcher.GetGraphForCurrency (algorithmCurrency);
						Console.WriteLine (algoCopy.ExecuteOnPriceGraph (graph));
					};
					return true;
				}
				return false;
			case "loss":
				if (ProcessArguments (ref input)) {
					if (algorithmCurrency == Currency.Null) {
						function = () => Console.WriteLine ("Please specify a currency for which to test the loss.");
						return false;
					}
					function = () => {
						if (Trader.GetImprovableAlgorithm (algorithmCurrency, out IImprovableAlgorithm algo))
							Console.WriteLine ($"Loss for algorithm '{Currencies.GetCurrencyToken (algorithmCurrency)}': {algo.GetLoss ()}");
					};
					return true;
				}
				return false;
			case "improve":
				if (ProcessArguments (ref input)) {
					if (algorithmCurrency == Currency.Null) {
						function = () => Console.WriteLine ("Please specify a currency for which to test the loss.");
						return false;
					}
					if (epochs == 0) {
						function = () => Console.WriteLine ("Please specify a number of epochs.");
						return false;
					}
					function = () => {
						if (Trader.GetImprovableAlgorithm (algorithmCurrency, out IImprovableAlgorithm algo))
							algo.Improve (epochs, threads == 0 ? AIProcessTaskScheduler.ThreadCount : threads, autosaveAfterImprovement);
					};
					return true;
				}
				return false;
			}
			return false;
		}

		private bool ProcessArguments (ref string input) {
			do {
				string arg = GetNextSegment (ref input);
				switch (arg) {
				case "-c":
					try {
						algorithmCurrency = Currencies.GetCurrencyFromToken (GetNextSegment (ref input));
					} catch (OutOfArgumentsException) {
						function = () => Console.WriteLine ("No argument was given for option '-c'.");
						return false;
					}
					break;
				case "-e":
					try {
						epochs = int.Parse (GetNextSegment (ref input));
					} catch (OutOfArgumentsException) {
						function = () => Console.WriteLine ("No argument was given for option '-e'.");
						return false;
					}
					break;
				case "-s":
					try {
						autosaveAfterImprovement = bool.Parse (GetNextSegment (ref input));
					} catch (OutOfArgumentsException) {
						function = () => Console.WriteLine ("No argument was given for option '-s'.");
						return false;
					}
					break;
				default:
					function = () => Console.WriteLine ($"Option '{arg}' not recognized.");
					return false;
				}
			} while (input.Length > 0);
			return true;
		}

	}

}
