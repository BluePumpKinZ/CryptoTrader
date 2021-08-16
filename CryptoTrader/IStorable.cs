﻿using CryptoTrader.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoTrader {

	public interface IStorable {

		public abstract void LoadFromBytes (ref int index, byte[] data);

		public abstract void SaveToBytes (ref List<byte> datalist);

		internal static byte[] GetDataRange (ref int index, byte[] data) {
			int length = DecompressInt (ref index, data);
			byte[] newData = data.GetRange (index, length);
			index += length;
			return newData;
		}

		internal static void AddData (ref List<byte> datalist, byte[] data) {
			int length = data.Length;
			datalist.AddRange (CompressInt (length));
			datalist.AddRange (data);
		}

		internal static byte[] CompressInt (int data) {

			if (data < 0)
				throw new ArgumentException ("Data must be positive");

			data &= 0x7FFF_FFFF;

			List<byte> newData = new List<byte> ();
			do {
				byte nextByte = BitConverter.GetBytes (data)[0];
				byte mainPart = (byte)(nextByte & 0x7F);
				byte carryPart = (byte)(newData.Count == 0 ? 0 : 0x80);
				newData.Add ((byte)(mainPart | carryPart));
				data >>= 7;
			} while (data > 0);

			newData.Reverse ();
			return newData.ToArray ();
		}

		internal static int DecompressInt (ref int index, byte[] data) {
			int newInt = 0;
			int i = 0;
			do {
				newInt += data[i] & 0x7F;
				data[i] >>= 7;
				index++;
				if (data[i] == 1)
					newInt <<= 7;
			} while (data[i++] == 1);
			return newInt;
		}

	}

}
