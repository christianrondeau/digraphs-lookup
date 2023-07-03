using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigraphsLookup
{
	// Author: @Socolin
	public class MemoryArraySearchDigraphsLookupV2 : IDigraphsLookup
	{
		private readonly int[] _result = new int[256 * 256];

		private static int DigraphToIndex(string digraph)
		{
			var digraphBytes = Encoding.ASCII.GetBytes(digraph);
			return digraphBytes[0] + digraphBytes[1] * 256;
		}

		public async Task<LookupResult[]> LookupAsync(Stream stream, params string[] digraphs)
		{
			if (stream.Length < 2) return digraphs.Select(d => new LookupResult(d, 0)).ToArray();

			for (var i = 0; i < _result.Length; i++)
			{
				_result[i] = 0;
			}

			var bytes = new byte[stream.Length];
			var read = 0;
			while (read < stream.Length)
				read = await stream.ReadAsync(bytes, read, (int)stream.Length);

			var first = bytes[0];
			for (var i = 1; i < bytes.Length; i++)
			{
				var second = bytes[i];

				_result[first + second * 256]++;

				first = second;
			}

			var result = new LookupResult[digraphs.Length];
			for (var i = 0; i < digraphs.Length; i++)
			{
				var index = DigraphToIndex(digraphs[i]);
				result[i] = new LookupResult(digraphs[i], _result[index]);
			}
			return result;
		}
	}
}