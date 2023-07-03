using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
			yield return new BitShiftingBinarySearchDigraphsLookup();
			yield return new MemoryArraySearchDigraphsLookup();
			yield return new MemoryArraySearchDigraphsLookupV2();
			yield return new MemoryArraySearchDigraphsLookupAvx();
#if NET8_0_OR_GREATER
			yield return new MemoryArraySearchDigraphsLookupAvx512();
#endif
			yield return new MemoryArraySearchDigraphsLookupAvxParallel();
			yield return new MemoryArraySearchDigraphsLookupGpu();
		}

		[Test]
		[TestCaseSource(nameof(GetImplementations))]
		public async Task EmptyTextAlwaysReturnZeroes(IDigraphsLookup implementation)
		{
			LookupResult[] result;
			using (var stream = GivenStream(""))
			{
				result = await implementation.LookupAsync(stream, "ab", "ad", "af");
			}

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
			LookupResult[] result;
			using (var stream = GivenStream("xy"))
			{
				result = await implementation.LookupAsync(stream, "ab");
			}

			CollectionAssert.AreEquivalent(new[]
			{
				new LookupResult ("ab", 0)
			}, result);
		}

		[Test]
		[TestCaseSource(nameof(GetImplementations))]
		public async Task FindOneDigraph(IDigraphsLookup implementation)
		{
			LookupResult[] result;
			using (var stream = GivenStream("ab"))
			{
				result = await implementation.LookupAsync(stream, "ab");
			}

			CollectionAssert.AreEquivalent(new[]
			{
				new LookupResult ("ab", 1)
			}, result);
		}

		[Test]
		[TestCaseSource(nameof(GetImplementations))]
		public async Task FindOneOfTwoDigraphs(IDigraphsLookup implementation)
		{
			LookupResult[] result;
			using (var stream = GivenStream("ab"))
			{
				result = await implementation.LookupAsync(stream, "ab", "xy");
			}

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
			LookupResult[] result;
			using (var stream = GivenStream("abab"))
			{
				result = await implementation.LookupAsync(stream, "ab", "xy");
			}

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
			LookupResult[] result;
			using (var stream = GivenStream("_ab_xy_"))
			{
				result = await implementation.LookupAsync(stream, "ab", "xy");
			}

			CollectionAssert.AreEquivalent(new[]
			{
				new LookupResult ("ab", 1),
				new LookupResult ("xy", 1),
			}, result);
		}

		[Test]
		[TestCaseSource(nameof(GetImplementations))]
		public async Task Performance(IDigraphsLookup implementation)
		{
			const int warmups = 1000;
			const int runs = 1000;
			var digraphs = new[] {"ab", "ad", "af", "ag", "ar"};

			var bookPath = GetBookPath("Frankenstein");
			using (var stream = new MemoryStream(await File.ReadAllBytesAsync(bookPath)))
			{
				// Health Check
				LookupResult[] result;
				using (var copy = new MemoryStream(stream.ToArray()))
				{
					result = await implementation.LookupAsync(copy, digraphs);
				}

				CollectionAssert.AreEquivalent(new[]
				{
					new LookupResult("ab", 668),
					new LookupResult("ad", 1282),
					new LookupResult("af", 246),
					new LookupResult("ag", 659),
					new LookupResult("ar", 2593)
				}, result);

				// Warmup
				for (var i = 0; i < warmups; i++)
				{
					using (var copy = new MemoryStream(stream.ToArray()))
					{
						await implementation.LookupAsync(copy, digraphs);
					}
				}

				// Run
				var watch = new Stopwatch();
				var times = new long[runs];
				for (var i = 0; i < runs; i++)
				{
					using (var copy = new MemoryStream(stream.ToArray()))
					{
						GC.Collect();
						watch.Restart();
						await implementation.LookupAsync(copy, digraphs);
						watch.Stop();
					}

					times[i] = watch.ElapsedTicks;
				}

				WriteTimings(implementation.GetType().Name, times);
			}
		}

		private static string GetBookPath(string name)
		{
			var assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().GetName().CodeBase).LocalPath);
			var testProjectPath = Path.Combine(assemblyPath, "../../..");
			var bookPath = Path.Combine(testProjectPath, $"Books/{name}.txt");
			return bookPath;
		}

		private static void WriteTimings(string name, long[] times)
		{
			Console.WriteLine($"> Performance ({name}):");
			Console.WriteLine($">   {times.Length} runs");
			Console.WriteLine($">   {times.Sum() / Stopwatch.Frequency}s total run time");
			//WriteLine($"  Values: {string.Join(',', times)}");
			Array.Sort(times);
			Console.WriteLine($">   Average time: {ToMicroSeconds(times.Average()):0000} µs");
			Console.WriteLine(">   Percentiles (in microseconds):");
			Console.WriteLine(">   |   Min |   25% |   50% |   75% |   95% |   Max |");
			Console.WriteLine($">   | {ToMicroSeconds(times.Min()),5:####0} | {ToMicroSeconds(times[times.Length / 4]),5:####0} | {ToMicroSeconds(times[times.Length / 2]),5:####0} | {ToMicroSeconds(times[times.Length / 4 * 3]),5:####0} | {ToMicroSeconds(times[times.Length / 20 * 19]),5:####0} | {ToMicroSeconds(times.Max()),5:####0} |");
		}

		private static double ToMicroSeconds(double ticks)
		{
			return ticks / (Stopwatch.Frequency / (1000 * 1000));
		}

		private static long ToMicroSeconds(long ticks)
		{
			return ticks / (Stopwatch.Frequency / (1000 * 1000));
		}

		private static Stream GivenStream(string value)
		{
			var bytes = Encoding.UTF8.GetBytes(value);
			return new MemoryStream(bytes);
		}
	}
}
