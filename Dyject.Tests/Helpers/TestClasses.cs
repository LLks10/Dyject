using Dyject.Attributes;

namespace Dyject.Tests;

public static class Helper
{
	public static void Reset()
	{
		Dyjector.Reset();
		Identifier.ResetId();
		Depender<TestEmpty>.Reset();
		Depender<TestMain>.Reset();
		Depender<ServiceA>.Reset();
		Depender<ServiceB>.Reset();
	}
}

public sealed class TestEmpty : Identifier
{

}

public class TestMain : Depender<TestMain>
{
	public static ServiceB serviceB_test = new();
	private ServiceA serviceA;
	public ServiceB serviceB;
}

[Injectable]
public class ServiceA : Identifier
{
	public ServiceB serviceB;
}

[Injectable]
public class ServiceB : Identifier
{

}


public abstract class Identifier
{
	private static int _id;
	public int Id { get; private set; }
	public Identifier()
	{
		Id = _id++;
	}

	public Identifier(bool no)
	{

	}

	public int NewId()
	{
		Id = _id++;
		return Id;
	}

	public static void ResetId() => _id = 0;

}