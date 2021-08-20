using System;
using System.Collections.Generic;

namespace CryptoTrader.Inputs {

	public static class Input {

		private readonly static Dictionary<string, string> inputProcessors = new Dictionary<string, string> () {
			{ "keys", "Keys" },
			{ "trader", "Trader" },
			{ "accounting", "Accounting" },
			{ "exchangepublic", "ExchangePublic" },
			{ "algorithms", "Algorithm" }
		};

		public static void ProcessInput (string input) {
			input = input.Trim ();
			if (input == "")
				return;
			string command = IInput.GetNextSegment (ref input);
			if (inputProcessors.TryGetValue (command, out string inputProcessorName)) {
				IInput inputProcessor = Activator.CreateInstance (Type.GetType ($"CryptoTrader.Inputs.{inputProcessorName}Input")) as IInput;
				inputProcessor.ProcessInput (command, input);
				return;
			}
			Console.WriteLine ($"Command '{command}' not recognized.");
		}

	}

}
