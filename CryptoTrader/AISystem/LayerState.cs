using CryptoTrader.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoTrader.AISystem {

	public class LayerState {

		private readonly double[] data;
		public int Size { get { return data.Length; } }

		public LayerState (int size) {
			if (size <= 0)
				throw new ArgumentException ("Size needs to be more than 0.");
			data = new double[size];
		}

		public LayerState (double[] data) {
			if (data.Length <= 0)
				throw new ArgumentException ("Length of data array needs to be more than 0.");
			this.data = data;
		}

		public void CheckIndex (int i) {
			if (i < 0 || i >= data.Length)
				throw new ArgumentException ("Index must be postive and less than the size of the array.");
		}

		public double GetNode (int i) {
			CheckIndex (i);
			return data[i];
		}

		public void SetNode (int i, double value) {
			CheckIndex (i);
			data[i] = value;
		}

		public double[] GetDataCopy () {
			return data.Copy ();
		}

		public double this[int index] {
			get {
				return GetNode (index);
			}
			set {
				SetNode (index, value);
			}
		}

		public static LayerState operator + (LayerState left, LayerState right) {
			if (left.Size != right.Size)
				throw new ArgumentException ("Both left and right operands must have the same size.");

			LayerState output = new LayerState (left.Size);
			for (int i = 0; i < output.Size; i++) {
				output[i] = left[i] + right[i];
			}
			return output;
		}

		public double Average () {
			double total = 0;
			Array.ForEach (data, t => total += t);
			return total / data.Length;
		}

		public double CalculateCost (LayerState other) {
			if (Size != other.Size)
				throw new ArgumentException ("outputs and desired output should have the same size");
			double loss = 0;
			for (int i = 0; i < Size; i++) {
				loss += MoreMath.Square (GetNode (i) - other.GetNode (i));
			}
			return loss / Size;
		}

		public static double CalculateCost (LayerState left, LayerState right) {
			return left.CalculateCost (right);
		}

	}

}
