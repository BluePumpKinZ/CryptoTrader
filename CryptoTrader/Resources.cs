using System.IO;
using System.Linq;
using System.Reflection;

namespace CryptoTrader {

	public static class Resources {

		public static string ReadResource (string name) {
			Assembly assembly = Assembly.GetExecutingAssembly ();
			string resourceName = assembly.GetManifestResourceNames ().Single (str => str.EndsWith (name));

			using (Stream stream = assembly.GetManifestResourceStream (resourceName))
			using (StreamReader reader = new StreamReader (stream))
				return reader.ReadToEnd ();
		}

	}

}
