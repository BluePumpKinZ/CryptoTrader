using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace CryptoTrader.Keys {

	public static class KeyValues {

		public static string LoadedKeySet { private set; get; }
		public static string ApiKey { private set; get; }
		public static string ApiSecret { private set; get; }
		public static string OrganizationID { private set; get; }

		private static string keyPath;
		private static Dictionary<string, KeySet> keySets;

		public static void SetPath (string path) {
			keyPath = path;
			string folderPath = Path.GetDirectoryName (keyPath);
			if (!Directory.Exists (folderPath))
				throw new DirectoryNotFoundException ($"The directory '{folderPath}' could not be found");
		}

		public static void ReadKeys () {
			string data = File.ReadAllText (keyPath);
			// data = data.Replace ("\t\r ", "");
			data = Regex.Replace (data, "[\\t\\r ]","");
			keySets = new Dictionary<string, KeySet> ();
			foreach (string keySet in data.Split ('\n')) {
				string[] split = keySet.Split (':');

				string keySetName = split[0];
				string keySetApiKey = split[1];
				string keySetApiSecret = split[2];
				string keySetOrganizationID = split[3];

				KeySet set = new KeySet (keySetApiKey, keySetApiSecret, keySetOrganizationID);
				if (!set.HasProperFormat ())
					continue;
				keySets.Add (keySetName, set);
			}
		}

		public static void SelectKeySet (string keyName) {
			if (keySets.TryGetValue (keyName, out KeySet set)) {
				LoadedKeySet = keyName;
				ApiKey = set.ApiKey;
				ApiSecret = set.ApiSecret;
				OrganizationID = set.OrganizationID;
				return;
			}
			throw new ArgumentException ($"The keyset \"{keyName}\" could not be found.");
		}

		protected struct KeySet {

			public string ApiKey { private set; get; }
			public string ApiSecret { private set; get; }
			public string OrganizationID { private set; get; }

			public KeySet (string apiKey, string apiSecret, string organizationID) {
				ApiKey = apiKey;
				ApiSecret = apiSecret;
				OrganizationID = organizationID;
			}

			public bool HasProperFormat () {
				if (ApiKey.Length != 36)
					return false;
				if (ApiSecret.Length != 72)
					return false;
				if (OrganizationID.Length != 36)
					return false;
				return Regex.Replace (ApiKey + ApiSecret + OrganizationID, "[0-9a-f-]", "").Length == 0;
			}

			public override string ToString () {
				return $"APIKey: {ApiKey}\nAPISecret: {ApiSecret}\nOrgID: {OrganizationID}";
			}

		}



	}

}
