﻿using CryptoTrader.AISystem;

namespace CryptoTrader.Algorithms {

	public interface IImprovableAlgorithm {

		public abstract void Improve (int epochs, int threads);

		public abstract double GetLoss (LayerState input, LayerState desiredOutputs);

	}

}