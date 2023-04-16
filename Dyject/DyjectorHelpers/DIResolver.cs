using Dyject.Attributes;
using Dyject.Extensions;
using System.Diagnostics;
using System.Reflection.Emit;

namespace Dyject.DyjectorHelpers;

internal class DIResolver
{
	public static Func<object> ResolveDI(DynamicMethod method, List<DINode> nodes)
	{
		DINode parent = nodes[^1];
		var ilgen = method.GetILGenerator();

		var self = new DIResolver(ilgen);
		self.Resolve(parent, 0);
		ilgen.Ret();

		return method.CreateDelegate<Func<object>>();
	}

	public static Action<object> ResolveDI4Ctor(DynamicMethod method, List<DINode> nodes)
	{
		DINode parent = nodes[^1];
		var ilgen = method.GetILGenerator();

		var self = new DIResolver(ilgen);
		self.Resolve(parent, 0, true);
		ilgen.Pop();
		ilgen.Ret();

		return method.CreateDelegate<Action<object>>();
	}

	readonly ILGenerator ilgen;
	readonly List<DINode> locals = new();
	private DIResolver(ILGenerator ilgen)
	{
		this.ilgen = ilgen;
	}

	private void Resolve(DINode current, int depth, bool getObjFromArg = false)
	{
		// Create current object
		if (!getObjFromArg)
		{
			CreateObject(current);
		}
		else
		{
			ilgen.Ldarg(0);
		}

		// Recursively iterate over children
		foreach(var child in current.children)
		{
			var field = child.Field;
			Debug.Assert(field != null);

			var node = child.Node;

			ilgen.Dup();

			switch (node.scope)
			{
				case InjScope.Transient:
					Resolve(node, depth + 1);
					break;

				case InjScope.Scoped:
					ProbeScope(node, depth + 1);
					break;

				case InjScope.Singleton:
					Dyjector.GetSingleton(node.type, ilgen);
					break;
			}

			ilgen.Stfld(field);
		}
	}

	private void CreateObject(DINode current)
	{
		var ctor = current.ctor;

		var pars = ctor.GetParameters();
		foreach(var p in pars)
		{
			// TODO: Construct service for constructor argument

			// Set default value
			if (p.HasDefaultValue)
			{
				HandleDefaultValue(p.ParameterType, p.DefaultValue);
				continue;
			}
		}

		ilgen.Newobj(ctor);
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

		Resolve(child, depth);

		if (child.references == 1)
			return;

		if (locals.Count >= ushort.MaxValue)
			throw new InvalidOperationException("Too many scoped services loaded");

		var loc = ilgen.DeclareLocal(typeof(object));
		Debug.Assert(loc.LocalIndex == locals.Count);

		ilgen
			.Dup()
			.Stloc(locals.Count);

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

	private void HandleDefaultValue(Type type, object value)
	{
		if (type.IsEnum)
		{
			type = Enum.GetUnderlyingType(type);
		}
		if (value is null)
		{
			ilgen.Ldnull();
			return;
		}
		if(type == typeof(string))
		{
			ilgen.Ldstr((string)value);
			return;
		}
		if (type == typeof(int))
		{
			ilgen.Ldc((int)value);
			return;
		}
		if (type == typeof(uint))
		{
			ilgen.Ldc((uint)value);
			return;
		}
		if (type == typeof(long))
		{
			ilgen.Ldc((long)value);
			return;
		}
		if (type == typeof(ulong))
		{
			ilgen.Ldc((ulong)value);
			return;
		}
		if (type == typeof(char))
		{
			ilgen.Ldc((char)value);
			return;
		}
		if(type == typeof(sbyte))
		{
			ilgen.Ldc((sbyte)value);
		}
		if(type == typeof(byte))
		{
			ilgen.Ldc((byte)value);
		}
		if (type == typeof(float))
		{
			ilgen.Ldc((float)value);
			return;
		}
		if (type == typeof(double))
		{
			ilgen.Ldc((double)value);
			return;
		}
		throw new NotSupportedException($"Default parameter value of type '{type.FullName}' is not supported.");
	}
}