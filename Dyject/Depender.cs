using System.Runtime.CompilerServices;

namespace Dyject;

public abstract class Depender<T> where T : class
{
	private static Func<T> _func;


	public Depender()
	{

	}

	public Depender(bool cancelInjection)
	{

	}

	public static T Create()
	{
		if (_func is not null)
			return _func();

		var obj = Dyjector.Resolve<T>();
		_func = Unsafe.As<Func<T>>(Dyjector.resolvers[typeof(T)]);
		return obj;
	}

	internal static void Reset() => _func = null;
}