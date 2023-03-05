using Dyject.Exceptions;
using Dyject.Tests.Helpers;
using Dyject.Tests.TestData.Dyjector;
using FluentAssertions;

namespace Dyject.Tests;

[Collection("Dyject")]
public class DyjectorT : IDisposable
{
	void IDisposable.Dispose() => Helper.Reset();

	[Fact]
	public void EmptyObject()
	{
		TestEmpty test = null;
		Action act = () => test = Dyjector.Resolve<TestEmpty>();

		act.Should().NotThrow();
		test.Should().NotBeNull();
		test.Id.Should().Be(0);

		TestEmpty test2 = null;
		Action act2 = () => test2 = Dyjector.Resolve<TestEmpty>();

		act2.Should().NotThrow();
		test2.Should().NotBeNull();
		test.Id.Should().Be(0);
		test2.Id.Should().Be(1);
	}

	[Fact]
	public void T0()
	{
		T0Main test = Dyjector.Resolve<T0Main>();

		test.FieldInjectionValidate();

		T0Main test2 = Dyjector.Resolve<T0Main>();

		test.FieldInjectionValidate();
		test2.FieldInjectionValidate();
	}

	[Fact]
	public void CircularDependency()
	{
		Func<Circ> circ = () => Dyjector.Resolve<Circ>();

		circ.Should().ThrowExactly<CircularDependencyException>();
	}

	[Fact]
	public void SharedSingleton()
	{
		ShSiMain0 test0 = Dyjector.Resolve<ShSiMain0>();
		ShSiMain1 test1 = Dyjector.Resolve<ShSiMain1>();
		ShSiMain0 test2 = Dyjector.Resolve<ShSiMain0>();
		ShSiMain1 test3 = Dyjector.Resolve<ShSiMain1>();

		test0.FieldInjectionValidate();
		test1.FieldInjectionValidate();
		test2.FieldInjectionValidate();
		test3.FieldInjectionValidate();
	}
}