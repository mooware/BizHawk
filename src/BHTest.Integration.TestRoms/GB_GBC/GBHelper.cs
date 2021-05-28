using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

using BizHawk.Common;
using BizHawk.Emulation.Common;
using BizHawk.Emulation.Cores;
using BizHawk.Emulation.Cores.Nintendo.Gameboy;
using BizHawk.Emulation.Cores.Nintendo.GBHawk;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using static BizHawk.Emulation.Cores.Nintendo.Gameboy.Gameboy;
using static BizHawk.Emulation.Cores.Nintendo.GBHawk.GBHawk;

namespace BHTest.Integration.TestRoms
{
	public static class GBHelper
	{
		public enum ConsoleVariant { CGB_C, CGB_D, DMG, DMG_B }

		private static readonly GambatteSyncSettings GambatteSyncSettings_GB_NOBIOS = new() { ConsoleMode = GambatteSyncSettings.ConsoleModeType.GB };

		private static readonly GambatteSyncSettings GambatteSyncSettings_GB_USEBIOS = new() { ConsoleMode = GambatteSyncSettings.ConsoleModeType.GB, EnableBIOS = true };

		private static readonly GambatteSyncSettings GambatteSyncSettings_GBC_NOBIOS = new() { ConsoleMode = GambatteSyncSettings.ConsoleModeType.GBC };

		private static readonly GambatteSyncSettings GambatteSyncSettings_GBC_USEBIOS = new() { ConsoleMode = GambatteSyncSettings.ConsoleModeType.GBC, EnableBIOS = true };

		private static readonly GBSyncSettings GBHawkSyncSettings_GB = new() { ConsoleMode = GBSyncSettings.ConsoleModeType.GB };

		private static readonly GBSyncSettings GBHawkSyncSettings_GBC = new() { ConsoleMode = GBSyncSettings.ConsoleModeType.GBC };

		public static readonly IReadOnlyDictionary<int, int> MattCurriePaletteMap = new Dictionary<int, int>
		{
			[0x0F3EAA] = 0x0000FF,
			[0x137213] = 0x009C00,
			[0x187890] = 0x0063C6,
			[0x695423] = 0x737300,
			[0x7BC8D5] = 0x6BBDFF,
			[0x7F3848] = 0x943939,
			[0x83C656] = 0x7BFF31,
			[0x9D7E34] = 0xADAD00,
			[0xE18096] = 0xFF8484,
			[0xE8BA4D] = 0xFFFF00,
			[0xF8F8F8] = 0xFFFFFF,
		};

		private static bool AddEmbeddedGBBIOS(this DummyFrontend.EmbeddedFirmwareProvider efp, ConsoleVariant variant)
			=> variant.IsColour()
				? efp.AddIfExists(new("GBC", "World"), false ? "res.fw.GBC__World__AGB.bin" : "res.fw.GBC__World__CGB.bin")
				: efp.AddIfExists(new("GB", "World"), "res.fw.GB__World__DMG.bin");

		public static GambatteSyncSettings GetGambatteSyncSettings(ConsoleVariant variant, bool biosAvailable)
			=> biosAvailable
				? variant.IsColour()
					? GambatteSyncSettings_GBC_USEBIOS
					: GambatteSyncSettings_GB_USEBIOS
				: variant.IsColour()
					? GambatteSyncSettings_GBC_NOBIOS
					: GambatteSyncSettings_GB_NOBIOS;

		public static GBSyncSettings? GetGBHawkSyncSettings(ConsoleVariant variant, bool biosAvailable)
			=> biosAvailable
				? variant.IsColour()
					? GBHawkSyncSettings_GBC
					: GBHawkSyncSettings_GB
				: null;

		public static DummyFrontend.ClassInitCallbackDelegate InitGBCore(string coreName, ConsoleVariant variant, string romFilename, byte[] rom)
			=> (efp, _, coreComm) =>
			{
				var biosAvailable = efp.AddEmbeddedGBBIOS(variant);
				var game = Database.GetGameInfo(rom, romFilename);
				IEmulator newCore;
				switch (coreName)
				{
					case CoreNames.Gambatte:
						newCore = new Gameboy(coreComm, game, rom, new(), GetGambatteSyncSettings(variant, biosAvailable), deterministic: true);
						break;
					case CoreNames.GbHawk:
						var ss = GetGBHawkSyncSettings(variant, biosAvailable);
						if (ss is null)
						{
							Assert.Inconclusive("GBHawk needs BIOS");
							throw new Exception(); // never hit
						}
						newCore = new GBHawk(coreComm, game, rom, new(), ss);
						break;
					default:
						throw new Exception();
				}
				var biosWaitDuration = biosAvailable
					? variant.IsColour()
						? 186
						: 334
					: 0;
				return (newCore, biosWaitDuration);
			};

		public static bool IsColour(this ConsoleVariant variant)
			=> variant is ConsoleVariant.CGB_C or ConsoleVariant.CGB_D;

		public static bool ScreenshotsEqualOn(string coreName, Stream expectFile, Image? actual, (string Suite, string Case) id)
		{
			if (actual is null) Assert.Fail("actual screenshot was null");
#if false // for debugging palette differences
			static string F(Image img) => string.Join(", ", ImageUtils.CollectPalette(img).Select(i => $"{i:X6}"));
			Assert.Fail($"\ne: {F(Image.FromStream(expectFile))}\na: {F(actual!)}");
#endif
			return OSTailoredCode.IsUnixHost
				? ImageUtils.ScreenshotsEqualImageMagick(expectFile, actual!, id)
				: ImageUtils.ScreenshotsEqualManaged(Image.FromStream(expectFile), actual!, id);
		}
	}
}
