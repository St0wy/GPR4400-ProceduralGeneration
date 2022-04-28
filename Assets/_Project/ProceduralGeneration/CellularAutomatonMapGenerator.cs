using System;
using Random = UnityEngine.Random;

namespace ProcGen.ProceduralGeneration
{
	[Serializable]
	public class CellularAutomatonMapGenerator : IMapGenerator
	{
		public CellStatus[,] GenerateMap(Size mapSize)
		{
			var cells = RandomFill(mapSize);
			return cells;
		}

		private static CellStatus[,] RandomFill(Size mapSize)
		{
			var cells = new CellStatus[mapSize.Width, mapSize.Height];

			for (var x = 0; x < mapSize.Width; x++)
			{
				for (var y = 0; y < mapSize.Height; y++)
				{
					cells[x, y] = (CellStatus) Random.Range(0, 2);
				}
			}

			return cells;
		}
	}
}