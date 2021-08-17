using System;
using System.Collections.Generic;

namespace CryptoTrader.AISystem {

	public class NetworkStructure : IStorable {

		public int Size { get { return structure.Length; } }
		private int[] structure;

		public NetworkStructure () {
			structure = new int[0];
		}

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

		public void LoadFromBytes (ref int index, byte[] data) {
			int length = BitConverter.ToInt32 (IStorable.GetDataRange (ref index, data));
			structure = new int[length];
			for (int i = 0; i < length; i++)
				structure[i] = BitConverter.ToInt32 (IStorable.GetDataRange (ref index, data));
		}

		public void SaveToBytes (ref List<byte> datalist) {
			int length = structure.Length;
			IStorable.AddData (ref datalist, BitConverter.GetBytes (length));
			for (int i = 0; i < length; i++)
				IStorable.AddData (ref datalist, BitConverter.GetBytes (structure[i]));
		}

		public int this[int index] {
			get {
				return GetSizeAtLayer (index);
			}
		}

		public int this[Index index] {
			get {
				return structure[index];
			}
		}

	}

}
