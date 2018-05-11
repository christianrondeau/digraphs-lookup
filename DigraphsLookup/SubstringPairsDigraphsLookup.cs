using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DigraphsLookup
{
	public class SubstringPairsDigraphsLookup : IDigraphsLookup
	{
		public async Task<LookupResult[]> LookupAsync(Stream stream, params string[] digraphs)
		{
			var dict = digraphs.ToDictionary(d => d, d => 0);

			string text;
			using (var reader = new StreamReader(stream))
				text = await reader.ReadToEndAsync();

			for (var i = 0; i < text.Length - 1; i++)
			{
				var digraph = text.Substring(i, 2);
				if (dict.ContainsKey(digraph))
					dict[digraph]++;
			}

			var result = dict.Select(e => new LookupResult(e.Key, e.Value)).ToArray();
			return result;
		}
	}
}