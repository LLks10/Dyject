using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

namespace Dyject.Extensions;

internal static class ILGeneratorExtensions
{
	public static ILGenerator Dup(this ILGenerator il) { il.Emit(OpCodes.Dup); return il; }

	public static ILGenerator Ldarg(this ILGenerator il, int arg)
	{
		Debug.Assert(arg >= 0);
		if (arg < 4)
		{
			Span<OpCode> ops = stackalloc OpCode[] { OpCodes.Ldarg_0, OpCodes.Ldarg_1, OpCodes.Ldarg_2, OpCodes.Ldarg_3 };
			il.Emit(ops[arg]);
		}
		else if (arg < 256)
			il.Emit(OpCodes.Ldarg_S, arg);
		else
			il.Emit(OpCodes.Ldarg, arg);
		return il;
	}

	public static ILGenerator Ldloc(this ILGenerator il, int arg)
	{
		Debug.Assert(arg >= 0);
		if (arg < 4)
		{
			Span<OpCode> ops = stackalloc OpCode[] { OpCodes.Ldloc_0, OpCodes.Ldloc_1, OpCodes.Ldloc_2, OpCodes.Ldloc_3 };
			il.Emit(ops[arg]);
		}
		else if (arg < 256)
			il.Emit(OpCodes.Ldloc_S, arg);
		else
			il.Emit(OpCodes.Ldloc, arg);
		return il;
	}

	public static ILGenerator Newobj(this ILGenerator il, ConstructorInfo arg) { il.Emit(OpCodes.Newobj, arg); return il; }

	public static ILGenerator Ret(this ILGenerator il) { il.Emit(OpCodes.Ret); return il; }

	public static ILGenerator Stfld(this ILGenerator il, FieldInfo arg) { il.Emit(OpCodes.Stfld, arg); return il; }
	public static ILGenerator Stsfld(this ILGenerator il, FieldInfo arg) { il.Emit(OpCodes.Stsfld, arg); return il; }

	public static ILGenerator Stloc(this ILGenerator il, int arg)
	{
		Debug.Assert(arg >= 0);
		if (arg < 4)
		{
			Span<OpCode> ops = stackalloc OpCode[] { OpCodes.Stloc_0, OpCodes.Stloc_1, OpCodes.Stloc_2, OpCodes.Stloc_3 };
			il.Emit(ops[arg]);
		}
		else if (arg < 256)
			il.Emit(OpCodes.Stloc_S, arg);
		else
			il.Emit(OpCodes.Stloc, arg);
		return il;
	}
}