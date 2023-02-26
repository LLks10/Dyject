using Dyject.Attributes;
using Dyject.Exceptions;
using System.Reflection;

namespace Dyject.Tests.Helpers;

public static class Validator
{
	public static void FieldInjectionValidate(this object obj)
	{
		HashSet<int> transients = new();
		Dictionary<Type, object> scope = new();

		Val(obj, transients, scope);
	}

	private static void Val(object obj, HashSet<int> transients, Dictionary<Type, object> scope)
	{
		var flds = obj.GetType().GetFields(Dyjector.fieldsFlags);
		
		foreach(var fld in flds)
		{
			var attr = fld.FieldType.GetCustomAttribute<Injectable>();
			if (attr is null)
			{
				if (fld.GetValue(obj) is not null)
					throw new ValidationException($"Field {fld.Name} of object {obj.GetType()} is initialized despite not being marked as {nameof(Injectable)}");
				continue;
			}
			if (fld.GetCustomAttribute<DontInject>() is not null)
			{
				if (fld.GetValue(obj) is not null)
					throw new ValidationException($"Field {fld.Name} of object {obj.GetType()} is initialized despite being marked as {nameof(DontInject)}");
				continue;
			}
			if (fld.FieldType == obj.GetType())
				throw new InvalidDependencyException($"Implementing self as dependency is not allowed. \"{obj.GetType().FullName} -> {fld.Name}\".");

			// Transient validation
			if(attr.Scope == InjScope.Transient)
			{
				var scpobj = (Identifier)fld.GetValue(obj)!;
				if(transients.Contains(scpobj.Id))
					throw new ValidationException($"Field {fld.Name} of object {obj.GetType()} is exists twice despite being marked as {nameof(InjScope.Transient)}");
				transients.Add(scpobj.Id);
				Val((Identifier)fld.GetValue(obj)!, transients, scope);
				return;
			}

			// Scoped validation
			if (attr.Scope == InjScope.Scoped)
			{
				if(scope.TryGetValue(fld.FieldType, out var scopeObj))
				{
					if (scopeObj != obj)
						throw new ValidationException($"Field {fld.Name} of object {obj.GetType()} is initialized twice despite being marked as {nameof(InjScope.Scoped)}");
					return;
				}

				scope.Add(fld.FieldType, fld.GetValue(obj)!);
				Val((Identifier)fld.GetValue(obj)!, transients, scope);
				return;
			}

			// Singleton validation
			if (attr.Scope == InjScope.Singleton)
			{
				var single = Dyjector.TryGetSingleton(fld.FieldType);
				if (fld.GetValue(obj) != single)
					throw new ValidationException($"Field {fld.Name} of object {obj.GetType()} is initialized twice despite being marked as {nameof(InjScope.Singleton)}");

				FieldInjectionValidate((Identifier)single);
				return;
			}

			throw new System.Diagnostics.UnreachableException();
		}
	}
}

internal class ValidationException : Exception
{
	public ValidationException(string message) : base(message)
	{

	}
}