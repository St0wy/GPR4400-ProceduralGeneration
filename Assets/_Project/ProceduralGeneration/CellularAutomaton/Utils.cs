using System;
using System.Collections.Generic;

namespace ProcGen.ProceduralGeneration.CellularAutomaton
{
	public static class Utils
	{
		public static int MaxIndex<T>(this IEnumerable<T> source)
		{
			IComparer<T> comparer = Comparer<T>.Default;
			using var iterator = source.GetEnumerator();
			if (!iterator.MoveNext())
			{
				throw new InvalidOperationException("Empty sequence");
			}

			var maxIndex = 0;
			T maxElement = iterator.Current;
			var index = 0;
			while (iterator.MoveNext())
			{
				index++;
				T element = iterator.Current;
				if (comparer.Compare(element, maxElement) <= 0) continue;
				maxElement = element;
				maxIndex = index;
			}

			return maxIndex;
		}
	}
}