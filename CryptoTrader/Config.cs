using CryptoTrader.NicehashAPI.JSONObjects;
using System;
using System.IO;

namespace CryptoTrader {

	public class Config : IParsable {

		public const string CONFIG_PATH = "cryptotrader.conf";

		public string AlgorithmPath { private set; get; } = "";
		public string PricewatcherPath { private set; get; } = "";
		public string KeyPath { private set; get; } = "";
		public string KeySet { private set; get; } = "";
		public int MaxThreads { private set; get; } = Environment.ProcessorCount;
		public bool AutoStart { private set; get; } = false;

		public Config () {
			if (!File.Exists (CONFIG_PATH)) {
				Console.WriteLine ($"Could not load prices at {Directory.GetCurrentDirectory()}\\{CONFIG_PATH}");
				Environment.Exit (-1);
			}

			string configtext = File.ReadAllText (CONFIG_PATH);

			Parse (configtext);
		}

		internal override void ParsePart (string key, string value) {
			switch (key) {
			case "algorithmpath":
				AlgorithmPath = value;
				break;
			case "pricewatcherpath":
				PricewatcherPath = value;
				break;
			case "keypath":
				KeyPath = value;
				break;
			case "keyset":
				KeySet = value;
				break;
			case "maxthreads":
				if (int.TryParse (value, out int intValue))
					MaxThreads = intValue;
				break;
			case "autostart":
				if (bool.TryParse (value, out bool boolValue))
					AutoStart = boolValue;
				break;
			default:
				Console.WriteLine ($"No settings found for '{key}' and value '{value}'");
				break;
			}
		}
	}

}
