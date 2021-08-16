using CryptoTrader.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoTrader {

	public abstract class IStorable {

		public abstract void LoadFromBytes (ref int index, byte[] data);

		public abstract void SaveToBytes (ref List<byte> datalist);

		private protected byte[] GetDataRange (ref int index, byte[] data) {
			int length = BitConverter.ToInt32 (data, 0);
			index += 4;
			byte[] newData = data.GetRange (index + 4, length);
			index += length;
			return newData;
		}

		private protected void AddData (ref List<byte> datalist, byte[] data) {
			int length = data.Length;
			datalist.AddRange (BitConverter.GetBytes (length));
			datalist.AddRange (data);
		}

	}

}
