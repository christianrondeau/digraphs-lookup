using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DigraphsLookup.Tests
{
	public class LookupTests
	{
		public static IEnumerable<IDigraphsLookup> GetImplementations()
		{
			yield return new SubstringPairsDigraphsLookup();
		}

		[Test]
		[TestCaseSource(nameof(GetImplementations))]
		public async Task EmptyTextAlwaysReturnZeroes(IDigraphsLookup implementation)
		{
			var stream = GivenStream("");
			var result = await implementation.LookupAsync(stream, "ab", "ad", "af");

			CollectionAssert.AreEquivalent(new[]
			{
				new LookupResult ("ab", 0),
				new LookupResult ("ad", 0),
				new LookupResult ("af", 0)
			}, result);
		}

		[Test]
		[TestCaseSource(nameof(GetImplementations))]
		public async Task DigraphNotFound(IDigraphsLookup implementation)
		{
			var stream = GivenStream("xy");
			var result = await implementation.LookupAsync(stream, "ab");

			CollectionAssert.AreEquivalent(new[]
			{
				new LookupResult ("ab", 0)
			}, result);
		}

		[Test]
		[TestCaseSource(nameof(GetImplementations))]
		public async Task FindOneDigraph(IDigraphsLookup implementation)
		{
			var stream = GivenStream("ab");
			var result = await implementation.LookupAsync(stream, "ab", "xy");

			CollectionAssert.AreEquivalent(new[]
			{
				new LookupResult ("ab", 1),
				new LookupResult ("xy", 0),
			}, result);
		}

		[Test]
		[TestCaseSource(nameof(GetImplementations))]
		public async Task FindOneDigraphTwice(IDigraphsLookup implementation)
		{
			var stream = GivenStream("abab");
			var result = await implementation.LookupAsync(stream, "ab", "xy");

			CollectionAssert.AreEquivalent(new[]
			{
				new LookupResult ("ab", 2),
				new LookupResult ("xy", 0),
			}, result);
		}

		[Test]
		[TestCaseSource(nameof(GetImplementations))]
		public async Task FindTwoDigraphs(IDigraphsLookup implementation)
		{
			var stream = GivenStream("_ab_xy_");
			var result = await implementation.LookupAsync(stream, "ab", "xy");

			CollectionAssert.AreEquivalent(new[]
			{
				new LookupResult ("ab", 1),
				new LookupResult ("xy", 1),
			}, result);
		}

		private static Stream GivenStream(string value)
		{
			var bytes = Encoding.UTF8.GetBytes(value);
			return new MemoryStream(bytes);
		}
	}
}
