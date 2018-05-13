using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DigraphsLookup
{
	public class ShortShiftingDigraphsLookup : IDigraphsLookup
	{
		public async Task<LookupResult[]> LookupAsync(Stream stream, params string[] digraphs)
		{
			if (stream.Length < 2) return digraphs.Select(d => new LookupResult(d, 0)).ToArray();

			var accumulator = new LookupAccumulator(digraphs);

			var bytes = new byte[stream.Length];
			await stream.ReadAsync(bytes, 0, (int)stream.Length);

			var current = (short)bytes[0];
			for (var i = 1; i < bytes.Length; i++)
			{
				current = (short) (bytes[i] + (current << 8));
				if (accumulator.TryGetValue(current, out var entry))
					entry.Count++;
			}

			return accumulator.Select(d => new LookupResult(d.Value.Digraph, d.Value.Count)).ToArray();
		}

		public class LookupAccumulator : Dictionary<short, LookupAccumulatorEntry>
		{
			public LookupAccumulator(IEnumerable<string> digraphs)
			{
				foreach (var digraph in digraphs)
				{
					var key = (short)(digraph[1] + 256 * digraph[0]);
					var entry = new LookupAccumulatorEntry{Digraph = digraph};
					Add(key, entry);
				}
			}
		}

		public class LookupAccumulatorEntry
		{
			public string Digraph;
			public int Count;
		}
	}
}
