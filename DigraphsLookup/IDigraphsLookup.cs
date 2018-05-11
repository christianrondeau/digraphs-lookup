using System.IO;
using System.Threading.Tasks;

namespace DigraphsLookup
{
	public interface IDigraphsLookup
	{
		Task<LookupResult[]> LookupAsync(Stream stream, params string[] digraphs);
	}
}
