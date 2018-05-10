using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace DigraphsLookup.Tests
{
	public class LookupTests
	{
		public static IEnumerable<ILookup> GetImplementations()
		{
			yield return new NaiveLookup();
		}

		[TestCaseSource(nameof(GetImplementations))]
		public void EmptyTextAlwaysReturnZeroes(ILookup implementation)
		{
			var stream = GivenStream("");
			var result = implementation.Lookup(stream, "ab", "ad", "af");

			CollectionAssert.AreEquivalent(new LookupResult
			{
				{"ab", 0},
				{"ad", 0},
				{"af", 0}
			}, result);
		}

		private static Stream GivenStream(string value)
		{
			var bytes = Encoding.UTF8.GetBytes(value);
			return new MemoryStream(bytes);
		}
	}
}
