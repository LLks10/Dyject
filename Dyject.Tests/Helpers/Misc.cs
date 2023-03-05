using Dyject.Tests.TestData.Dyjector;

namespace Dyject.Tests;

public static class Helper
{
	public static void Reset()
	{
		Dyjector.Reset();
		Identifier.ResetId();
		Dyjector<T0Main>.Reset();
		Dyjector<ShSiMain0>.Reset();
		Dyjector<ShSiMain1>.Reset();
		Dyjector<Circ>.Reset();
	}
}

public abstract class Identifier
{
	private static int _id;
	public int Id { get; private set; }
	public Identifier()
	{
		Id = _id++;
	}

	public int NewId()
	{
		Id = _id++;
		return Id;
	}

	public static void ResetId() => _id = 0;

}