using Dyject.Attributes;
using System.Reflection;

namespace Dyject.DyjectorHelpers;

internal class DINode
{
	public Type type;
	public FieldInfo field;
	public ConstructorInfo ctor;
	public InjScope scope;
	public int references;
	public int depth;

	public DINode parent;
	public List<DINode> children = new();
}