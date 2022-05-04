using System;
using JetBrains.Annotations;
using MyBox;
using ProcGen.ProceduralGeneration.CellularAutomaton;
using ProcGen.ProceduralGeneration.PerlinNoise;
using UnityEngine;
using UnityEngine.Serialization;
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

		[FormerlySerializedAs("landTilemap")] [MustBeAssigned] [SerializeField]
		private Tilemap levelTilemap;

		[MustBeAssigned] [SerializeField] private TileBase groundTile;

		[FormerlySerializedAs("waterTilemap")] [MustBeAssigned] [SerializeField]
		private Tilemap backgroundTilemap;

		[FormerlySerializedAs("waterTile")] [MustBeAssigned] [SerializeField]
		private TileBase backgroundTile;

		[SerializeField] private SolidMode solidMode;

		[SerializeField] private Size mapSize;

		[SerializeField] private GeneratorType currentGeneratorType;

		[ConditionalField(nameof(currentGeneratorType), false, GeneratorType.PerlinNoise)] [SerializeField]
		private PerlinMapGenerator perlinMapGenerator;

		[ConditionalField(nameof(currentGeneratorType), false, GeneratorType.CellularAutomaton)] [SerializeField]
		private CellularAutomatonMapGenerator cellularAutomatonMapGenerator;

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

		[UsedImplicitly]
		[ButtonMethod(ButtonMethodDrawOrder.BeforeInspector)]
		public void Regenerate()
		{
			NewSeed();
			switch (currentGeneratorType)
			{
				case GeneratorType.PerlinNoise:
					Generate(perlinMapGenerator);
					break;
				case GeneratorType.CellularAutomaton:
					Generate(cellularAutomatonMapGenerator);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		[ButtonMethod]
		public void NewSeed()
		{
			StaticRandom.InitState(Environment.TickCount);
			perlinMapGenerator.Origin = new Vector2(
				Random.Range(MinSeed, MaxSeed),
				Random.Range(MinSeed, MaxSeed)
			);
		}

		private void FillWater()
		{
			backgroundTilemap.ClearAllTiles();
			var area = new BoundsInt(0, 0, 0, mapSize.Width, mapSize.Height, 1);
			var tileArray = new TileBase[area.size.x * area.size.y * area.size.z];
			for (var i = 0; i < tileArray.Length; i++)
			{
				tileArray[i] = backgroundTile;
			}

			backgroundTilemap.SetTilesBlock(area, tileArray);
		}

		private void Generate(IMapGenerator generator)
		{
			var map = generator.GenerateMap(mapSize);
			switch (solidMode)
			{
				case SolidMode.SolidInside:
					DrawMap(map);
					break;
				case SolidMode.SolidOutside:
					InvertMap(map);
					DrawMap(map);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void DrawMap(CellStatus[,] map)
		{
			levelTilemap.ClearAllTiles();
			FillWater();

			var area = new BoundsInt(0, 0, 0, mapSize.Width, mapSize.Height, 1);
			var tileArray = new TileBase[area.size.x * area.size.y];

			for (var x = 0; x < mapSize.Width; x++)
			{
				for (var y = 0; y < mapSize.Height; y++)
				{
					if (map[x, y] == CellStatus.Solid)
					{
						tileArray[x + (y * area.size.y)] = groundTile;
					}
				}
			}

			levelTilemap.SetTilesBlock(area, tileArray);
		}

		private static void InvertMap(CellStatus[,] map)
		{
			int maxX = map.GetLength(0);
			int maxY = map.GetLength(1);

			for (var x = 0; x < maxX; x++)
			{
				for (var y = 0; y < maxY; y++)
				{
					switch (map[x, y])
					{
						case CellStatus.Empty:
							map[x, y] = CellStatus.Solid;
							break;
						case CellStatus.Solid:
							map[x, y] = CellStatus.Empty;
							break;
						case CellStatus.Max:
						default:
							throw new ArgumentOutOfRangeException();
					}
				}
			}
		}
	}
}