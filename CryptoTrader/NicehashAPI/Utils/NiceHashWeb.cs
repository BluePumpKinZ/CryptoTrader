using CryptoTrader.Keys;
using System;
using System.Collections.Generic;

namespace CryptoTrader.NicehashAPI.Utils {

	public static class NicehashWeb {

		public static string Get (string url) {
			return Get (url, false, null);
		}

		public static string Get (string url, bool auth) {
			return Get (url, auth, NicehashSystem.GetUTCTimeMillis ().ToString ());
		}

		public static string Get (string url, bool auth, string time) {
			var client = new RestSharp.RestClient (NicehashURLs.root);
			var request = new RestSharp.RestRequest (url);

			if (auth) {
				string nonce = NicehashSystem.GenerateNonce ();
				string digest = HashBySegments (KeyValues.ApiSecret, KeyValues.ApiKey, time, nonce, KeyValues.OrganizationID, "GET", NicehashSystem.GetPath (url), NicehashSystem.GetQuery (url), null);

				request.AddHeader ("X-Time", time);
				request.AddHeader ("X-Nonce", nonce);
				request.AddHeader ("X-Auth", KeyValues.ApiKey + ":" + digest);
				request.AddHeader ("X-Organization-Id", KeyValues.OrganizationID);
			}

			var response = client.Execute (request, RestSharp.Method.GET);
			var content = response.Content;
			if (content == "")
				throw new OperationCanceledException ("A connection to the server cannot be made.");
			return content;
		}

		public static string Post (string url, string payload, string time, bool requestId) {
			var client = new RestSharp.RestClient (NicehashURLs.root);
			var request = new RestSharp.RestRequest (url);
			request.AddHeader ("Accept", "application/json");
			request.AddHeader ("Content-type", "application/json");

			string nonce = NicehashSystem.GenerateNonce ();
			string digest = HashBySegments (KeyValues.ApiSecret, KeyValues.ApiKey, time, nonce, KeyValues.OrganizationID, "POST", NicehashSystem.GetPath (url), NicehashSystem.GetQuery (url), payload ?? "");

			if (payload != null) {
				request.AddJsonBody (payload);
			}

			request.AddHeader ("X-Time", time);
			request.AddHeader ("X-Nonce", nonce);
			request.AddHeader ("X-Auth", KeyValues.ApiKey + ":" + digest);
			request.AddHeader ("X-Organization-Id", KeyValues.OrganizationID);

			if (requestId) {
				request.AddHeader ("X-Request-Id", Guid.NewGuid ().ToString ());
			}

			var response = client.Execute (request, RestSharp.Method.POST);
			var content = response.Content;
			if (content == "")
				throw new OperationCanceledException ("A connection to the server cannot be made.");
			return content;
		}

		public static string Delete (string url, string time, bool requestId) {
			var client = new RestSharp.RestClient (NicehashURLs.root);
			var request = new RestSharp.RestRequest (url);

			string nonce = NicehashSystem.GenerateNonce ();
			string digest = HashBySegments (KeyValues.ApiSecret, KeyValues.ApiKey, time, nonce, KeyValues.OrganizationID, "DELETE", NicehashSystem.GetPath (url), NicehashSystem.GetQuery (url), null);

			request.AddHeader ("X-Time", time);
			request.AddHeader ("X-Nonce", nonce);
			request.AddHeader ("X-Auth", KeyValues.ApiKey + ":" + digest);
			request.AddHeader ("X-Organization-Id", KeyValues.OrganizationID);

			if (requestId) {
				request.AddHeader ("X-Request-Id", Guid.NewGuid ().ToString ());
			}

			var response = client.Execute (request, RestSharp.Method.DELETE);
			var content = response.Content;
			if (content == "")
				throw new OperationCanceledException ("A connection to the server cannot be made.");
			return content;
		}

		private static string HashBySegments (string key, string apiKey, string time, string nonce, string orgId, string method, string encodedPath, string query, string bodyStr) {
			List<string> segments = new List<string> ();
			segments.Add (apiKey);
			segments.Add (time);
			segments.Add (nonce);
			segments.Add (null);
			segments.Add (orgId);
			segments.Add (null);
			segments.Add (method);
			segments.Add (encodedPath == null ? null : encodedPath);
			segments.Add (query == null ? null : query);

			if (bodyStr != null && bodyStr.Length > 0) {
				segments.Add (bodyStr);
			}
			return NicehashSystem.CalcHMACSHA256Hash (NicehashSystem.JoinSegments (segments), key);
		}

	}
}
