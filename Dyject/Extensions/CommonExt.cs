using System.Runtime.CompilerServices;

namespace Dyject.Extensions;

internal static class CommonExt
{
	public static T As<T>(this object from) where T : class => Unsafe.As<T>(from)!;
}