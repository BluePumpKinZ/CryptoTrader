using CryptoTrader.AISystem;
using CryptoTrader.Algorithms;
using CryptoTrader.Exceptions;
using CryptoTrader.NicehashAPI;
using CryptoTrader.Utils;
using System;
using System.Linq;

namespace CryptoTrader.Inputs {

	internal class AlgorithmInput : IInput {

		private Currency algorithmCurrency = Currency.Null;
		private int epochs = 0;
		private int threads = 0;
		private bool autosaveAfterImprovement = false;
		private string algorithmType = "";
		private double assignRatio = -1;

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
			case "load":
				function = () => Trader.LoadAlgorithms ();
				return true;
			case "test":
				if (ProcessArguments (ref input)) {
					if (algorithmCurrency == Currency.Null) {
						function = () => Console.WriteLine ("Please specify a currency.");
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
						function = () => Console.WriteLine ("Please specify a currency.");
						return false;
					}
					function = () => {
						if (Trader.GetImprovableAlgorithm (algorithmCurrency, out IImprovableAlgorithm algo))
							Console.WriteLine ($"Loss for algorithm '{Currencies.GetCurrencyToken (algorithmCurrency)}': {algo.GetLoss ()}");
					};
					return true;
				}
				return false;
			case "enable":
				if (input.Length == 0) {
					function = () => Trader.EnableTrading ();
					return true;
				}
				if (ProcessArguments (ref input)) {
					if (algorithmCurrency == Currency.Null) {
						function = () => Console.WriteLine ("Please specify a currency.");
						return false;
					}
					function = () => Trader.EnableAlgorithm (algorithmCurrency);
					return true;
				}
				return false;
			case "disable":
				if (input.Length == 0) {
					function = () => Trader.DisableTrading ();
					return true;
				}
				if (ProcessArguments (ref input)) {
					if (algorithmCurrency == Currency.Null) {
						function = () => Console.WriteLine ("Please specify a currency.");
						return false;
					}
					function = () => Trader.DisableAlgorithm (algorithmCurrency);
					return true;
				}
				return false;
			case "improve":
				if (ProcessArguments (ref input)) {
					if (algorithmCurrency == Currency.Null) {
						function = () => Console.WriteLine ("Please specify a currency.");
						return false;
					}
					if (epochs == 0) {
						function = () => Console.WriteLine ("Please specify the number of epochs.");
						return false;
					}
					function = () => {
						Trader.ImproveImprovableAlgorithm (algorithmCurrency, epochs, threads == 0 ? AIProcessTaskScheduler.ThreadCount : threads, autosaveAfterImprovement);
					};
					return true;
				}
				return false;
			case "assign":
				if (ProcessArguments (ref input)) {
					if (algorithmCurrency == Currency.Null) {
						function = () => Console.WriteLine ("Please specify a currency.");
						return false;
					}
					if (assignRatio == -1) {
						function = () => Console.WriteLine ("Please specify an assignment ratio.");
						return false;
					}
					function = () => Trader.AssignRatio (algorithmCurrency, assignRatio);
					return true;
				}
				return false;
			case "add":
				if (ProcessArguments (ref input)) {
					if (algorithmCurrency == Currency.Null) {
						function = () => Console.WriteLine ("Please specify a currency.");
						return false;
					}
					if (algorithmType == "") {
						function = () => Console.WriteLine ("Please specify an algorithm type.");
						return false;
					}
					function = () => {
						Algorithm algorithm = TypeMapping.AlgorithmFromName (algorithmType);
						algorithm.SetPrimaryCurrency (algorithmCurrency);
						algorithm.IsTraining = true;
						Trader.AddAlgorithm (algorithm);
					};
					return true;
				}
				return false;
			case "delete":
				if (ProcessArguments (ref input)) {
					if (algorithmCurrency == Currency.Null) {
						function = () => Console.WriteLine ("Please specify a currency.");
						return false;
					}
					function = () => Trader.DeleteAlgorithm (algorithmCurrency);
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
				case "-a":
					try {
						string type = GetNextSegment (ref input);
						if (TypeMapping.GetAllDerivedTypes (typeof (Algorithm)).Contains (type))
							algorithmType = type;
						else
							function = () => Console.WriteLine ($"{type} is not a valid algorithm. For a full list execute 'algorithms list types'.");
					} catch (OutOfArgumentsException) {
						function = () => Console.WriteLine ("No argument was given for option '-a'.");
						return true;
					}
					break;
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
				case "-t":
					try {
						threads = int.Parse (GetNextSegment (ref input));
					} catch (OutOfArgumentsException) {
						function = () => Console.WriteLine ("No argument was given for option '-t'.");
						return false;
					}
					break;
				case "-s":
					autosaveAfterImprovement = true;
					break;
				case "-r":
					try {
						assignRatio = double.Parse (GetNextSegment (ref input).Replace (".", ","));
						if (assignRatio < 0 || assignRatio > 1) {
							function = () => Console.WriteLine ("Ratio should be in range (0..1).");
							return false;
						}
					} catch (OutOfArgumentsException) {
						function = () => Console.WriteLine ("No argument was given for option '-r'.");
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
