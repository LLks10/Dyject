using Dyject.Attributes;

namespace Dyject.Tests;

public static class Helper
{
	public static void Reset()
	{
		Dyjector.Reset();
		Identifier.ResetId();
		Depender<TestMain>.Reset();
	}
}

public sealed class TestEmpty : Identifier
{
	int a, b, c;
}

public class TestMain : Depender<TestMain>
{
	public static ServiceB serviceB_test = new();
	public ServiceA serviceA;
	public ServiceB serviceB;
	public ServiceF serviceF;
	//public Singleton sig;
}

[Injectable]
public class ServiceA : Identifier
{
	public ServiceB serviceB;
	public ServiceB serviceB2;
	public ServiceC serC;
	public ServiceD servD;
}

[Injectable]
public class ServiceB : Identifier
{
	public ServiceC serC;
	public ServiceD servD;
}

[Injectable(InjScope.Scoped)]
public class ServiceC : Identifier
{

}

[Injectable(InjScope.Scoped)]
public class ServiceD : Identifier
{
	public ServiceE servE;
}

[Injectable]
public class ServiceE : Identifier
{

}

[Injectable(InjScope.Scoped)]
public class ServiceF : Identifier
{

}

[Injectable(InjScope.Singleton)]
public class Singleton : Identifier
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