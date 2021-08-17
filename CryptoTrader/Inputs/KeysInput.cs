using CryptoTrader.Keys;
using System;

namespace CryptoTrader.Inputs {

	internal class KeysInput : IInput {

		private protected override bool Process (ref string input) {
			switch (GetNextSegment (ref input)) {
			case "read":
				if (input == "") {
					function = () => KeyValues.ReadKeys ();
					return true;
				}
				switch (GetNextSegment (ref input)) {
				case "set":
					string name = KeyValues.LoadedKeySet;
					string apikey = KeyValues.ApiKey;
					string apisecret = KeyValues.ApiSecret;
					string orgid = KeyValues.OrganizationID;
					function = () => Console.WriteLine ($"Loaded key set: {name}\nAPIKey: {apikey}\nAPISecret: {apisecret}\nOrgID: {orgid}");
					return true;
				case "name":
					function = () => Console.WriteLine ("Loaded key set: " + KeyValues.LoadedKeySet);
					return true;
				case "apikey":
					function = () => Console.WriteLine ("Loaded api key: " + KeyValues.ApiKey);
					return true;
				case "apisecret":
					function = () => Console.WriteLine ("Loaded api secret: " + KeyValues.ApiSecret);
					return true;
				case "orgid":
					function = () => Console.WriteLine ("Loaded orginization id: " + KeyValues.OrganizationID);
					return true;
				}
				return false;
			case "select":
				string keyset = GetNextSegment (ref input);
				function = () => KeyValues.SelectKeySet (keyset);
				return true;
			}


			return false;
		}

	}

}
