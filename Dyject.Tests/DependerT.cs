using Dyject.Exceptions;
using Dyject.Tests.Helpers;
using Dyject.Tests.TestData.Depender;
using FluentAssertions;

namespace Dyject.Tests;

[Collection("Dyject")]
public class DependerT : IDisposable
{
	void IDisposable.Dispose() => Helper.Reset();

	[Fact]
	public void T0()
	{
		T0Main test = new();

		test.FieldInjectionValidate();

		T0Main test2 = new();

		test.FieldInjectionValidate();
		test2.FieldInjectionValidate();
	}

	[Fact]
	public void CircularDependency()
	{
		Func<Circ> circ = () => new();

		circ.Should().ThrowExactly<CircularDependencyException>();
	}

	[Fact]
	public void SharedSingleton()
	{
		ShSiMain0 test0 = new();
		ShSiMain1 test1 = new();
		ShSiMain0 test2 = new();
		ShSiMain1 test3 = new();

		test0.FieldInjectionValidate();
		test1.FieldInjectionValidate();
		test2.FieldInjectionValidate();
		test3.FieldInjectionValidate();
	}
}