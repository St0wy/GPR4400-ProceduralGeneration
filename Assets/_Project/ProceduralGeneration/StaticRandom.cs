using System;
using System.Threading;

namespace ProcGen.ProceduralGeneration
{
	/// <summary>
	/// A thread safe random based on this post : https://stackoverflow.com/a/19271062/12330678
	/// </summary>
	public static class StaticRandom
	{
		private static int seed = Environment.TickCount;

		private static readonly ThreadLocal<Random> Random = new(() => new Random(Interlocked.Increment(ref seed)));

		public static void InitState(int newSeed)
		{
			seed = newSeed;
		}
		
		public static int Rand()
		{
			return Random.Value.Next();
		}

		/// <summary>
		/// Returns a random floating-point number that is greater than or equal to 0.0, and less than 1.0.
		/// </summary>
		public static double NextDouble()
		{
			return Random.Value.NextDouble();
		}

		/// <summary>
		/// Gets a random value between min (inclusive) and max (exclusive).
		/// </summary>
		/// <param name="min">Inclusive min number.</param>
		/// <param name="max">Exclusive max number.</param>
		/// <returns>A random value between min and max.</returns>
		public static int Next(int min, int max)
		{
			return Random.Value.Next(min, max);
		}
	}
}