using System.IO;

namespace DigraphsLookup
{
	public interface ILookup
	{
		LookupResult Lookup(Stream stream, params string[] digraphs);
	}
}
