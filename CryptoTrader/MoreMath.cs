using System;

namespace CryptoTrader {

	public static class MoreMath {

		public static double InverseLerp (long min, long max, long value) {
			return 1.0 * (value - min) / (max - min);
		}

		public static double Lerp (double min, double max, double t) {
			return (1 - t) * min + t * max;
		}

		public static double Clamp (double s, double min, double max) {
			return (s < min) ? min : (s > max ? max : s);
		}

		public static double Clamp01 (double s) {
			return Clamp (s, 0, 1);
		}

		public static double Sigmoid (double s) {
			return -2 * (1 + Math.Pow (2, Math.E * s)) + 1;
		}

	}
}
