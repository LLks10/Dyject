using Dyject.DyjectorHelpers;
using Dyject.Extensions;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

namespace Dyject;

public static class Dyjector
{
    internal static readonly Dictionary<Type, Func<object>> resolvers = new();
	internal static readonly Dictionary<Type, Action<object>> ctorResolvers = new();
	private static readonly Dictionary<Type, int> singletonMap = new();
	private static readonly List<object> singletons = new();

	internal const string methodNameConst = "djct_ctor_";
    internal const BindingFlags fieldsFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

    private static readonly FieldInfo SingletonField = typeof(Dyjector).GetField(nameof(singletons), BindingFlags.NonPublic | BindingFlags.Static)!;
    private static readonly MethodInfo IndexList = typeof(List<object>).GetMethod("get_Item")!;

	public static T Resolve<T>() where T : class => Resolve(typeof(T)).As<T>();
    public static object Resolve(Type type)
    {
        if (resolvers.TryGetValue(type, out Func<object>? func))
            return func();

		var method = CreateDynamicMethod(type);
		var tree = DITreeBuilder.BuildDITree(type);
		var ctor = DIResolver.ResolveDI(method, tree);

		resolvers.Add(type, ctor);

		return ctor();
    }

	internal static Action<object> Resolve4Ctor(Type type)
	{
		if (ctorResolvers.TryGetValue(type, out Action<object>? func))
			return func;

		var method = new DynamicMethod($"{methodNameConst}{type.Name}", null, new Type[] { typeof(object) });
		var tree = DITreeBuilder.BuildDITree(type);
		var ctor = DIResolver.ResolveDI4Ctor(method, tree);

		ctorResolvers.Add(type, ctor);

		return ctor;
	}

	internal static Func<object> Init(Type type)
	{
		var method = CreateDynamicMethod(type);
		var tree = DITreeBuilder.BuildDITree(type);
		var ctor = DIResolver.ResolveDI(method, tree);

		resolvers[type] = ctor;

		return ctor;
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

    public static void RegisterSingleton<T>(T obj)
    {
		if (singletonMap.ContainsKey(typeof(T)))
			throw new InvalidOperationException($"'{typeof(T)}' already has a singleton registered");

		singletonMap[typeof(T)] = singletons.Count;
		singletons.Add(obj);
	}

    // Initialize all Dyjector<T>'s
    public static void InitAll()
    {
		throw new NotImplementedException();
	}

	internal static ILGenerator GetSingleton(Type type, ILGenerator ilgen)
	{
		if (!singletonMap.TryGetValue(type, out var idx))
		{
			object obj = ResolveSingleton(type);
			singletonMap[type] = singletons.Count;
			idx = singletons.Count;
			singletons.Add(obj);
		}

		return ilgen
			.Ldsfld(SingletonField)
			.Ldc(idx)
			.Callvirt(IndexList);
	}

	internal static object TryGetSingleton(Type type)
	{
		if (!singletonMap.TryGetValue(type, out var idx))
			throw new InvalidOperationException($"Type \"{type.FullName}\" isnt a singleton");

		return singletons[idx];
	}

	internal static void Reset()
    {
        resolvers.Clear();
		ctorResolvers.Clear();
        singletonMap.Clear();
        singletons.Clear();
    }

    private static object ResolveSingleton(Type type)
    {
		var tree = DITreeBuilder.BuildDITree(type);

		var method = new DynamicMethod($"{methodNameConst}{type.Name}", type, Array.Empty<Type>());

		DIResolver.ResolveDI(method, tree);

		return method.CreateDelegate<Func<object>>()();
	}

	private static DynamicMethod CreateDynamicMethod(Type type) =>
		new DynamicMethod($"{methodNameConst}{type.Name}", type, Array.Empty<Type>());
}

public static class Dyjector<T> where T : class
{
	private static Func<T> _func;

    public static void Init() =>
        _func = Dyjector.Init(typeof(T)).As<Func<T>>();

	internal static void Reset() => _func = null;

    public static T Create()
    {
        Debug.Assert(_func != null);
		return _func();
	}
}