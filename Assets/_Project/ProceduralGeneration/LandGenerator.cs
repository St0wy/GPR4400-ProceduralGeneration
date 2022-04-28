using System;
using MyBox;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace ProcGen.ProceduralGeneration
{
	public enum GeneratorType
	{
		PerlinNoise,
		CellularAutomaton,
	}

	[ExecuteInEditMode]
	public class LandGenerator : MonoBehaviour
	{
		private const float MinSeed = 0f;
		private const float MaxSeed = 300f;

		[MustBeAssigned] [SerializeField] private Tilemap landTilemap;
		[MustBeAssigned] [SerializeField] private TileBase groundTile;
		[MustBeAssigned] [SerializeField] private Tilemap waterTilemap;
		[MustBeAssigned] [SerializeField] private TileBase waterTile;
		[SerializeField] private Size mapSize;
		[SerializeField] private GeneratorType currentGeneratorType;
		[SerializeField] private PerlinMapGenerator perlinMapGenerator;
		[SerializeField] private CellularAutomatonMapGenerator cellularAutomatonMapGenerator;

		private void Awake()
		{
			perlinMapGenerator = new PerlinMapGenerator()
			{
				Origin = new Vector2(
					Random.Range(MinSeed, MaxSeed),
					Random.Range(MinSeed, MaxSeed)
				),
			};
			cellularAutomatonMapGenerator = new CellularAutomatonMapGenerator();
		}

		[ButtonMethod]
		public void Regenerate()
		{
			NewSeed();
			switch (currentGeneratorType)
			{
				case GeneratorType.PerlinNoise:
					GenerateWithPerlin();
					break;
				case GeneratorType.CellularAutomaton:
					GenerateWithCellularAutomaton();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		[ButtonMethod]
		public void GenerateWithPerlin()
		{
			landTilemap.ClearAllTiles();
			FillWater();
			Generate(perlinMapGenerator);
		}

		[ButtonMethod]
		public void GenerateWithCellularAutomaton()
		{
			landTilemap.ClearAllTiles();
			FillWater();
			Generate(cellularAutomatonMapGenerator);
		}

		[ButtonMethod]
		public void NewSeed()
		{
			Random.InitState((int) DateTime.Now.Ticks);
			perlinMapGenerator.Origin = new Vector2(
				Random.Range(MinSeed, MaxSeed),
				Random.Range(MinSeed, MaxSeed)
			);
		}

		private void FillWater()
		{
			waterTilemap.ClearAllTiles();
			var area = new BoundsInt(0, 0, 0, mapSize.Width, mapSize.Height, 1);
			var tileArray = new TileBase[area.size.x * area.size.y * area.size.z];
			for (var i = 0; i < tileArray.Length; i++)
			{
				tileArray[i] = waterTile;
			}

			waterTilemap.SetTilesBlock(area, tileArray);
		}

		private void Generate(IMapGenerator generator)
		{
			var map = generator.GenerateMap(mapSize);
			var area = new BoundsInt(0, 0, 0, mapSize.Width, mapSize.Height, 1);
			var tileArray = new TileBase[area.size.x * area.size.y];
			for (var x = 0; x < mapSize.Width; x++)
			{
				for (var y = 0; y < mapSize.Height; y++)
				{
					if (map[x, y] == CellStatus.Ground)
					{
						tileArray[x + (y * area.size.y)] = groundTile;
					}
				}
			}

			landTilemap.SetTilesBlock(area, tileArray);
		}
	}
}