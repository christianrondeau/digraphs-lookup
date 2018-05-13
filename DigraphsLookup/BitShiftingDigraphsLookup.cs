using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DigraphsLookup
{
	public class BitShiftingDigraphsLookup : IDigraphsLookup
	{
		public async Task<LookupResult[]> LookupAsync(Stream stream, params string[] digraphs)
		{
			Array.Sort(digraphs);
			if (stream.Length < 2) return digraphs.Select(d => new LookupResult(d, 0)).ToArray();

			var accumulator = new LookupAccumulatorEntry[digraphs.Length];
			var accumulatorLength = accumulator.Length;
			var smallest = int.MaxValue;
			var largest = int.MinValue;
			for (var i = 0; i < accumulatorLength; i++)
			{
				var digraph = digraphs[i];
				var digraphInt = digraph[1] + 256 * digraph[0];
				accumulator[i] = new LookupAccumulatorEntry {DigraphInt = digraphInt, Digraph = digraph};
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
				for (var entryIndex = 0; entryIndex < accumulatorLength; entryIndex++)
				{
					var entry = accumulator[entryIndex];
					if (entry.DigraphInt == current)
					{
						entry.Count++;
						break;
					}
				}
			}

			return accumulator.Select(d => new LookupResult(d.Digraph, d.Count)).ToArray();
		}

		public class LookupAccumulatorEntry
		{
			public string Digraph;
			public int DigraphInt;
			public int Count;
		}
	}
}
