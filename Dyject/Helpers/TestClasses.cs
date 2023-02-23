using Dyject.Attributes;

namespace Dyject.Helpers;

public sealed class TestEmpty
{
	int a, b, c;
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
	public ServiceB serviceB2;
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

	public int NewId()
	{
		Id = _id++;
		return Id;
	}

	public static void ResetId() => _id = 0;

}