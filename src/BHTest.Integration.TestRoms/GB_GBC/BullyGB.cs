using System.Collections.Generic;
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
	public sealed class BullyGB
	{
		private const string SUITE_ID = "BullyGB";

		private static readonly IReadOnlyCollection<string> KnownFailures = new[]
		{
			"BullyGB on CGB_C in GBHawk",
		};

		[ClassInitialize]
		public static void BeforeAll(TestContext ctx) => TestUtils.PrepareDBAndOutput(SUITE_ID);

		[DataTestMethod]
		[DataRow(ConsoleVariant.CGB_C, CoreNames.Gambatte)]
		[DataRow(ConsoleVariant.CGB_C, CoreNames.GbHawk)]
		[DataRow(ConsoleVariant.DMG, CoreNames.Gambatte)]
		[DataRow(ConsoleVariant.DMG, CoreNames.GbHawk)]
		public void RunBullyTest(ConsoleVariant variant, string coreName)
		{
			if (OSTailoredCode.IsUnixHost && coreName is CoreNames.Gambatte) Assert.Inconclusive("Gambatte unavailable on Linux");
			var caseStr = $"BullyGB on {variant} in {coreName}";
			var knownFail = KnownFailures.Contains(caseStr);
#if !BIZHAWKTEST_RUN_KNOWN_FAILURES
			if (knownFail) Assert.Inconclusive("short-circuiting this test which is known to fail");
#endif
			var actual = DummyFrontend.RunAndScreenshot(
				InitGBCore(coreName, variant, "bully.gbc", ReflectionCache.EmbeddedResourceStream("res.BullyGB_artifact.bully.gb").ReadAllBytes()),
				fe => fe.FrameAdvanceUntil(() => false, fe.FrameCount + 18)); // just timeout
			var screenshotMatches = ScreenshotsEqualOn(
				coreName,
				ReflectionCache.EmbeddedResourceStream("res.BullyGB_expect.png"),
				actual,
				(SUITE_ID, caseStr));
			if (!knownFail)
			{
				Assert.IsTrue(screenshotMatches, "expected and actual screenshots differ");
			}
			else if (screenshotMatches)
			{
				Assert.Fail("expected and actual screenshots matched unexpectedly (this is a good thing)");
			}
			else
			{
				Assert.Inconclusive("expected failure, verified");
			}
		}
	}
}
