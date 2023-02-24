using Dyject.DyjectorHelpers;
using System.Reflection;
using System.Reflection.Emit;

namespace Dyject;

public static class Dyjector
{
    internal static readonly Dictionary<Type, Func<object>> resolvers = new();

    internal const string methodNameConst = "djct_ctor_";
    internal const BindingFlags fieldsFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

	public static T Resolve<T>() where T : class => (T)Resolve(typeof(T));
    public static object Resolve(Type type)
    {
        if (resolvers.TryGetValue(type, out Func<object>? func))
            return func();

        var tree = DITreeBuilder.BuildDITree(type);

		var method = new DynamicMethod($"{methodNameConst}{type.Name}", type, Array.Empty<Type>());

        DIResolver.ResolveDI(method, tree);

		var ctor = method.CreateDelegate<Func<object>>();
		resolvers.Add(type, ctor);

		return ctor();
    }


    // Allow registering instantiations to be limited to instantiation of specified type?
    // Allow specifying specific instantiation using attribute on field
    // When encountering type T -> instantiate U
    public static bool RegisterInstantiation<T, U>() where U : T
    {
        throw new NotImplementedException();
    }
    // When encountering type T -> instantiate to value 
    public static bool RegisterInstantiation<T>(T value)
    {
        throw new NotImplementedException();
    }

    public static bool RegisterSingleton<T>(T obj)
    {
        // Singleton constructor?
        throw new NotImplementedException();
    }

    internal static void Reset() => resolvers.Clear();
}