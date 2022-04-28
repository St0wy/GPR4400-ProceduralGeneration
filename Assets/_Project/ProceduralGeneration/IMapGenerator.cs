namespace ProcGen.ProceduralGeneration
{
	public interface IMapGenerator
	{
		CellStatus[,] GenerateMap(Size mapSize);
	}
}