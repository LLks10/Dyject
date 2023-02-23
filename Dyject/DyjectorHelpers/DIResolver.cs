using System.Diagnostics;
using System.Reflection.Emit;

namespace Dyject.DyjectorHelpers;

internal static class DIResolver
{
	public static DynamicMethod ResolveDI(DynamicMethod method, List<DINode> nodes)
	{
		DINode parent = nodes[^1];
		var ilgen = method.GetILGenerator();

		Resolve(ilgen, parent, 0);
		ilgen.Emit(OpCodes.Ret);

		return method;
	}

	private static void Resolve(ILGenerator ilgen, DINode current, int depth)
	{
		var ctors = current.type.GetConstructors();
		if (ctors.Length > 1)
			throw new NotImplementedException("Services with multiple constructors not implemented");

		var ctor = ctors[0];

		ilgen.Emit(OpCodes.Newobj, ctor);
		foreach(var child in current.children)
		{
			var field = child.field;
			Debug.Assert(child.field != null);

			ilgen.Emit(OpCodes.Dup);
			Resolve(ilgen, child, depth + 1);
			ilgen.Emit(OpCodes.Stfld, field);
		}
	}
}