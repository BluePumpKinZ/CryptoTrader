using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoTrader {
	public static class MoreMath {

		public static double InverseLerp (long min, long max, long value) {
			return 1.0 * (value - min) / (max - min);
		}

		public static double Lerp (double min, double max, double t) {
			return (1 - t) * min + t * max;
		}

	}
}
