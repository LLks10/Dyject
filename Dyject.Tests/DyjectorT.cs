using FluentAssertions;

namespace Dyject.Tests;

public class DyjectorT : IDisposable
{
	public void Dispose() => Helper.Reset();

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
}