using Dyject.Attributes;
using Dyject.Extensions;
using System.Diagnostics;
using System.Reflection.Emit;

namespace Dyject.DyjectorHelpers;

internal class DIResolver
{
	public static Func<object> Resolve(DynamicMethod method, DINode node)
	{
		var ilgen = method.GetILGenerator();

		var self = new DIResolver(ilgen);
		self.Resolve(node);
		ilgen.Ret();

		return method.CreateDelegate<Func<object>>();
	}

	public static Action<object> Resolve4Ctor(DynamicMethod method, DINode node)
	{
		var ilgen = method.GetILGenerator();

		var self = new DIResolver(ilgen);
		self.Resolve(node, true);
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

	private void Resolve(DINode current, bool getObjFromArg = false)
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
					Resolve(node);
					break;

				case InjScope.Scoped:
					ProbeScope(node);
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

		foreach((var param, var node) in current.args)
		{
			// Set default value
			if (param.HasDefaultValue)
			{
				HandleDefaultValue(param.ParameterType, param.DefaultValue);
				continue;
			}

			switch (node.scope)
			{
				case InjScope.Transient:
					Resolve(node);
					break;

				case InjScope.Scoped:
					ProbeScope(node);
					break;

				case InjScope.Singleton:
					Dyjector.GetSingleton(node.type, ilgen);
					break;
			}
		}

		ilgen.Newobj(ctor);
	}

	private void ProbeScope(DINode child)
	{
		Debug.Assert(child.scope == InjScope.Scoped);

		var idx = GetLocalVarIdx(child);
		if(idx is { } i)
		{
			ilgen.Ldloc(i);
			return;
		}

		Resolve(child);

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
			type = Enum.GetUnderlyingType(type);

		switch (value)
		{
			case null	: ilgen.Ldnull();			  break;
			case string	: ilgen.Ldstr((string)value); break;
			case long	: ilgen.Ldc((long)value);	  break;
			case ulong	: ilgen.Ldc((ulong)value);	  break;
			case int	: ilgen.Ldc((int)value);	  break;
			case uint	: ilgen.Ldc((uint)value);	  break;
			case short	: ilgen.Ldc((short)value);	  break;
			case ushort	: ilgen.Ldc((ushort)value);	  break;
			case sbyte	: ilgen.Ldc((sbyte)value);	  break;
			case byte	: ilgen.Ldc((byte)value);	  break;
			case char	: ilgen.Ldc((char)value);	  break;
			case double	: ilgen.Ldc((double)value);	  break;
			case float	: ilgen.Ldc((float)value);	  break;
			default: throw new NotSupportedException($"Default parameter value of type '{type.FullName}' is not supported.");
		}
	}
}