namespace Dyject.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class Injectable : Attribute
{
	public InjScope Scope { get; }

	public Injectable(InjScope scope = InjScope.Transient)
	{
		Scope = scope;
	}
}

public enum InjScope
{
	Transient = 0,
	Scoped = 1,
	Singleton = 2,
}