using System.IO;

using BizHawk.Emulation.Common;

namespace BHTest.Integration.TestRoms
{
	public static class TestUtils
	{
		private static bool _initialised = false;

		public static void PrepareDBAndOutput(string suiteID)
		{
			if (_initialised) return;
			_initialised = true;
			Database.InitializeDatabase(Path.Combine(".", "gamedb", "gamedb.txt"), warnForCollisions: false); // runs in the background; required for Database.GetGameInfo calls by GBHelper
			DirectoryInfo di = new(suiteID);
			if (di.Exists) di.Delete(recursive: true);
			di.Create();
		}
	}
}
