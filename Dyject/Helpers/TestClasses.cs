using Dyject.Attributes;

namespace Dyject.Helpers;

internal sealed class TestEmpty
{
	int a, b, c;
}

internal class TestMain : Depender
{
	public static ServiceB serviceB_test = new();
	public ServiceA serviceA;
	public ServiceB serviceB;
	public ServiceF serviceF;
	public Singleton sig;
}

[Injectable]
internal class ServiceA : Identifier
{
	public ServiceB serviceB;
	public ServiceB serviceB2;
	public ServiceC serC;
	public ServiceD servD;
}

[Injectable]
internal class ServiceB : Identifier
{
	public ServiceC serC;
	public ServiceD servD;
}

[Injectable(InjScope.Scoped)]
internal class ServiceC : Identifier
{

}

[Injectable(InjScope.Scoped)]
internal class ServiceD : Identifier
{
	public ServiceE servE;
}

[Injectable]
internal class ServiceE : Identifier
{

}

[Injectable(InjScope.Scoped)]
internal class ServiceF : Identifier
{

}

[Injectable(InjScope.Singleton)]
internal class Singleton : Identifier
{
	private ServiceA sa;
}

internal abstract class Identifier
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