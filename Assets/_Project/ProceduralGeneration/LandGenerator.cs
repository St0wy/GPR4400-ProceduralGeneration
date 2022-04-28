using System;
using MyBox;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace ProcGen.ProceduralGeneration
{
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
		[SerializeField] private PerlinMapGenerator perlinMapGenerator;

		private void Awake()
		{
			perlinMapGenerator = new PerlinMapGenerator()
			{
				Origin = new Vector2(
					Random.Range(MinSeed, MaxSeed),
					Random.Range(MinSeed, MaxSeed)
				),
			};
		}

		[ButtonMethod]
		public void Regenerate()
		{
			NewSeed();
			GenerateWithPerlin();
		}

		[ButtonMethod]
		public void GenerateWithPerlin()
		{
			landTilemap.ClearAllTiles();
			FillWater();
			Generate(perlinMapGenerator);
		}

		[ButtonMethod]
		public void NewSeed()
		{
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
			int[,] map = generator.GenerateMap(mapSize);
			var area = new BoundsInt(0, 0, 0, mapSize.Width, mapSize.Height, 1);
			var tileArray = new TileBase[area.size.x * area.size.y];
			for (var x = 0; x < mapSize.Width; x++)
			{
				for (var y = 0; y < mapSize.Height; y++)
				{
					if (map[x, y] > 0)
					{
						tileArray[x + (y * area.size.y)] = groundTile;
					}
				}
			}

			landTilemap.SetTilesBlock(area, tileArray);
		}
	}
}