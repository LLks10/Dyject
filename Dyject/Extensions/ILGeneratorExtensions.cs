using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

namespace Dyject.Extensions;

internal static class ILGeneratorExtensions
{
	public static ILGenerator Callvirt(this ILGenerator il, MethodInfo arg) { il.Emit(OpCodes.Callvirt, arg); return il; }
	public static ILGenerator Callvirt(this ILGenerator il, MethodInfo arg, Type[]? varargs) { il.EmitCall(OpCodes.Callvirt, arg, varargs); return il; }

	public static ILGenerator Dup(this ILGenerator il) { il.Emit(OpCodes.Dup); return il; }

	public static ILGenerator Ldarg(this ILGenerator il, int arg)
	{
		Debug.Assert(arg >= 0);
		if (arg < 4)
		{
			Span<OpCode> ops = stackalloc OpCode[] { OpCodes.Ldarg_0, OpCodes.Ldarg_1, OpCodes.Ldarg_2, OpCodes.Ldarg_3 };
			il.Emit(ops[arg]);
		}
		else if (arg <= byte.MaxValue)
			il.Emit(OpCodes.Ldarg_S, (byte)arg);
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
		else if (arg <= byte.MaxValue)
			il.Emit(OpCodes.Ldloc_S, (byte)arg);
		else
			il.Emit(OpCodes.Ldloc, arg);
		return il;
	}

	#region ldc
	public static ILGenerator Ldc(this ILGenerator il, int arg) 
	{
		if (arg >= 0 && arg < 9)
		{
			Span<OpCode> ops = stackalloc OpCode[] 
			{ 
				OpCodes.Ldc_I4_0, OpCodes.Ldc_I4_1, OpCodes.Ldc_I4_2, 
				OpCodes.Ldc_I4_3, OpCodes.Ldc_I4_4, OpCodes.Ldc_I4_5, 
				OpCodes.Ldc_I4_6, OpCodes.Ldc_I4_7, OpCodes.Ldc_I4_8, 
			};
			il.Emit(ops[arg]);
		}
		else if(arg == -1)
			il.Emit(OpCodes.Ldc_I4_M1);
		else if(arg >= sbyte.MinValue && arg <= sbyte.MaxValue)
			il.Emit(OpCodes.Ldc_I4_S, (sbyte)arg);
		else
			il.Emit(OpCodes.Ldc_I4, arg);
		return il;
	}
	public static ILGenerator Ldc(this ILGenerator il, long arg)
	{
		if (arg >= int.MinValue && arg <= int.MaxValue)
			return il.Ldc((int)arg);

		il.Emit(OpCodes.Ldc_I8, arg); 
		return il;
	}
	public static ILGenerator Ldc(this ILGenerator il, float arg) { il.Emit(OpCodes.Ldc_R4, arg); return il; }
	public static ILGenerator Ldc(this ILGenerator il, double arg)
	{
		if (arg >= float.MinValue && arg <= float.MaxValue)
			return il.Ldc((float)arg);

		il.Emit(OpCodes.Ldc_R8, arg); 
		return il;
	}
	#endregion

	public static ILGenerator Ldfld(this ILGenerator il, FieldInfo arg) { il.Emit(OpCodes.Ldfld, arg); return il; }
	public static ILGenerator Ldflda(this ILGenerator il, FieldInfo arg) { il.Emit(OpCodes.Ldflda, arg); return il; }
	public static ILGenerator Ldsfld(this ILGenerator il, FieldInfo arg) { il.Emit(OpCodes.Ldsfld, arg); return il; }
	public static ILGenerator Ldsflda(this ILGenerator il, FieldInfo arg) { il.Emit(OpCodes.Ldsflda, arg); return il; }

	public static ILGenerator Newobj(this ILGenerator il, ConstructorInfo arg) { il.Emit(OpCodes.Newobj, arg); return il; }

	public static ILGenerator Pop(this ILGenerator il) { il.Emit(OpCodes.Pop); return il; }

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
		else if (arg <= byte.MaxValue)
			il.Emit(OpCodes.Stloc_S, (byte)arg);
		else
			il.Emit(OpCodes.Stloc, arg);
		return il;
	}
}