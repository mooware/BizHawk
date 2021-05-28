using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

using BizHawk.Common;

namespace BHTest.Integration.TestRoms
{
	public static class ImageUtils
	{
		public static Bitmap AsBitmap(this Image img) => img as Bitmap ?? new Bitmap(img, img.Size);

		public static IReadOnlySet<int> CollectPalette(Image img)
		{
			var b = img.AsBitmap();
			HashSet<int> paletteE = new();
			for (int y = 0, ly = b.Height; y < ly; y++) for (int x = 0, lx = b.Width; x < lx; x++)
			{
				paletteE.Add(b.GetRawPixel(x, y));
			}
			return paletteE;
		}

		public static int GetRawPixel(this Bitmap b, int x, int y) => b.GetPixel(x, y).ToArgb() & 0xFFFFFF;

		/// <param name="map">ints are ARGB as <see cref="System.Drawing.Color.ToArgb"/></param>
		public static Bitmap PaletteSwap(Image img, IReadOnlyDictionary<int, int> map)
		{
			int Lookup(int c) => map.TryGetValue(c, out var c1) ? c1 : c;
			var b = ((Image) img.Clone()).AsBitmap();
			for (int y = 0, ly = b.Height; y < ly; y++) for (int x = 0, lx = b.Width; x < lx; x++)
			{
				b.SetPixel(x, y, Color.FromArgb(0xFF, Color.FromArgb(Lookup(b.GetRawPixel(x, y)))));
			}
			return b;
		}

		/// <remarks>
		/// using ImageMagick not because it's faster, but because everything else I do hits a bug in .NET, including:
		/// <list type="bullet">
		/// <item><description>https://github.com/dotnet/runtime/issues/1920 (ArgumentException "Parameter is not valid" at Gdip.CheckStatus at Bitmap.GetPixel)</description></item>
		/// <item><description>https://github.com/dotnet/runtime/issues/13678 / https://github.com/dotnet/runtime/issues/28069 (crash without throwing from `image.Save(path, ImageFormat.Png)`)</description></item>
		/// <item><description>B&amp;W/grayscale BMP doesn't round-trip, it becomes red and blue</description></item>
		/// <item><description>24bpp BMP (convert -type truecolor) doesn't round-trip</description></item>
		/// </list>
		/// </remarks>
		public static bool ScreenshotsEqualImageMagick(Stream expectFile, Image actual, (string Suite, string Case) id)
		{
			int? DoComparison(int hash)
			{
				var outPathActual = $"{id.Suite}/{hash:X8}_actual.bmp";
				var outPathExpect = $"{id.Suite}/{hash:X8}_expect.png";
				Task.Run(async () =>
				{
					await using FileStream fsActual = new(outPathActual, FileMode.Create);
					actual.Save(fsActual, ImageFormat.Bmp);
					await using FileStream fsExpect = new(outPathExpect, FileMode.Create);
					await expectFile.CopyToAsync(fsExpect);
					Console.WriteLine($"screenshots saved for {id.Case} as {id.Suite}/{hash:X8}_*");
				});
				var stdout = OSTailoredCode.SimpleSubshell("sh", $"compare.sh {outPathExpect} {outPathActual}", "compare.sh (ImageMagick's compare) returned nothing"); // uses `-metric AE` which returns the number of pixels that differ
				return int.TryParse(stdout, out var i) ? i : null;
			}
			// compare fails with "no such file" sporadically. Adding a Task.Delay/Thread.Sleep after the I/O calls only seem to make the failures more frequent. --yoshi
			var diffPixelCount = DoComparison(id.Case.GetHashCode()) // sadly not stable
				?? DoComparison(id.Case.GetHashCode() + 1)
				?? DoComparison(id.Case.GetHashCode() + 2)
				?? throw new Exception("compare.sh (ImageMagick's compare) failed and 2 retries also failed"); // ¯\_(ツ)_/¯
			return diffPixelCount is 0;
		}

		public static bool ScreenshotsEqualManaged(Image expect, Image actual, (string Suite, string Case) id)
		{
			bool DoComparison()
			{
				if (actual.Height != expect.Height || actual.Width != expect.Width) return false;
				var e = expect.AsBitmap();
				var a = actual.AsBitmap();
				for (int y = 0, ly = e.Height; y < ly; y++) for (int x = 0, lx = e.Width; x < lx; x++)
				{
					if (a.GetRawPixel(x, y) != e.GetRawPixel(x, y)) return false;
				}
				return true;
			}
			var areEqual = DoComparison();
			if (!areEqual)
			{
				var hash = $"{id.Case.GetHashCode():X8}"; // sadly not stable
				expect.Save($"{id.Suite}/{hash}_expect.png", ImageFormat.Png);
				actual.Save($"{id.Suite}/{hash}_actual.png", ImageFormat.Png);
				Console.WriteLine($"screenshots saved for {id.Case} as {id.Suite}/{hash}_*");
			}
			return areEqual;
		}
	}
}
