using Dyject.Attributes;
using Dyject.Extensions;
using System.Diagnostics;
using System.Reflection.Emit;

namespace Dyject.DyjectorHelpers;

internal class DIResolver
{
	public static DynamicMethod ResolveDI(DynamicMethod method, List<DINode> nodes)
	{
		DINode parent = nodes[^1];
		var ilgen = method.GetILGenerator();

		var self = new DIResolver(ilgen);
		self.Resolve(parent, 0);
		ilgen.Ret();

		return method;
	}

	readonly ILGenerator ilgen;
	readonly List<DINode> locals = new();
	private DIResolver(ILGenerator ilgen)
	{
		this.ilgen = ilgen;
	}

	private void Resolve(DINode current, int depth)
	{
		var ctors = current.type.GetConstructors();
		if (ctors.Length > 1)
			throw new NotImplementedException("Services with multiple constructors not implemented");

		var ctor = ctors[0];
		ilgen.Newobj(ctor);

		foreach(var child in current.children)
		{
			var field = child.field;
			Debug.Assert(field != null);

			ilgen.Dup();

			switch (child.scope)
			{
				case InjScope.Transient:
					Resolve(child, depth + 1);
					break;

				case InjScope.Scoped:
					ProbeScope(child, depth + 1);
					break;
			}

			ilgen.Stfld(field);
		}
	}

	private void ProbeScope(DINode child, int depth)
	{
		Debug.Assert(child.scope == InjScope.Scoped);

		var idx = GetLocalVarIdx(child);
		if(idx is { } i)
		{
			ilgen.Ldloc(i);
			return;
		}

		if (locals.Count >= ushort.MaxValue)
			throw new InvalidOperationException("Too large dependency tree");

		Resolve(child, depth);

		ilgen.DeclareLocal(child.type);
		ilgen
			.Dup()
			.Stloc((ushort)locals.Count);

		locals.Add(child);
	}

	private ushort? GetLocalVarIdx(DINode node)
	{
		for(ushort i = 0; i < locals.Count; i++)
		{
			if (locals[i] == node)
				return i;
		}
		return null;
	}
}