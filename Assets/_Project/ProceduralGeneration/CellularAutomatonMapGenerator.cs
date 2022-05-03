using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyBox;
using UnityEngine;

namespace ProcGen.ProceduralGeneration
{
	public enum FloodFillBehaviour
	{
		KeepOnlyBiggestIsland,
		RemoveSmallIslands,
	}

	[Serializable]
	public class CellularAutomatonMapGenerator : IMapGenerator
	{
		[OverrideLabel("Number of Generations")] [SerializeField]
		private int nbrGenerations = 50;

		[Range(0, 1)] [SerializeField] private double chanceToBeGround = 0.55d;

		[SerializeField] private FloodFillBehaviour floodFillBehaviour;

		[ConditionalField(
			nameof(floodFillBehaviour),
			false,
			FloodFillBehaviour.RemoveSmallIslands)]
		[SerializeField]
		private int smallIslandSize = 30;

		public CellStatus[,] GenerateMap(Size mapSize)
		{
			var cells = RandomFill(mapSize);
			cells = ComputeAutomaton(cells);
			cells = RemoveSmallIslands(cells);
			return cells;
		}

		/// <summary>
		/// Computes the cellular automation with the provided number of generations.
		/// </summary>
		/// <param name="cells">The cells to apply the automation on.</param>
		/// <returns>The final status of the cellular automation.</returns>
		private CellStatus[,] ComputeAutomaton(CellStatus[,] cells)
		{
			for (var i = 0; i < nbrGenerations; i++)
			{
				cells = ComputeNextGeneration(cells);
			}

			return cells;
		}

		public CellStatus[,] ComputeNextGeneration(CellStatus[,] cells)
		{
			var newCells = (CellStatus[,]) cells.Clone();

			int maxX = cells.GetLength(0);
			int maxY = cells.GetLength(1);

			for (var x = 0; x < maxX; x++)
			{
				for (var y = 0; y < maxY; y++)
				{
					CellStatus cell = cells[x, y];
					int neighborsCount = GetNumberOfMooreNeighbors(cells, x, y);

					bool isStayingAlive = neighborsCount is 1 or >= 4 and <= 8;
					bool isBecomingAlive = neighborsCount is >= 5 and <= 8;

					newCells[x, y] = cell switch
					{
						CellStatus.Ground when isStayingAlive => CellStatus.Ground,
						CellStatus.Empty when isBecomingAlive => CellStatus.Ground,
						_ => CellStatus.Empty,
					};
				}
			}

			return newCells;
		}

		public CellStatus[,] RemoveSmallIslands(CellStatus[,] cells)
		{
			int maxX = cells.GetLength(0);
			int maxY = cells.GetLength(1);

			var newCells = (CellStatus[,]) cells.Clone();

			int[,] areas = FindAreas(cells, out var areaCounts);

			switch (floodFillBehaviour)
			{
				case FloodFillBehaviour.KeepOnlyBiggestIsland:
				{
					int biggestIndex = areaCounts.MaxIndex();

					for (var x = 0; x < maxX; x++)
					{
						for (var y = 0; y < maxY; y++)
						{
							if (areas[x, y] != biggestIndex)
							{
								newCells[x, y] = CellStatus.Empty;
							}
						}
					}

					break;
				}
				case FloodFillBehaviour.RemoveSmallIslands:
				{
					for (var x = 0; x < maxX; x++)
					{
						for (var y = 0; y < maxY; y++)
						{
							if (areas[x, y] != 0 && areaCounts[areas[x, y]] <= smallIslandSize)
							{
								newCells[x, y] = CellStatus.Empty;
							}
						}
					}

					break;
				}
				default:
					throw new ArgumentOutOfRangeException();
			}

			return newCells;
		}

		private int[,] FindAreas(CellStatus[,] cells, out List<int> areaCounts)
		{
			int maxX = cells.GetLength(0);
			int maxY = cells.GetLength(1);

			areaCounts = new List<int> {0};
			var areas = new int[maxX, maxY];

			var currentId = 1;
			for (var x = 0; x < maxX; x++)
			{
				for (var y = 0; y < maxY; y++)
				{
					var count = 0;

					void FillArea(Vector2Int pos)
					{
						while (true)
						{
							if (pos.x < 0 || pos.y < 0 || pos.x >= maxX || pos.y >= maxY) return;
							if (!(cells[pos.x, pos.y] == CellStatus.Ground && areas[pos.x, pos.y] == 0)) return;

							areas[pos.x, pos.y] = currentId;
							count++;

							pos.x += 1;
							FillArea(pos);
							pos.x -= 2;
							FillArea(pos);
							pos.x += 1;
							pos.y += 1;
							FillArea(pos);
							pos.y -= 2;
						}
					}

					FillArea(new Vector2Int(x, y));
					if (count <= 0) continue;
					areaCounts.Add(count);
					currentId++;
				}
			}

			return areas;
		}

		private bool IsInList(IEnumerable<List<Vector2Int>> areasCoords, Vector2Int pos)
		{
			return areasCoords
				.SelectMany(coords => coords)
				.Any(coord => coord == pos);
		}

		private int GetNumberOfMooreNeighbors(CellStatus[,] cells, int x, int y)
		{
			int maxX = cells.GetLength(0);
			int maxY = cells.GetLength(0);

			var count = 0;

			for (int i = -1; i <= 1; i++)
			{
				int newX = x + i;
				if (newX < 0 || newX >= maxX) continue;

				for (int j = -1; j <= 1; j++)
				{
					int newY = y + j;
					if (newY < 0 || newY >= maxY) continue;
					if (newX == x && newY == y) continue;

					if (cells[newX, newY] == CellStatus.Ground)
					{
						count++;
					}
				}
			}

			return count;
		}

		public CellStatus[,] RandomFill(Size mapSize)
		{
			var cells = new CellStatus[mapSize.Width, mapSize.Height];

			Parallel.For(0, mapSize.Width, x =>
			{
				for (var y = 0; y < mapSize.Height; y++)
				{
					double val = StaticRandom.NextDouble();
					if (val <= chanceToBeGround)
					{
						cells[x, y] = CellStatus.Ground;
					}
					else
					{
						cells[x, y] = CellStatus.Empty;
					}
				}
			});

			return cells;
		}
	}
}