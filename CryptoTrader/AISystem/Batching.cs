using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoTrader.AISystem {

	public static class Batching {

		public static void GetBatchSplits (int size, int batches, out int[] markers, out int[] sizes) {
			if (batches > size)
				batches = size;
			markers = new int[batches];
			for (int i = 0; i < markers.Length; i++)
				markers[i] = i * size / batches;

			sizes = new int[batches];
			for (int i = 0; i < sizes.Length - 1; i++)
				sizes[i] = markers[i + 1] - markers[i];
			sizes[^1] = size - markers[^1];
		}

	}

}
