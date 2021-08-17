using CryptoTrader.Exceptions;
using System;

namespace CryptoTrader.Inputs {

	public abstract class IInput {

		private protected Action function;

		public void ProcessInput (string command, string input) {
			if (!Process (ref input)) {
				Console.WriteLine (HelpTexts.GetHelpText(command));
				return;
			}
			Execute ();
		}

		internal static string GetNextSegment (ref string input) {
			try {
				if (!input.Contains (' ')) {
					string temp = input;
					input = "";
					return temp;
				}
				int nextSplit = input.IndexOf (' ');
				string output = input.Substring (0, nextSplit);
				input = input.Substring (nextSplit + 1);
				return output;
			} catch (IndexOutOfRangeException) {
				throw new OutOfArgumentsException ();
			}
		}

		private void Execute () {
			function.Invoke ();
		}

		private protected abstract bool Process (ref string input);

	}

}
