using Dyject;
using Dyject.Helpers;
using System.Reflection;
using System.Reflection.Emit;

// TODO
// scoped + singleton
// constructor injection
// Depender ctor
// specify concretions
// Add logic to decide which constructor to select

Test.Run();

Console.ReadLine();


namespace Dyject
{
	internal static class Test
	{
		public static void Run()
		{
			var a = TestMain.Create();
			var b = TestMain.Create();
			return;
		}

		public static TestMain Example()
		{
			TestMain tm = new TestMain();
			var sA = new ServiceA();
			var sB = new ServiceB();
			sA.serviceB = sB;
			tm.serviceB = TestMain.serviceB_test;

			var a = Dyjector.resolver[typeof(TestMain)];

			Console.WriteLine("lol");

			return (TestMain)a();
		}
	}

	internal class Scrub : Hacker
	{
		private int myInt = 2;
	}

	internal abstract class Hacker
	{
		public Hacker()
		{
			var t = GetType();
			var hackz = new DynamicMethod("inject", null, new Type[]{typeof(object), typeof(int)}, t, false);
			var ilgen = hackz.GetILGenerator();

			ilgen.Emit(OpCodes.Ldarg_0);
			ilgen.Emit(OpCodes.Ldarg_1);
			ilgen.Emit(OpCodes.Stfld, t.GetField("myInt", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
			ilgen.Emit(OpCodes.Ret);

			var func = hackz.CreateDelegate<Action<object, int>>();
			func(this, 5);
		}
	}
}