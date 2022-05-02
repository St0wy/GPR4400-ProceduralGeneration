using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyBox;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

namespace ProcGen.ProceduralGeneration
{
	[Serializable]
	public class CellularAutomatonMapGenerator : IMapGenerator
	{
		[OverrideLabel("Number of Generations")] [SerializeField]
		private int nbrGenerations = 50;

		[Range(0, 1)] [SerializeField] private double chanceToBeGround = 0.55d;

		[SerializeField] private int smallIslandSize = 30;

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
			var newCells = (CellStatus[,]) cells.Clone();

			List<List<Vector2Int>> areasCoords = new();

			FindAreas(cells, areasCoords);

			// Removed the coordinates contained in the List<List<>>
			// that are smaller that the specified island size
			foreach (Vector2Int position in from positions in areasCoords
			         let areaSize = positions.Count
			         where areaSize <= smallIslandSize
			         from position in positions
			         select position)
			{
				cells[position.x, position.y] = CellStatus.Empty;
			}

			return newCells;
		}

		private void FindAreas(CellStatus[,] cells, List<List<Vector2Int>> areasCoords)
		{
			int maxX = cells.GetLength(0);
			int maxY = cells.GetLength(1);

			for (var x = 0; x < maxX; x++)
			{
				for (var y = 0; y < maxY; y++)
				{
					List<Vector2Int> newCoords = new();
					FillArea(areasCoords, cells, new Vector2Int(x, y), newCoords);
					if (newCoords.Count > 0)
					{
						areasCoords.Add(newCoords);
					}
				}
			}
		}

		/// <summary>
		/// Fills the area of the cell at the specified position.
		/// </summary>
		/// <param name="areasCoords">The list of coordinates of every areas.</param>
		/// <param name="cells">The array containing the world.</param>
		/// <param name="pos">The coordinate to check the area of.</param>
		/// <param name="newCoords">The array for the current area.</param>
		private void FillArea(IEnumerable<List<Vector2Int>> areasCoords, CellStatus[,] cells, Vector2Int pos,
			ICollection<Vector2Int> newCoords)
		{
			if (pos.x < 0 || pos.y < 0 ||
			    pos.x >= cells.GetLength(0) || pos.y >= cells.GetLength(1))
				return;
			var coords = areasCoords as List<Vector2Int>[] ?? areasCoords.ToArray();
			bool isInList = IsInList(coords, pos) && !newCoords.Contains(pos);
			if (!(cells[pos.x, pos.y] == CellStatus.Ground && !isInList)) return;

			newCoords.Add(pos);

			pos.x += 1;
			FillArea(coords, cells, pos, newCoords);
			pos.x -= 2;
			FillArea(coords, cells, pos, newCoords);
			pos.x += 1;

			pos.y += 1;
			FillArea(coords, cells, pos, newCoords);

			pos.y -= 2;
			// ReSharper disable once TailRecursiveCall
			FillArea(coords, cells, pos, newCoords);
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