﻿using Dyject.Attributes;
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
		self.TopologicalSort(parent);
		return self.nodes;
	}

	readonly HashSet<FieldInfo> visited = new();
	readonly Dictionary<Type, DINode> scope = new();
	readonly List<DINode> nodes = new();
	private DITreeBuilder() { }

	private DINode TopologicalSort(DINode current)
	{
		var fields = current.type.GetFields(Dyjector.fieldsFlags);
		foreach (var field in fields)
		{
			// Guards
			var attr = field.FieldType.GetCustomAttribute<Injectable>();
			if (attr is null)
				continue;
			if (field.GetCustomAttribute<DontInject>() is not null)
				continue;
			if (field.FieldType == current.type)
				throw new InvalidDependencyException($"Implementing self as dependency is not allowed. \"{current.type.FullName} -> {field.Name}\".");
			if (visited.Contains(field))
				CircularDependencyException.Throw(current);

			visited.Add(field);

			var child = attr.Scope switch
			{
				InjScope.Transient	=> ResolveTransient(field, current),
				InjScope.Scoped		=> ResolveScoped(field, current),
				InjScope.Singleton	=> ResolveSingleton(field, current),
				_					=> throw new UnreachableException(),
			};

			child.scope = attr.Scope;

			current.children.Add(child);

			visited.Remove(field);
		}

		nodes.Add(current);
		return current;
	}

	private DINode ResolveTransient(FieldInfo field, DINode current)
	{
		var child = new DINode
		{
			depth = current.depth + 1,
			parent = current,
			field = field,
			type = field.FieldType,
			references = 1,
		};
		TopologicalSort(child);

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
			field = field,
			type = field.FieldType,
			references = 1,
		};

		scope[field.FieldType] = child;

		TopologicalSort(child);

		return child;
	}

	private DINode ResolveSingleton(FieldInfo field, DINode current)
	{
		throw new NotImplementedException();
	}
}