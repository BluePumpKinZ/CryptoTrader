using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Security.Cryptography;
using CryptoTrader.Keys;

namespace CryptoTrader.NicehashAPI.Utils {

	public static class NicehashSystem {

		public static long GetUTCTimeMillis () {
			return GetUTCTimeMillis (DateTime.Now);
		}

		public static long GetUTCTimeMillis (DateTime time) {
			int ms = time.Millisecond;
			long unixTime = ((DateTimeOffset)time).ToUnixTimeSeconds () * 1000 + ms;
			return unixTime;
		}

		public static string GenerateNonce () {
			char[] chars = new char[36];
			char[] charSet = "0123456789abcdef".ToCharArray ();
			byte[] randomData = new byte[18];
			RandomNumberGenerator.Fill (randomData);
			for (byte i = 0; i < 18; i++) {
				chars[(i << 1) | 1] = charSet[randomData[i] & 0x0f];
				chars[i << 1] = charSet[(randomData[i] & 0xf0) >> 4];
			}
			return new string (chars);
		}

		public static string GenerateDigest (string url, string time, string nonce) {

			string bodyStr = GetPath (url);
			string queryStr = GetQuery (url);

			List<string> segments = new List<string> ();
			segments.Add (KeyValues.ApiKey);
			segments.Add (time);
			segments.Add (nonce);
			segments.Add (null);
			segments.Add (KeyValues.OrganizationID);
			segments.Add (null);
			segments.Add ("GET");
			segments.Add (null);
			segments.Add (queryStr ?? null);

			if (bodyStr != null && bodyStr.Length > 0) {
				segments.Add (bodyStr);
			}
			return CalcHMACSHA256Hash (JoinSegments (segments), KeyValues.ApiKey);
		}

		public static string GetPath (string url) {
			return url.Split ('?')[0];
		}

		public static string GetQuery (string url) {
			var arrSplit = url.Split ('?');

			if (arrSplit.Length == 1) {
				return null;
			} else {
				return arrSplit[1];
			}
		}

		public static string JoinSegments (List<string> segments) {
			var sb = new System.Text.StringBuilder ();
			bool first = true;
			foreach (var segment in segments) {
				if (!first) {
					sb.Append ("\x00");
				} else {
					first = false;
				}

				if (segment != null) {
					sb.Append (segment);
				}
			}
			return sb.ToString ();
		}

		public static string CalcHMACSHA256Hash (string plaintext, string salt) {
			string result = "";
			var enc = Encoding.Default;
			byte[]
			baText2BeHashed = enc.GetBytes (plaintext),
			baSalt = enc.GetBytes (salt);
			HMACSHA256 hasher = new HMACSHA256 (baSalt);
			byte[] baHashedText = hasher.ComputeHash (baText2BeHashed);
			result = string.Join ("", baHashedText.ToList ().Select (b => b.ToString ("x2")).ToArray ());
			return result;
		}

	}
}
