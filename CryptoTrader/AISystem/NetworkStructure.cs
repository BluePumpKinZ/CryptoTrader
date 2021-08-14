using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoTrader.AISystem {

	public class NetworkStructure {

		public int Size { get { return structure.Length; } }
		private int[] structure;

		public NetworkStructure (int[] structure) {
			if (structure == null || structure.Length == 0)
				throw new ArgumentException ("Structure cannot be null and must have more than one layer.");
			this.structure = structure;
		}

		public NetworkLayer[] GetNetworkLayers () {
			NetworkLayer[] layers = new NetworkLayer[Size - 1];
			for (int i = 0; i < layers.Length; i++) {
				layers[i] = new NetworkLayer (structure[i], structure[i + 1]);
			}
			return layers;
		}

		public int GetSizeAtLayer (int index) {
			return structure[index];
		}

		public int this[int index] {
			get {
				return GetSizeAtLayer (index);
			}
		}

	}

}
