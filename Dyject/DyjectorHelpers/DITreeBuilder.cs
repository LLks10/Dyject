using Dyject.Attributes;
using Dyject.Exceptions;
using System.Diagnostics;
using System.Reflection;

namespace Dyject.DyjectorHelpers;

internal class DITreeBuilder
{
	public static DINode BuildTree(Type type)
	{
		DINode parent = new()
		{
			type = type,
			depth = 0,
			references = 0,
		};

		var self = new DITreeBuilder();
		self.BuildTree(parent, InjScope.Transient);
		return parent;
	}

	readonly HashSet<object> visited = new();
	readonly Dictionary<Type, DINode> scope = new();
	private DITreeBuilder() { }

	private DINode BuildTree(DINode current, InjScope currentScope)
	{
		ConstructorInjection(current, currentScope);

		FieldInjection(current, currentScope);

		return current;
	}

	private void ConstructorInjection(DINode current, InjScope currentScope)
	{
		(var ctor, var pars) = GetConstructor(current);
		current.ctor = ctor;

		foreach (var par in pars)
		{
			if (par.HasDefaultValue)
			{
				current.args.Add((par, null));
				continue;
			}

			var scope = par.ParameterType.GetCustomAttribute<Injectable>()?.Scope;

			if (Dyjector.singletonMap.ContainsKey(par.ParameterType))
				scope = InjScope.Singleton;

			if (scope is null)
				continue;
			if (par.ParameterType == current.type)
				throw new InvalidDependencyException($"Implementing self as dependency is not allowed. \"{current.type.FullName} -> {par.Name}\".");
			if (visited.Contains(par))
				CircularDependencyException.Throw(current);

			visited.Add(par);

			var child = ResolveChild(current, par.ParameterType, currentScope, scope);

			current.args.Add((par, child));

			visited.Remove(par);
		}
	}

	private void FieldInjection(DINode current, InjScope currentScope)
	{
		var fields = GetInstantiation(current.type).GetFields(Dyjector.fieldsFlags);
		var ctorTypes = current.args.Where(x => x.Param.DefaultValue != null).Select(x => x.Param.ParameterType).ToArray();

		foreach (var field in fields)
		{
			var scope = field.FieldType.GetCustomAttribute<Injectable>()?.Scope;

			if (Dyjector.singletonMap.ContainsKey(field.FieldType))
				scope = InjScope.Singleton;

			if (scope is null)
				continue;
			if (field.GetCustomAttribute<DontInject>() is not null)
				continue;
			if (ctorTypes.Contains(field.FieldType))
				continue;
			if (field.FieldType == current.type)
				throw new InvalidDependencyException($"Implementing self as dependency is not allowed. \"{current.type.FullName} -> {field.Name}\".");
			if (visited.Contains(field))
				CircularDependencyException.Throw(current);

			visited.Add(field);

			var child = ResolveChild(current, field.FieldType, currentScope, scope);

			current.children.Add((field, child));

			visited.Remove(field);
		}
	}

	private DINode ResolveChild(DINode current, Type type, InjScope currentScope, InjScope? scope) 
		=> currentScope > scope
			? ResolveTransient(type, current, currentScope)
			: scope switch
			{
				InjScope.Transient => ResolveTransient(type, current, InjScope.Transient),
				InjScope.Scoped => ResolveScoped(type, current),
				InjScope.Singleton => ResolveSingleton(type, current),
				_ => throw new UnreachableException()
			};

	private DINode ResolveTransient(Type type, DINode current, InjScope currentScope)
	{
		var child = new DINode
		{
			depth = current.depth + 1,
			parent = current,
			type = type,
			references = 1,
			scope = InjScope.Transient,
		};
		BuildTree(child, currentScope);

		return child;
	}

	private DINode ResolveScoped(Type type, DINode current)
	{
		if (scope.TryGetValue(type, out var child))
		{
			child.references++;
			return child;
		}

		child = new DINode
		{
			depth = current.depth + 1,
			parent = current,
			type = type,
			references = 1,
			scope = InjScope.Scoped,
		};

		scope[type] = child;

		BuildTree(child, InjScope.Scoped);

		return child;
	}

	private DINode ResolveSingleton(Type type, DINode current)
	{
		var child = new DINode
		{
			depth = current.depth + 1,
			parent = current,
			type = type,
			references = 1,
			scope = InjScope.Singleton,
		};

		return child;
	}

	private (ConstructorInfo Ctor, ParameterInfo[] Params) GetConstructor(DINode node)
	{
		var type = GetInstantiation(node.type);
		var ctors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

		if (ctors.Length == 0)
			throw new InvalidOperationException($"Can't find a constructor for '{type.FullName}'");

		if (ctors.Length == 1)
			return (ctors[0], ctors[0].GetParameters());

		// Try to find preferred ctor by attribute
		foreach(var ctor in ctors)
		{
			if (ctor.GetCustomAttribute<InjectionConstructor>() is not null)
				return (ctor, ctor.GetParameters());
		}

		// Get valid constructor with most arguments
		int bestScore = -1;
		int bestIdx = -1;
		ParameterInfo[] bestParams = null;

		for(int i = 0; i < ctors.Length; i++)
		{
			var pars = ctors[i].GetParameters();
			int score = 0;
			foreach(var p in pars)
			{
				// Check if constructor is valid
				if(!p.HasDefaultValue && p.GetCustomAttribute<Injectable>() is null && !Dyjector.singletonMap.ContainsKey(p.ParameterType))
				{
					score = -1;
					break;
				}

				if(!p.HasDefaultValue)
					score++;
			}
			
			if(score > bestScore)
			{
				bestScore = score;
				bestIdx = i;
				bestParams = pars;
			}
		}

		if (bestParams is null)
			throw new InvalidOperationException($"No usable constructors found for '{type.FullName}'.");

		return (ctors[bestIdx], bestParams);
	}

	private Type GetInstantiation(Type type)
	{
		if (Dyjector.instantiations.TryGetValue(type, out var st))
			return st;
		
		if (type.IsInterface)
		{
			var subTypes = AppDomain.CurrentDomain.GetAssemblies()
				.Where(x => x.FullName is not null && !x.FullName.StartsWith("System.") && !x.FullName.StartsWith("Microsoft."))
				.SelectMany(s => s.GetTypes())
				.Where(p => !p.IsInterface && type.IsAssignableFrom(p));

			var c = subTypes.Count();
			if (c == 0)
				throw new InvalidOperationException($"No instantiation of interface '{type.FullName}' found.");
			if (c > 1)
				throw new InvalidOperationException($"Multiple instantiations of interface '{type.FullName}' found. Specify which instantiation to use with '{nameof(Dyjector.RegisterInstantiation)}'.");
			
			var subtype = subTypes.First();
			Dyjector.RegisterInstantiation(type, subtype);
			type = subtype;
		}

		return type;
	}
}