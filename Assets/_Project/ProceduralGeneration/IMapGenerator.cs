namespace ProcGen.ProceduralGeneration
{
	public interface IMapGenerator
	{
		int[,] GenerateMap(Size mapSize);
	}
}