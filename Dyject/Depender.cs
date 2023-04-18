namespace Dyject;

public abstract class Depender
{
	public Depender() => Dyjector.Resolve4Ctor(GetType())(this);

	public Depender(bool cancelInjection)
	{
		if(!cancelInjection)
			Dyjector.Resolve4Ctor(GetType())(this);
	}
}