using Dyject.Attributes;
using Dyject.Exceptions;
using System.Diagnostics;
using System.Reflection;

namespace Dyject.DyjectorHelpers;

internal class DITreeBuilder
{
	public static List<DINode> BuildDITree(Type type)
	{
		DINode parent = new()
		{
			type = type,
			depth = 0,
			references = 0,
		};

		var self = new DITreeBuilder();
		self.TopologicalSort(parent, InjScope.Transient);
		return self.nodes;
	}

	readonly HashSet<FieldInfo> visited = new();
	readonly Dictionary<Type, DINode> scope = new();
	readonly List<DINode> nodes = new();
	private DITreeBuilder() { }

	private DINode TopologicalSort(DINode current, InjScope currentScope)
	{
		var ctor = GetConstructor(current.type);
		current.ctor = ctor;
		var paras = ctor.GetParameters();

		var fields = current.type.GetFields(Dyjector.fieldsFlags);
		foreach (var field in fields)
		{
			var scope = field.FieldType.GetCustomAttribute<Injectable>()?.Scope;

			if (Dyjector.singletonMap.ContainsKey(field.FieldType))
				scope = InjScope.Singleton;

			if (scope is null)
				continue;
			if (field.GetCustomAttribute<DontInject>() is not null)
				continue;
			if (field.FieldType == current.type)
				throw new InvalidDependencyException($"Implementing self as dependency is not allowed. \"{current.type.FullName} -> {field.Name}\".");
			if (visited.Contains(field))
				CircularDependencyException.Throw(current);

			visited.Add(field);

			var child = currentScope > scope
				? ResolveTransient(field, current, currentScope)
				: scope switch
				{
					InjScope.Transient => ResolveTransient(field, current, InjScope.Transient),
					InjScope.Scoped => ResolveScoped(field, current),
					InjScope.Singleton => ResolveSingleton(field, current),
					_ => throw new UnreachableException()
				};

			current.children.Add((field, child));

			visited.Remove(field);
		}

		nodes.Add(current);
		return current;
	}

	private DINode ResolveTransient(FieldInfo field, DINode current, InjScope currentScope)
	{
		var child = new DINode
		{
			depth = current.depth + 1,
			parent = current,
			type = field.FieldType,
			references = 1,
			scope = InjScope.Transient,
		};
		TopologicalSort(child, currentScope);

		return child;
	}

	private DINode ResolveScoped(FieldInfo field, DINode current)
	{
		if (scope.TryGetValue(field.FieldType, out var child))
		{
			child.references++;
			return child;
		}

		child = new DINode
		{
			depth = current.depth + 1,
			parent = current,
			type = field.FieldType,
			references = 1,
			scope = InjScope.Scoped,
		};

		scope[field.FieldType] = child;

		TopologicalSort(child, InjScope.Scoped);

		return child;
	}

	private DINode ResolveSingleton(FieldInfo field, DINode current)
	{
		var child = new DINode
		{
			depth = current.depth + 1,
			parent = current,
			type = field.FieldType,
			references = 1,
			scope = InjScope.Singleton,
		};

		return child;
	}

	private ConstructorInfo GetConstructor(Type type)
	{
		var t = ResolveType(type);


		var ctors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		return ctors[0];
	}

	private Type ResolveType(Type type)
	{
		if (Dyjector.instantiations.TryGetValue(type, out var st))
			return st;
		
		if (type.IsInterface)
		{
			var subTypes = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(s => s.GetTypes())
				.Where(p => !p.IsInterface && type.IsAssignableFrom(p));

			var c = subTypes.Count();
			if (c == 0)
				throw new InvalidOperationException($"No instantiation of interface '{type.FullName}' found.");
			if (c > 1)
				throw new InvalidOperationException($"Multiple instantiations of interface '{type.FullName}' found. Specify which instantiation to use with '{nameof(Dyjector.RegisterInstantiation)}'.");
			return subTypes.First();
		}

		return type;
	}
}