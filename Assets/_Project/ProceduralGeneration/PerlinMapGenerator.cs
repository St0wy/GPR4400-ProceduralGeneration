using System;
using MyBox;
using UnityEngine;

namespace ProcGen.ProceduralGeneration
{
	[Serializable]
	public class PerlinMapGenerator : IMapGenerator
	{
		/// <summary>
		/// The threshold between 1 and 0 at which a cell will be considered alive.
		/// </summary>

		[field: SerializeField, Range(0, 1)]
		public float Threshold { get; set; } = 0.5f;

		[field: SerializeField, Range(1, 10)] public float Scale { get; set; } = 1f;

		/// <summary>
		/// Gets or sets the origin of the perlin noise.
		/// </summary>
		[field: SerializeField, ReadOnly]
		public Vector2 Origin { get; set; }

		public CellStatus[,] GenerateMap(Size mapSize)
		{
			var world = new CellStatus[mapSize.Width, mapSize.Height];

			for (var x = 0; x < mapSize.Width; x++)
			{
				for (var y = 0; y < mapSize.Height; y++)
				{
					float xCoord = Origin.x + (float) x / mapSize.Width * Scale;
					float yCoord = Origin.y + (float) y / mapSize.Height * Scale;
					float sample = Mathf.PerlinNoise(xCoord, yCoord);
					if (sample > Threshold)
					{
						world[x, y] = CellStatus.Ground;
					}
					else
					{
						world[x, y] = CellStatus.Empty;
					}
				}
			}

			return world;
		}
	}
}