using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DigraphsLookup
{
	public class BitShiftingBinarySearchDigraphsLookup : IDigraphsLookup
	{
		public async Task<LookupResult[]> LookupAsync(Stream stream, params string[] digraphs)
		{
			Array.Sort(digraphs);
			if (stream.Length < 2) return digraphs.Select(d => new LookupResult(d, 0)).ToArray();

			var counts = new int[digraphs.Length];
			var digraphInts = new int[digraphs.Length];
			var digraphsMaxIndex = digraphInts.Length - 1;
			var smallest = int.MaxValue;
			var largest = int.MinValue;
			for (var i = 0; i < digraphs.Length; i++)
			{
				var digraph = digraphs[i];
				var digraphInt = digraph[1] + 256 * digraph[0];
				digraphInts[i] = digraphInt;
				if (digraphInt < smallest) smallest = digraphInt;
				if (digraphInt > largest) largest = digraphInt;
			}

			var bytes = new byte[stream.Length];
			await stream.ReadAsync(bytes, 0, (int)stream.Length);

			var current = (int)bytes[0];
			for (var stringIndex = 1; stringIndex < bytes.Length; stringIndex++)
			{
				current = (bytes[stringIndex] + (current << 8)) & 0x0000FFFF;
				if (current > largest || current < smallest) continue;
				var digraphIndex = BinarySearch(digraphInts, digraphsMaxIndex, current);
				if (digraphIndex >= 0) counts[digraphIndex]++;
			}

			var result = new LookupResult[digraphs.Length];
			for(var i = 0; i < digraphs.Length; i++)
				result[i] = new LookupResult(digraphs[i], counts[i]);
			return result;
		}

		// https://github.com/dotnet/coreclr/blob/85374ceaed177f71472cc4c23c69daf7402e5048/src/System.Private.CoreLib/src/System/Collections/Generic/ArraySortHelper.cs#L430
		// To avoid boxing, calls to Comparer, calls to CompareTo and null checks, I inlined the Array.BinarySearch algo here
		private static int BinarySearch(int[] array, int maxIndex, int value)
		{
			var lo = 0;
			var hi = maxIndex;
			while (lo <= hi)
			{
				var i = lo + ((hi - lo) >> 1);
				var order = array[i] - value;

				if (order == 0)
					return i;

				if (order < 0)
					lo = i + 1;
				else
					hi = i - 1;
			}
			return ~lo;
		}
	}
}
