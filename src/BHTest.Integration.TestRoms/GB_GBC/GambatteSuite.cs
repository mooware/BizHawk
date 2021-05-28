using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using BizHawk.Common;
using BizHawk.Common.CollectionExtensions;
using BizHawk.Common.IOExtensions;
using BizHawk.Common.StringExtensions;
using BizHawk.Emulation.Cores;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using static BHTest.Integration.TestRoms.GBHelper;

namespace BHTest.Integration.TestRoms
{
//	[Ignore]
	[TestClass]
	public sealed class GambatteSuite
	{
		public readonly struct GambatteTestCase
		{
			public readonly string CoreName;

			public readonly string ExpectEmbedPath;

			public readonly string RomEmbedPath;

			public string RomFilename => RomEmbedPath[(RomEmbedPath.LastIndexOf('.', RomEmbedPath.LastIndexOf('.')) + 1)..];

			public readonly string TestName;

			public readonly ConsoleVariant Variant;

			public GambatteTestCase(string testName, string coreName, ConsoleVariant variant, string romEmbedPath, string expectEmbedPath)
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
		private sealed class GambatteTestDataAttribute : Attribute, ITestDataSource
		{
			public IEnumerable<object?[]> GetData(MethodInfo methodInfo)
			{
				const string SUITE_PREFIX = "res.Gambatte_testroms_artifact.";
				var variants = new[] { ("_cgb04c.png", ConsoleVariant.CGB_C), ("_dmg08.png", ConsoleVariant.DMG) };
				List<GambatteTestCase> testCases = new();
				foreach (var item in ReflectionCache.EmbeddedResourceList(SUITE_PREFIX).Where(item => item.EndsWith(".png")))
				{
					var found = variants.FirstOrNull(kvp => item.EndsWith(kvp.Item1));
					if (found is null)
					{
//						Console.WriteLine($"orphan: {item}");
						continue;
					}
					var (suffix, variant) = found.Value;
					var testName = item.RemoveSuffix(suffix);
					var romEmbedPath = SUITE_PREFIX + $"{testName}.{(testName.StartsWith("dmgpalette_during_m3") ? "gb" : "gbc")}";
					var expectEmbedPath = SUITE_PREFIX + item;
					testCases.Add(new(testName, CoreNames.Gambatte, variant, romEmbedPath, expectEmbedPath));
					testCases.Add(new(testName, CoreNames.GbHawk, variant, romEmbedPath, expectEmbedPath));
				}
//				testCases.RemoveAll(testCase => testCase.Variant is not ConsoleVariant.DMG); // uncomment and modify to run a subset of the test cases
				return testCases.Select(tuple => new object?[] { tuple }); // don't bother caching this, it should only run once
			}

			public string GetDisplayName(MethodInfo methodInfo, object?[] data)
				=> $"{methodInfo.Name}({((GambatteTestCase) data[0]!).DisplayName()})";
		}

		private const string SUITE_ID = "GambatteSuite";

		private static readonly IReadOnlyCollection<string> KnownFailures = new[]
		{
			"bgtiledata.bgtiledata_spx08_1 on DMG in GBHawk",
			"bgtiledata.bgtiledata_spx08_2 on DMG in GBHawk",
			"bgtiledata.bgtiledata_spx08_3 on DMG in GBHawk",
			"bgtiledata.bgtiledata_spx08_4 on DMG in GBHawk",
			"bgtiledata.bgtiledata_spx0A_1 on DMG in GBHawk",
			"bgtiledata.bgtiledata_spx0A_2 on DMG in GBHawk",
			"bgtiledata.bgtiledata_spx0A_3 on DMG in GBHawk",
			"bgtiledata.bgtiledata_spx0A_4 on DMG in GBHawk",
			"bgtilemap.bgtilemap_spx08_ds_1 on CGB_C in GBHawk",
			"bgtilemap.bgtilemap_spx08_ds_2 on CGB_C in GBHawk",
			"bgtilemap.bgtilemap_spx08_ds_3 on CGB_C in GBHawk",
			"bgtilemap.bgtilemap_spx08_ds_4 on CGB_C in GBHawk",
			"bgtilemap.bgtilemap_spx09_1 on CGB_C in GBHawk",
			"bgtilemap.bgtilemap_spx09_2 on CGB_C in GBHawk",
			"bgtilemap.bgtilemap_spx09_3 on CGB_C in GBHawk",
			"bgtilemap.bgtilemap_spx09_4 on CGB_C in GBHawk",
			"bgtilemap.bgtilemap_spx0A_1 on DMG in GBHawk",
			"bgtilemap.bgtilemap_spx0A_2 on DMG in GBHawk",
			"bgtilemap.bgtilemap_spx0A_3 on DMG in GBHawk",
			"bgtilemap.bgtilemap_spx0A_4 on DMG in GBHawk",
			"dmgpalette_during_m3.dmgpalette_during_m3_2 on DMG in GBHawk",
			"dmgpalette_during_m3.dmgpalette_during_m3_3 on DMG in GBHawk",
			"dmgpalette_during_m3.dmgpalette_during_m3_4 on DMG in GBHawk",
			"dmgpalette_during_m3.dmgpalette_during_m3_5 on DMG in GBHawk",
			"dmgpalette_during_m3.dmgpalette_during_m3_scx1_4 on DMG in GBHawk",
			"dmgpalette_during_m3.dmgpalette_during_m3_scx2_1 on DMG in GBHawk",
			"dmgpalette_during_m3.lycint_dmgpalette_during_m3_1 on DMG in GBHawk",
			"dmgpalette_during_m3.lycint_dmgpalette_during_m3_2 on DMG in GBHawk",
			"dmgpalette_during_m3.lycint_dmgpalette_during_m3_3 on DMG in GBHawk",
			"dmgpalette_during_m3.lycint_dmgpalette_during_m3_4 on DMG in GBHawk",
			"dmgpalette_during_m3.scx3.dmgpalette_during_m3_1 on DMG in GBHawk",
			"dmgpalette_during_m3.scx3.dmgpalette_during_m3_2 on DMG in GBHawk",
			"dmgpalette_during_m3.scx3.dmgpalette_during_m3_3 on DMG in GBHawk",
			"dmgpalette_during_m3.scx3.dmgpalette_during_m3_4 on DMG in GBHawk",
			"dmgpalette_during_m3.scx3.dmgpalette_during_m3_5 on DMG in GBHawk",
			"scx_during_m3.scx1_scx0_during_m3_1 on CGB_C in GBHawk",
			"scx_during_m3.scx2_scx0_during_m3_1 on CGB_C in GBHawk",
			"scx_during_m3.scx2_scx0_during_m3_1 on DMG in GBHawk",
			"scx_during_m3.scx2_scx1_during_m3_1 on CGB_C in GBHawk",
			"scx_during_m3.scx_0063c0.scx_during_m3_1 on CGB_C in GBHawk",
			"scx_during_m3.scx_0063c0.scx_during_m3_1 on DMG in GBHawk",
			"scx_during_m3.scx_0063c0.scx_during_m3_2 on CGB_C in GBHawk",
			"scx_during_m3.scx_0063c0.scx_during_m3_2 on DMG in GBHawk",
			"scx_during_m3.scx_0063c0.scx_during_m3_ds_1 on CGB_C in GBHawk",
			"scx_during_m3.scx_0063c0.scx_during_m3_ds_2 on CGB_C in GBHawk",
			"scx_during_m3.scx_0360c0.scx_during_m3_2 on CGB_C in GBHawk",
			"scx_during_m3.scx_0360c0.scx_during_m3_2 on DMG in GBHawk",
			"scx_during_m3.scx_0360c0.scx_during_m3_3 on CGB_C in GBHawk",
			"scx_during_m3.scx_0360c0.scx_during_m3_3 on DMG in GBHawk",
			"scx_during_m3.scx_0360c0.scx_during_m3_4 on CGB_C in GBHawk",
			"scx_during_m3.scx_0360c0.scx_during_m3_4 on DMG in GBHawk",
			"scx_during_m3.scx_0360c0.scx_during_m3_5 on CGB_C in GBHawk",
			"scx_during_m3.scx_0360c0.scx_during_m3_5 on DMG in GBHawk",
			"scx_during_m3.scx_0360c0.scx_during_m3_6 on CGB_C in GBHawk",
			"scx_during_m3.scx_0360c0.scx_during_m3_6 on DMG in GBHawk",
			"scx_during_m3.scx_0360c0.scx_during_m3_ds_1 on CGB_C in GBHawk",
			"scx_during_m3.scx_0360c0.scx_during_m3_ds_2 on CGB_C in GBHawk",
			"scx_during_m3.scx_0360c0.scx_during_m3_ds_3 on CGB_C in GBHawk",
			"scx_during_m3.scx_0360c0.scx_during_m3_ds_4 on CGB_C in GBHawk",
			"scx_during_m3.scx_0360c0.scx_during_m3_ds_5 on CGB_C in GBHawk",
			"scx_during_m3.scx_0360c0.scx_during_m3_ds_6 on CGB_C in GBHawk",
			"scx_during_m3.scx_0360c0.scx_during_m3_ds_7 on CGB_C in GBHawk",
			"scx_during_m3.scx_0360c0.scx_during_m3_ds_8 on CGB_C in GBHawk",
			"scx_during_m3.scx_0363c0.scx_during_m3_1 on CGB_C in GBHawk",
			"scx_during_m3.scx_0363c0.scx_during_m3_1 on DMG in GBHawk",
			"scx_during_m3.scx_0363c0.scx_during_m3_2 on CGB_C in GBHawk",
			"scx_during_m3.scx_0363c0.scx_during_m3_2 on DMG in GBHawk",
			"scx_during_m3.scx_0363c0.scx_during_m3_3 on CGB_C in GBHawk",
			"scx_during_m3.scx_0363c0.scx_during_m3_3 on DMG in GBHawk",
			"scx_during_m3.scx_0363c0.scx_during_m3_4 on CGB_C in GBHawk",
			"scx_during_m3.scx_0363c0.scx_during_m3_4 on DMG in GBHawk",
			"scx_during_m3.scx_0363c0.scx_during_m3_5 on CGB_C in GBHawk",
			"scx_during_m3.scx_0363c0.scx_during_m3_5 on DMG in GBHawk",
			"scx_during_m3.scx_0363c0.scx_during_m3_6 on CGB_C in GBHawk",
			"scx_during_m3.scx_0363c0.scx_during_m3_6 on DMG in GBHawk",
			"scx_during_m3.scx_0363c0.scx_during_m3_ds_1 on CGB_C in GBHawk",
			"scx_during_m3.scx_0363c0.scx_during_m3_ds_2 on CGB_C in GBHawk",
			"scx_during_m3.scx_0363c0.scx_during_m3_ds_3 on CGB_C in GBHawk",
			"scx_during_m3.scx_0363c0.scx_during_m3_ds_4 on CGB_C in GBHawk",
			"scx_during_m3.scx_0363c0.scx_during_m3_ds_5 on CGB_C in GBHawk",
			"scx_during_m3.scx_0363c0.scx_during_m3_ds_6 on CGB_C in GBHawk",
			"scx_during_m3.scx_0363c0.scx_during_m3_ds_7 on CGB_C in GBHawk",
			"scx_during_m3.scx_0363c0.scx_during_m3_ds_8 on CGB_C in GBHawk",
			"scx_during_m3.scx_0367c0.scx_during_m3_1 on CGB_C in GBHawk",
			"scx_during_m3.scx_0367c0.scx_during_m3_1 on DMG in GBHawk",
			"scx_during_m3.scx_0367c0.scx_during_m3_2 on CGB_C in GBHawk",
			"scx_during_m3.scx_0367c0.scx_during_m3_2 on DMG in GBHawk",
			"scx_during_m3.scx_0367c0.scx_during_m3_3 on CGB_C in GBHawk",
			"scx_during_m3.scx_0367c0.scx_during_m3_3 on DMG in GBHawk",
			"scx_during_m3.scx_0367c0.scx_during_m3_4 on CGB_C in GBHawk",
			"scx_during_m3.scx_0367c0.scx_during_m3_4 on DMG in GBHawk",
			"scx_during_m3.scx_0367c0.scx_during_m3_5 on CGB_C in GBHawk",
			"scx_during_m3.scx_0367c0.scx_during_m3_5 on DMG in GBHawk",
			"scx_during_m3.scx_0367c0.scx_during_m3_6 on CGB_C in GBHawk",
			"scx_during_m3.scx_0367c0.scx_during_m3_6 on DMG in GBHawk",
			"scx_during_m3.scx_0367c0.scx_during_m3_ds_1 on CGB_C in GBHawk",
			"scx_during_m3.scx_0367c0.scx_during_m3_ds_2 on CGB_C in GBHawk",
			"scx_during_m3.scx_0367c0.scx_during_m3_ds_3 on CGB_C in GBHawk",
			"scx_during_m3.scx_0367c0.scx_during_m3_ds_4 on CGB_C in GBHawk",
			"scx_during_m3.scx_0367c0.scx_during_m3_ds_5 on CGB_C in GBHawk",
			"scx_during_m3.scx_0367c0.scx_during_m3_ds_6 on CGB_C in GBHawk",
			"scx_during_m3.scx_0367c0.scx_during_m3_ds_7 on CGB_C in GBHawk",
			"scx_during_m3.scx_0367c0.scx_during_m3_ds_8 on CGB_C in GBHawk",
			"scx_during_m3.scx_0761c0.scx_during_m3_1 on CGB_C in GBHawk",
			"scx_during_m3.scx_0761c0.scx_during_m3_2 on CGB_C in GBHawk",
			"scx_during_m3.scx_0761c0.scx_during_m3_2 on DMG in GBHawk",
			"scx_during_m3.scx_0761c0.scx_during_m3_3 on CGB_C in GBHawk",
			"scx_during_m3.scx_0761c0.scx_during_m3_3 on DMG in GBHawk",
			"scx_during_m3.scx_0761c0.scx_during_m3_4 on CGB_C in GBHawk",
			"scx_during_m3.scx_0761c0.scx_during_m3_4 on DMG in GBHawk",
			"scx_during_m3.scx_0761c0.scx_during_m3_5 on CGB_C in GBHawk",
			"scx_during_m3.scx_0761c0.scx_during_m3_5 on DMG in GBHawk",
			"scx_during_m3.scx_0761c0.scx_during_m3_6 on CGB_C in GBHawk",
			"scx_during_m3.scx_0761c0.scx_during_m3_6 on DMG in GBHawk",
			"scx_during_m3.scx_0761c0.scx_during_m3_ds_1 on CGB_C in GBHawk",
			"scx_during_m3.scx_0761c0.scx_during_m3_ds_2 on CGB_C in GBHawk",
			"scx_during_m3.scx_0761c0.scx_during_m3_ds_3 on CGB_C in GBHawk",
			"scx_during_m3.scx_0761c0.scx_during_m3_ds_4 on CGB_C in GBHawk",
			"scx_during_m3.scx_0761c0.scx_during_m3_ds_5 on CGB_C in GBHawk",
			"scx_during_m3.scx_0761c0.scx_during_m3_ds_6 on CGB_C in GBHawk",
			"scx_during_m3.scx_0761c0.scx_during_m3_ds_7 on CGB_C in GBHawk",
			"scx_during_m3.scx_0761c0.scx_during_m3_ds_8 on CGB_C in GBHawk",
			"scx_during_m3.scx_attrib_during_m3_spx1_ds on CGB_C in GBHawk",
			"scx_during_m3.scx_attrib_during_m3_spx2_ds on CGB_C in GBHawk",
			"scx_during_m3.scx_during_m3_spx0 on CGB_C in GBHawk",
			"scx_during_m3.scx_during_m3_spx0 on DMG in GBHawk",
			"scx_during_m3.scx_during_m3_spx1 on CGB_C in GBHawk",
			"scx_during_m3.scx_during_m3_spx1 on DMG in GBHawk",
			"scx_during_m3.scx_during_m3_spx2 on CGB_C in GBHawk",
			"scx_during_m3.scx_during_m3_spx2 on DMG in GBHawk",
			"scx_during_m3.scx_during_m3_spx2_ds on CGB_C in GBHawk",
			"scy.scx3.scy_during_m3_2 on CGB_C in GBHawk",
			"scy.scx3.scy_during_m3_4 on CGB_C in GBHawk",
			"scy.scx3.scy_during_m3_6 on CGB_C in GBHawk",
			"scy.scy_during_m3_2 on CGB_C in GBHawk",
			"scy.scy_during_m3_4 on CGB_C in GBHawk",
			"scy.scy_during_m3_6 on CGB_C in GBHawk",
			"scy.scy_during_m3_spx08_1 on CGB_C in GBHawk",
			"scy.scy_during_m3_spx08_2 on DMG in GBHawk",
			"scy.scy_during_m3_spx08_3 on CGB_C in GBHawk",
			"scy.scy_during_m3_spx08_ds_1 on CGB_C in GBHawk",
			"scy.scy_during_m3_spx08_ds_2 on CGB_C in GBHawk",
			"scy.scy_during_m3_spx08_ds_3 on CGB_C in GBHawk",
			"scy.scy_during_m3_spx08_ds_4 on CGB_C in GBHawk",
			"scy.scy_during_m3_spx09_1 on CGB_C in GBHawk",
			"scy.scy_during_m3_spx09_2 on CGB_C in GBHawk",
			"scy.scy_during_m3_spx09_3 on CGB_C in GBHawk",
			"scy.scy_during_m3_spx09_4 on CGB_C in GBHawk",
			"scy.scy_during_m3_spx0A_1 on CGB_C in GBHawk",
			"scy.scy_during_m3_spx0A_1 on DMG in GBHawk",
			"scy.scy_during_m3_spx0A_2 on DMG in GBHawk",
			"scy.scy_during_m3_spx0A_3 on CGB_C in GBHawk",
			"scy.scy_during_m3_spx0A_3 on DMG in GBHawk",
			"scy.scy_during_m3_spx0B_1 on CGB_C in GBHawk",
			"scy.scy_during_m3_spx0B_3 on CGB_C in GBHawk",
			"window.on_screen.weon_wx18_weoff_weon_wx80 on CGB_C in GBHawk",
			"window.on_screen.weon_wx18_weoff_weon_wx80 on DMG in GBHawk",
			"window.on_screen.wx17_weoff_wxA5_weon on CGB_C in GBHawk",
			"window.on_screen.wx17_weoff_wxA5_weon on DMG in GBHawk",
			"window.on_screen.wxA5_weoff_at_xposA5 on CGB_C in GBHawk",
			"window.on_screen.wxA5_weoff_at_xposA5 on DMG in GBHawk",
			"window.on_screen.wxA6_3 on DMG in GBHawk",
			"window.on_screen.wxA6_late_we_reenable_1 on DMG in GBHawk",
			"window.on_screen.wxA6_late_we_reenable_2 on CGB_C in GBHawk",
			"window.on_screen.wxA6_late_we_reenable_2 on DMG in GBHawk",
			"window.on_screen.wxA6_late_we_reenable_3 on CGB_C in GBHawk",
			"window.on_screen.wxA6_late_we_reenable_3 on DMG in GBHawk",
			"window.on_screen.wxA6_late_we_reenable_4 on CGB_C in GBHawk",
			"window.on_screen.wxA6_scx7 on DMG in GBHawk",
			"window.on_screen.wxA6_weoff_at_xposA6 on CGB_C in GBHawk",
			"window.on_screen.wxA6_weoff_at_xposA6 on DMG in GBHawk",
			"window.on_screen.wxA6_wy00 on DMG in GBHawk",
			"window.on_screen.wxA6_wy01 on DMG in GBHawk",
			"window.on_screen.wxA6_wy01_weoff_ly02 on DMG in GBHawk",
			"window.on_screen.wxA6_wy01_weoff_ly02_weon_ly60 on DMG in GBHawk",
			"window.on_screen.wxA6_wy01_wxA5_ly02 on DMG in GBHawk",
			"window.on_screen.wxA6_wy01_wxA7_ly02 on DMG in GBHawk",
			"window.on_screen.wxA6_wy8F on DMG in GBHawk",
		};

		[ClassInitialize]
		public static void BeforeAll(TestContext ctx) => TestUtils.PrepareDBAndOutput(SUITE_ID);

		[DataTestMethod]
		[GambatteTestData]
		public void RunGambatteTest(GambatteTestCase testCase)
		{
			if (OSTailoredCode.IsUnixHost && testCase.CoreName is CoreNames.Gambatte) Assert.Inconclusive("Gambatte unavailable on Linux");
			var caseStr = testCase.DisplayName();
			var knownFail = KnownFailures.Contains(caseStr);
#if !BIZHAWKTEST_RUN_KNOWN_FAILURES
			if (knownFail) Assert.Inconclusive("short-circuiting this test which is known to fail");
#endif
			var actual = DummyFrontend.RunAndScreenshot(
				InitGBCore(testCase.CoreName, testCase.Variant, testCase.RomFilename, ReflectionCache.EmbeddedResourceStream(testCase.RomEmbedPath).ReadAllBytes()),
				fe => fe.FrameAdvanceUntil(() => false, fe.FrameCount + 14)); // just timeout
			var screenshotMatches = ScreenshotsEqualOn(testCase.CoreName, ReflectionCache.EmbeddedResourceStream(testCase.ExpectEmbedPath), actual, (SUITE_ID, caseStr));
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
