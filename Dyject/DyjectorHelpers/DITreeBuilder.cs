using Dyject.Attributes;
using Dyject.DyjectorHelpers;
using System.Reflection;

namespace Dyject.DyjecterHelpers;

internal static class DITreeBuilder
{
	public static List<DINode> BuildDITree(Type type)
	{
		HashSet<FieldInfo> visited = new();
		List<DINode> nodes = new();
		TopologicalSort(type, visited, nodes, 0);

		return nodes;
	}

	private static DINode TopologicalSort(Type current, HashSet<FieldInfo> visited, List<DINode> nodes, int depth)
	{
		var node = new DINode { type = current, depth = depth };
		var scope = current.GetCustomAttribute<Injectable>();
		if (scope is { } scp)
			node.scope = scp.Scope;

		var fields = current.GetFields(Dyjector.fieldsFlags);
		foreach (var field in fields)
		{
			if (visited.Contains(field))
				throw new Exception("cycle"); //TODO: improve error message
			if (field.FieldType.GetCustomAttribute<Injectable>() is null)
				continue;
			if (field.GetCustomAttribute<DontInject>() is not null)
				continue;

			visited.Add(field);
			var child = TopologicalSort(field.FieldType, visited, nodes, depth + 1);
			child.field = field;
			node.children.Add(child);
		}

		nodes.Add(node); //TODO: Add check to avoid adding duplicate scopes and singletons
		return node;
	}
}