using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using BizHawk.Common;
using BizHawk.Common.IOExtensions;
using BizHawk.Emulation.Cores;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using static BHTest.Integration.TestRoms.GBHelper;

namespace BHTest.Integration.TestRoms
{
//	[Ignore]
	[TestClass]
	public sealed class AcidTestroms
	{
		public readonly struct AcidTestCase
		{
			public readonly string CoreName;

			public readonly string ExpectEmbedPath;

			public readonly string RomEmbedPath;

			public readonly string TestName;

			public readonly ConsoleVariant Variant;

			public AcidTestCase(string testName, string coreName, ConsoleVariant variant, string romEmbedPath, string expectEmbedPath)
			{
				TestName = testName;
				Variant = variant;
				CoreName = coreName;
				RomEmbedPath = romEmbedPath;
				ExpectEmbedPath = expectEmbedPath;
			}

			public readonly string DisplayName() => $"{TestName} on {Variant} in {CoreName}";
		}

		[AttributeUsage(AttributeTargets.Method)]
		private sealed class AcidTestDataAttribute : Attribute, ITestDataSource
		{
			public IEnumerable<object?[]> GetData(MethodInfo methodInfo)
			{
				var testCases = new AcidTestCase[]
				{
					new("cgb-acid-hell", CoreNames.GbHawk, ConsoleVariant.CGB_C, "res.cgb_acid_hell_artifact.cgb-acid-hell.gbc", "res.cgb_acid_hell_artifact.reference.png"),
					new("cgb-acid2", CoreNames.GbHawk, ConsoleVariant.CGB_C, "res.cgb_acid2_artifact.cgb-acid2.gbc", "res.cgb_acid2_artifact.reference.png"),
					new("dmg-acid2", CoreNames.GbHawk, ConsoleVariant.CGB_C, "res.dmg_acid2_artifact.dmg-acid2.gb", "res.dmg_acid2_artifact.reference-cgb.png"),
					new("dmg-acid2", CoreNames.GbHawk, ConsoleVariant.DMG, "res.dmg_acid2_artifact.dmg-acid2.gb", "res.dmg_acid2_artifact.reference-dmg.png"),
				};
				return testCases
					.SelectMany(tc => new[] { new(tc.TestName, CoreNames.Gambatte, tc.Variant, tc.RomEmbedPath, tc.ExpectEmbedPath), tc })
					.Select(tuple => new object?[] { tuple }); // don't bother caching this, it should only run once
			}

			public string GetDisplayName(MethodInfo methodInfo, object?[] data)
				=> $"{methodInfo.Name}({((AcidTestCase) data[0]!).DisplayName()})";
		}

		private const string SUITE_ID = "AcidTestroms";

		private static readonly IReadOnlyCollection<string> KnownFailures = new[]
		{
			"",
		};

		[ClassInitialize]
		public static void BeforeAll(TestContext ctx) => TestUtils.PrepareDBAndOutput(SUITE_ID);

		[AcidTestData]
		[DataTestMethod]
		public void RunAcidTest(AcidTestCase testCase)
		{
			if (OSTailoredCode.IsUnixHost && testCase.CoreName is CoreNames.Gambatte) Assert.Inconclusive("Gambatte unavailable on Linux");
			var caseStr = testCase.DisplayName();
			var knownFail = KnownFailures.Contains(caseStr);
#if !BIZHAWKTEST_RUN_KNOWN_FAILURES
			if (knownFail) Assert.Inconclusive("short-circuiting this test which is known to fail");
#endif
			var actual = DummyFrontend.RunAndScreenshot(
				InitGBCore(testCase.CoreName, testCase.Variant, $"{testCase.TestName}.gbc", ReflectionCache.EmbeddedResourceStream(testCase.RomEmbedPath).ReadAllBytes()),
				fe => fe.FrameAdvanceUntil(() => false, fe.FrameCount + 15)); // just timeout
			var screenshotMatches = ScreenshotsEqualOn(
				testCase.CoreName,
				ReflectionCache.EmbeddedResourceStream(testCase.ExpectEmbedPath),
				ImageUtils.PaletteSwap(actual, MattCurriePaletteMap),
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
