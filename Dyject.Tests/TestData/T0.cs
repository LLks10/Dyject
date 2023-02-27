using Dyject.Attributes;
using Dyject;
using Dyject.Tests;

namespace Dyject.Tests.TestData;

public sealed class TestEmpty : Identifier
{
	int a, b, c;
}

public class T0Main
{
	public T0TrA tr0;
	public T0TrB tr1;
	public T0ScC sc0;
	public T0SiA si0;
}

[Injectable]
public class T0TrA : Identifier
{
	public T0TrB tr0;
	public T0TrB tr1;
	public T0ScA sc0;
	public T0ScB sc1;
}

[Injectable]
public class T0TrB : Identifier
{
	public T0ScA sc0;
	public T0ScB sc1;
}

[Injectable]
public class T0TrC : Identifier
{

}

[Injectable(InjScope.Scoped)]
public class T0ScA : Identifier
{

}

[Injectable(InjScope.Scoped)]
public class T0ScB : Identifier
{
	public T0TrC tr0;
}


[Injectable(InjScope.Scoped)]
public class T0ScC : Identifier
{

}

[Injectable(InjScope.Singleton)]
public class T0SiA : Identifier
{
	T0TrA tr0;
}