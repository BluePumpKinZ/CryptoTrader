using System;
using System.Collections.Concurrent;
using System.Threading;

namespace CryptoTrader.AISystem {

	public static class AIProcessTaskScheduler {

		public static ConcurrentQueue<Action> tasks = new ConcurrentQueue<Action> ();
		private static bool stopWorkerThreads = true;
		private static bool hasJoined = false;
		private static Thread[] workerThreads;
		public static int ThreadCount { private set; get; } = 1;

		public static void RunOnThread (Action action) {
			new Thread (new ThreadStart (action)).Start ();
		}

		public static void StartExecuting () {
			stopWorkerThreads = false;
			hasJoined = false;
			workerThreads = new Thread[ThreadCount];
			ThreadStart threadStart = new ThreadStart (delegate { WorkerThread (); });
			for (int i = 0; i < ThreadCount; i++) {
				workerThreads[i] = new Thread (threadStart);
				workerThreads[i].Start ();
			}
		}

		public static void SetThreadCount (int threads) {
			bool running = !stopWorkerThreads;
			int min = 1;
			int max = Environment.ProcessorCount;
			if (min > threads || threads > max) {
				throw new ArgumentException ($"Couldn't set threadcount: number of threads should be between {min} and {max}.");
			}
			if (running) {
				StopExecuting ();
				FinishTasks ();
				ThreadCount = threads;
				StartExecuting ();
			} else
				ThreadCount = threads;
		}

		public static void StopExecuting () {
			stopWorkerThreads = true;
		}

		/// <summary>
		/// Joins the worker thread(s) until all tasks have been completed
		/// </summary>
		public static void FinishTasks () {
			try {
				hasJoined = true;
				Array.ForEach (workerThreads, (workerThread) => workerThread.Join ());
			} catch (NullReferenceException) { } finally {
				hasJoined = false;
			}
		}

		public static void AddTask (Action action) {
			tasks.Enqueue (action);
		}

		private static void WorkerThread () {
			while (!stopWorkerThreads) {
				while (tasks.TryDequeue (out Action task)) {
					task.Invoke ();
					if (stopWorkerThreads)
						return;
				}
				if (hasJoined)
					return;
				Thread.Sleep (1);
			}
		}
	}

}
