using Dyject.Attributes;

namespace Dyject.Helpers;

public sealed class TestEmpty
{
	int a, b, c;
}

public class TestMain : Depender<TestMain>
{
	public static ServiceB serviceB_test = new();
	public ServiceA serviceA;
	public ServiceB serviceB;
}

[Injectable]
public class ServiceA : Identifier
{
	public ServiceB serviceB;
	public ServiceB serviceB2;
	public ServiceD servD;
}

[Injectable]
public class ServiceB : Identifier
{
	public ServiceC serC;
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