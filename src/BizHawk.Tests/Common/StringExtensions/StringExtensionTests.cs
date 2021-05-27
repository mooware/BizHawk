using Microsoft.VisualStudio.TestTools.UnitTesting;
using BizHawk.Common.StringExtensions;

namespace BizHawk.Tests.Common.StringExtensions
{
	[TestClass]
	public class StringExtensionTests
	{
		private const string abcdef = "abcdef";

		private const string qrs = "qrs";

		[TestMethod]
		public void In_CaseInsensitive()
		{
			var strArray = new[]
			{
				"Hello World"
			};

			var actual = "hello world".In(strArray);
			Assert.IsTrue(actual);
		}

		[TestMethod]
		public void TestRemovePrefix()
		{
			Assert.AreEqual("bcdef", abcdef.RemovePrefix('a', qrs));
			Assert.AreEqual(string.Empty, "a".RemovePrefix('a', qrs));
			Assert.AreEqual(qrs, abcdef.RemovePrefix('c', qrs));
			Assert.AreEqual(qrs, abcdef.RemovePrefix('x', qrs));
			Assert.AreEqual(qrs, string.Empty.RemovePrefix('a', qrs));

			Assert.AreEqual("def", abcdef.RemovePrefix("abc", qrs));
			Assert.AreEqual("bcdef", abcdef.RemovePrefix("a", qrs));
			Assert.AreEqual(abcdef, abcdef.RemovePrefix(string.Empty, qrs));
			Assert.AreEqual(string.Empty, abcdef.RemovePrefix(abcdef, qrs));
			Assert.AreEqual(string.Empty, "a".RemovePrefix("a", qrs));
			Assert.AreEqual(qrs, abcdef.RemovePrefix("c", qrs));
			Assert.AreEqual(qrs, abcdef.RemovePrefix("x", qrs));
			Assert.AreEqual(qrs, string.Empty.RemovePrefix("abc", qrs));
		}

		[TestMethod]
		public void TestRemoveSuffix()
		{
			Assert.AreEqual("abc", abcdef.RemoveSuffix("def", qrs));
			Assert.AreEqual("abcde", abcdef.RemoveSuffix("f", qrs));
			Assert.AreEqual(abcdef, abcdef.RemoveSuffix(string.Empty, qrs));
			Assert.AreEqual(string.Empty, abcdef.RemoveSuffix(abcdef, qrs));
			Assert.AreEqual(string.Empty, "f".RemoveSuffix("f", qrs));
			Assert.AreEqual(qrs, abcdef.RemoveSuffix("d", qrs));
			Assert.AreEqual(qrs, abcdef.RemoveSuffix("x", qrs));
			Assert.AreEqual(qrs, string.Empty.RemoveSuffix("def", qrs));
		}
	}
}
