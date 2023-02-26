using Dyject.Tests.TestData;

namespace Dyject.Tests;

public static class Helper
{
	public static void Reset()
	{
		Dyjector.Reset();
		Identifier.ResetId();
		Depender<T0Main>.Reset();
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