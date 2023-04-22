using Dyject.DyjectorHelpers;

namespace Dyject.Exceptions;

internal sealed class CircularDependencyException : Exception
{
	private CircularDependencyException() { }
	private CircularDependencyException(string message) : base(message) { }
	private CircularDependencyException(string message, Exception inner) : base(message, inner) { }

	public static void Throw(DINode errorNode)
	{
		List<Type> path = new();
		path.Add(errorNode.type);

		var type = Dyjector.TryGetInstantiation(errorNode.type);

		DINode current = errorNode.parent;
		while (Dyjector.TryGetInstantiation(current.type) != type)
		{
			path.Add(current.type);
			current = current.parent;
		}
		path.Add(type);

		path.Reverse();

		var parentType = GetParentType(errorNode);
		var msg = $"Error while trying to resolve dependencies for \"{parentType.FullName}\".\n" + string.Join(" -> ", path.Select(x => "( "+x.FullName+" )"));

		throw new CircularDependencyException(msg);
	}

	private static Type GetParentType(DINode node)
	{
		if (node.parent is null)
			return node.type;
		return GetParentType(node.parent);
	}
}