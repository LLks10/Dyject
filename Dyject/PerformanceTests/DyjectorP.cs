using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Dyject.Helpers;

namespace Dyject.Performance;


public class DyjectorP
{
	public static void RunPerfTest()
	{
		BenchmarkRunner.Run<DyjectorP>();
	}

	[GlobalSetup(Target = nameof(Dyject))]
	public void Setup() => TestMain.Create();

	[Benchmark]
	public TestMain Manual()
	{
		TestMain tm = new();
		var c = new ServiceC();
		var d = new ServiceD();
		d.servE = new();

		tm.serviceA = new();
		tm.serviceA.serviceB = new();
		tm.serviceA.serviceB.serC = c;
		tm.serviceA.serviceB.servD = d;
		tm.serviceA.serviceB2 = new();
		tm.serviceA.serviceB2.serC = c;
		tm.serviceA.serviceB2.servD = d;
		tm.serviceA.serC = c;
		tm.serviceA.servD = d;

		tm.serviceB = new();
		tm.serviceB.serC = c;
		tm.serviceB.servD = d;

		tm.serviceF = new();

		return tm;
	}

	[Benchmark]
	public TestMain Dyject()
	{
		return TestMain.Create();
	}
}