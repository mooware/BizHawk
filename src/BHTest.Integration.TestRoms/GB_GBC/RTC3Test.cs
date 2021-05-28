using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using BizHawk.Common;
using BizHawk.Common.IOExtensions;
using BizHawk.Emulation.Cores;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using static BHTest.Integration.TestRoms.GBHelper;

namespace BHTest.Integration.TestRoms
{
//	[Ignore]
	[TestClass]
	public sealed class RTC3Test
	{
		private const string SUITE_ID = "RTC3Test";

		private static readonly IReadOnlyCollection<string> KnownFailures = new[]
		{
			"",
		};

		[ClassInitialize]
		public static void BeforeAll(TestContext ctx) => TestUtils.PrepareDBAndOutput(SUITE_ID);

		[DataRow(ConsoleVariant.CGB_C, CoreNames.Gambatte)]
		[DataRow(ConsoleVariant.CGB_C, CoreNames.GbHawk)]
		[DataRow(ConsoleVariant.DMG, CoreNames.Gambatte)]
		[DataRow(ConsoleVariant.DMG, CoreNames.GbHawk)]
		[DataTestMethod]
		public void RunRTC3Test(ConsoleVariant variant, string coreName)
		{
			string DisplayName(string subTest) => $"RTC3Test.{subTest} on {variant} in {coreName}";
#if !BIZHAWKTEST_RUN_KNOWN_FAILURES
			if (new[] { "basic", "range", "subSecond" }.Select(DisplayName).All(KnownFailures.Contains))
			{
				Assert.Inconclusive("short-circuiting this test which is known to fail");
			}
#endif
			bool DoSubcaseAssertion(string subTest, Bitmap actual)
			{
				var caseStr = DisplayName(subTest);
				var screenshotMatches = ScreenshotsEqualOn(
					coreName,
					ReflectionCache.EmbeddedResourceStream($"res.rtc3test_expect_{variant}_{subTest}.png"),
					actual,
					(SUITE_ID, caseStr));
				if (!KnownFailures.Contains(caseStr))
				{
					if (screenshotMatches) return true;
					Assert.Fail("expected and actual screenshots differ");
				}
				else if (screenshotMatches)
				{
					Assert.Fail("expected and actual screenshots matched unexpectedly (this is a good thing)");
				}
				else
				{
					Console.WriteLine("expected failure, verified");
				}
				return false;
			}
			if (OSTailoredCode.IsUnixHost && coreName is CoreNames.Gambatte) Assert.Inconclusive("Gambatte unavailable on Linux");
			DummyFrontend fe = new(InitGBCore(coreName, variant, "rtc3test.gb", ReflectionCache.EmbeddedResourceStream("res.rtc3test_artifact.rtc3test.gb").ReadAllBytes()));
			fe.FrameAdvanceBy(6);
			fe.SetButton("P1 A");
			fe.FrameAdvanceBy(variant.IsColour() ? 676 : 648);
			var basicPassed = DoSubcaseAssertion("basic", fe.Screenshot());
#if true
			fe.Dispose();
			if (!basicPassed) Assert.Inconclusive(); // for this to be false, it must have been an expected failure or execution would have stopped with an Assert.Fail call
			Assert.Inconclusive("(other subtests aren't implemented)");
#else // screenshot seems to freeze emulation, or at least rendering
			fe.SetButton("P1 A");
			fe.FrameAdvanceBy(3);
			fe.SetButton("P1 Down");
			fe.FrameAdvanceBy(2);
			fe.SetButton("P1 A");
			fe.FrameAdvanceBy(429);
			var rangePassed = DoSubcaseAssertion("range", fe.Screenshot());
			fe.SetButton("P1 A");
			// didn't bother TASing the remaining menu navigation because it doesn't work
//			var subSecondPassed = DoSubcaseAssertion("subSecond", fe.Screenshot());
			fe.Dispose();
			if (!(basicPassed && rangePassed /*&& subSecondPassed*/)) Assert.Inconclusive(); // for one of these to be false, it must have been an expected failure or execution would have stopped with an Assert.Fail call
#endif
		}
	}
}
