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
	internal static readonly Dictionary<Type, int> singletonMap = new();
	internal static readonly Dictionary<Type, Type> instantiations = new();
	private static readonly List<object> singletons = new();

    private static readonly FieldInfo SingletonField = typeof(Dyjector).GetField(nameof(singletons), BindingFlags.NonPublic | BindingFlags.Static)!;
    private static readonly MethodInfo IndexList = typeof(List<object>).GetMethod("get_Item")!;

	internal const string methodNameConst = "djct_ctor_";
    internal const BindingFlags fieldsFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

	public static T Resolve<T>() where T : class => Resolve(typeof(T)).As<T>();
    public static object Resolve(Type type)
    {
        if (resolvers.TryGetValue(type, out Func<object>? func))
            return func();

		return Init(type)();
    }

	internal static Func<object> Init(Type type)
	{
		var method = CreateDynamicMethod(type);
		var tree = DITreeBuilder.BuildTree(type);
		var ctor = DIResolver.Resolve(method, tree);

		resolvers[type] = ctor;

		return ctor;
	}

	internal static Action<object> Resolve4Ctor(Type type)
	{
		if (ctorResolvers.TryGetValue(type, out Action<object>? func))
			return func;

		var method = new DynamicMethod($"{methodNameConst}{type.Name}", null, new Type[] { typeof(object) });
		var tree = DITreeBuilder.BuildTree(type);
		var ctor = DIResolver.Resolve4Ctor(method, tree);

		ctorResolvers.Add(type, ctor);

		return ctor;
	}

	public static void RegisterInstantiation<T, U>() where U : class, T
													 where T : class
    {
		var u = typeof(U);
		if (u.IsInterface || u.IsAbstract)
			throw new ArgumentException($"{typeof(U).FullName} can't be an interface or abstract");
		var t = typeof(T);

		if(instantiations.ContainsKey(t))
			throw new InvalidOperationException($"'{typeof(T).FullName}' already has an instantiation registered");

		instantiations[t] = u;
	}

	public static void RegisterInstantiation(Type @abstract, Type instantiation)
	{
		if (instantiation.IsInterface || instantiation.IsAbstract)
			throw new ArgumentException($"{instantiation.FullName} can't be an interface or abstract");

		if (instantiations.ContainsKey(@abstract))
			throw new InvalidOperationException($"'{@abstract.FullName}' already has an instantiation registered");

		instantiations[@abstract] = instantiation;
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
		instantiations.Clear();
    }

    private static object ResolveSingleton(Type type)
    {
		var tree = DITreeBuilder.BuildTree(type);

		var method = new DynamicMethod($"{methodNameConst}{type.Name}", type, Array.Empty<Type>());

		DIResolver.Resolve(method, tree);

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