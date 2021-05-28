using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using BizHawk.Common;
using BizHawk.Common.IOExtensions;
using BizHawk.Common.StringExtensions;
using BizHawk.Emulation.Common;
using BizHawk.Emulation.Cores;
using BizHawk.Emulation.Cores.Nintendo.Gameboy;
using BizHawk.Emulation.Cores.Nintendo.GBHawk;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using static BHTest.Integration.TestRoms.GBHelper;

namespace BHTest.Integration.TestRoms
{
//	[Ignore]
	[TestClass]
	public sealed class MealybugTearoomTests
	{
		public readonly struct MealybugTestCase
		{
			public readonly string CoreName;

			public readonly string ExpectEmbedPath;

			public readonly string RomEmbedPath;

			public readonly string TestName;

			public readonly ConsoleVariant Variant;

			public MealybugTestCase(string testName, string coreName, ConsoleVariant variant, string romEmbedPath, string expectEmbedPath)
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
		private sealed class MealybugTestDataAttribute : Attribute, ITestDataSource
		{
			public IEnumerable<object?[]> GetData(MethodInfo methodInfo)
			{
				const string SUITE_PREFIX = "res.mealybug_tearoom_tests_artifact.";
				var variants = new[] { ("expected.CPU_CGB_C.", ConsoleVariant.CGB_C), ("expected.CPU_CGB_D.", ConsoleVariant.CGB_D), ("expected.DMG_blob.", ConsoleVariant.DMG), ("expected.DMG_CPU_B.", ConsoleVariant.DMG_B) };
				List<MealybugTestCase> testCases = new();
				foreach (var item in ReflectionCache.EmbeddedResourceList(SUITE_PREFIX).Where(item => item.EndsWith(".png")))
				{
					var (prefix, variant) = variants.First(kvp => item.StartsWith(kvp.Item1));
					var testName = item.RemovePrefix(prefix).RemoveSuffix(".png");
					var romEmbedPath = SUITE_PREFIX + $"build.ppu.{testName}.gb";
					var expectEmbedPath = SUITE_PREFIX + item;
					testCases.Add(new(testName, CoreNames.Gambatte, variant, romEmbedPath, expectEmbedPath));
					testCases.Add(new(testName, CoreNames.GbHawk, variant, romEmbedPath, expectEmbedPath));
				}
				// expected value is a "no screenshot available" message
				testCases.RemoveAll(testCase =>
					testCase.Variant is ConsoleVariant.CGB_C or ConsoleVariant.CGB_D
						&& testCase.TestName is "m3_lcdc_win_en_change_multiple_wx" or "m3_wx_4_change" or "m3_wx_5_change" or "m3_wx_6_change");
				// these are identical to CGB_C
				testCases.RemoveAll(testCase =>
					testCase.Variant is ConsoleVariant.CGB_D
						&& testCase.TestName is "m2_win_en_toggle" or "m3_lcdc_bg_en_change" or "m3_lcdc_bg_map_change" or "m3_lcdc_obj_en_change" or "m3_lcdc_obj_size_change" or "m3_lcdc_obj_size_change_scx" or "m3_lcdc_tile_sel_change" or "m3_lcdc_tile_sel_win_change" or "m3_lcdc_win_en_change_multiple" or "m3_lcdc_win_map_change" or "m3_scx_high_5_bits" or "m3_scx_low_3_bits" or "m3_wx_4_change" or "m3_wx_4_change_sprites" or "m3_wx_5_change" or "m3_wx_6_change");

//				testCases.RemoveAll(testCase => testCase.Variant is not ConsoleVariant.DMG); // uncomment and modify to run a subset of the test cases
				return testCases.Select(tuple => new object?[] { tuple }); // don't bother caching this, it should only run once
			}

			public string GetDisplayName(MethodInfo methodInfo, object?[] data)
				=> $"{methodInfo.Name}({((MealybugTestCase) data[0]!).DisplayName()})";
		}

		private const string SUITE_ID = "Mealybug";

		private static readonly IReadOnlyCollection<string> KnownFailures = new[]
		{
			"m3_bgp_change on CGB_C in GBHawk", "m3_bgp_change on CGB_D in GBHawk", "m3_bgp_change on DMG in GBHawk",
			"m3_bgp_change_sprites on CGB_C in GBHawk", "m3_bgp_change_sprites on CGB_D in GBHawk", "m3_bgp_change_sprites on DMG in GBHawk",
			"m3_lcdc_bg_en_change on DMG in GBHawk", "m3_lcdc_bg_en_change on DMG_B in GBHawk",
			"m3_lcdc_bg_map_change on CGB_C in GBHawk", "m3_lcdc_bg_map_change on DMG in GBHawk",
			"m3_lcdc_bg_map_change2 on CGB_C in GBHawk",
			"m3_lcdc_obj_en_change on DMG in GBHawk",
			"m3_lcdc_obj_en_change_variant on CGB_C in GBHawk", "m3_lcdc_obj_en_change_variant on CGB_D in GBHawk", "m3_lcdc_obj_en_change_variant on DMG in GBHawk",
			"m3_lcdc_obj_size_change on CGB_C in GBHawk", "m3_lcdc_obj_size_change on DMG in GBHawk",
			"m3_lcdc_obj_size_change_scx on CGB_C in GBHawk", "m3_lcdc_obj_size_change_scx on DMG in GBHawk",
			"m3_lcdc_tile_sel_change on CGB_C in GBHawk", "m3_lcdc_tile_sel_change on DMG in GBHawk",
			"m3_lcdc_tile_sel_change2 on CGB_C in GBHawk",
			"m3_lcdc_tile_sel_win_change on CGB_C in GBHawk", "m3_lcdc_tile_sel_win_change on DMG in GBHawk",
			"m3_lcdc_tile_sel_win_change2 on CGB_C in GBHawk",
			"m3_lcdc_win_en_change_multiple on CGB_C in GBHawk", "m3_lcdc_win_en_change_multiple on DMG in GBHawk",
			"m3_lcdc_win_en_change_multiple_wx on DMG in GBHawk", "m3_lcdc_win_en_change_multiple_wx on DMG_B in GBHawk",
			"m3_lcdc_win_map_change on CGB_C in GBHawk", "m3_lcdc_win_map_change on DMG in GBHawk",
			"m3_lcdc_win_map_change2 on CGB_C in GBHawk",
			"m3_obp0_change on CGB_C in GBHawk", "m3_obp0_change on CGB_D in GBHawk", "m3_obp0_change on DMG in GBHawk",
			"m3_scx_high_5_bits on CGB_C in GBHawk", "m3_scx_high_5_bits on DMG in GBHawk",
			"m3_scx_high_5_bits_change2 on CGB_C in GBHawk",
			"m3_scy_change on CGB_C in GBHawk", "m3_scy_change on CGB_D in GBHawk", "m3_scy_change on DMG in GBHawk",
			"m3_scy_change2 on CGB_C in GBHawk",
			"m3_window_timing on CGB_C in GBHawk", "m3_window_timing on CGB_D in GBHawk", "m3_window_timing on DMG in GBHawk",
			"m3_window_timing_wx_0 on CGB_C in GBHawk", "m3_window_timing_wx_0 on CGB_D in GBHawk", "m3_window_timing_wx_0 on DMG in GBHawk",
			"m3_wx_4_change on DMG in GBHawk",
			"m3_wx_4_change_sprites on CGB_C in GBHawk", "m3_wx_4_change_sprites on DMG in GBHawk",
			"m3_wx_5_change on DMG in GBHawk",
			"m3_wx_6_change on DMG in GBHawk",
		};

		[ClassInitialize]
		public static void BeforeAll(TestContext ctx) => TestUtils.PrepareDBAndOutput(SUITE_ID);

		[DataTestMethod]
		[MealybugTestData]
		public void RunMealybugTest(MealybugTestCase testCase)
		{
			if (OSTailoredCode.IsUnixHost && testCase.CoreName is CoreNames.Gambatte) Assert.Inconclusive("Gambatte unavailable on Linux");
			var caseStr = testCase.DisplayName();
			var knownFail = KnownFailures.Contains(caseStr);
#if !BIZHAWKTEST_RUN_KNOWN_FAILURES
			if (knownFail) Assert.Inconclusive("short-circuiting this test which is known to fail");
#endif
			var actual = DummyFrontend.RunAndScreenshot(
				InitGBCore(testCase.CoreName, testCase.Variant, $"{testCase.TestName}.gb", ReflectionCache.EmbeddedResourceStream(testCase.RomEmbedPath).ReadAllBytes()),
				fe =>
				{
					var domain = fe.CoreAsMemDomains!.SystemBus!;
					Func<long> derefPC = fe.Core switch
					{
						Gameboy => () => domain.PeekByte((long) fe.CoreAsDebuggable!.GetCpuFlagsAndRegisters()["PC"].Value),
						GBHawk gbHawk => () => domain.PeekByte(gbHawk.cpu.RegPC),
						_ => throw new Exception()
					};
					var finished = false;
					fe.CoreAsDebuggable!.MemoryCallbacks.Add(new MemoryCallback(
						domain.Name,
						MemoryCallbackType.Execute,
						"breakpoint",
						(_, _, _) =>
						{
							if (!finished && derefPC() is 0x40) finished = true;
						},
						address: null, // all addresses
						mask: null));
					Assert.IsTrue(fe.FrameAdvanceUntil(() => finished), "timed out waiting for exec hook");
				});
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
