using CryptoTrader.AISystem;

namespace CryptoTrader.Algorithms {

	public interface IImprovableAlgorithm {

		public abstract void Improve (int epochs, int threads, bool autoSave);

		public abstract double GetLoss ();

	}

}
