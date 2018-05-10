using System.IO;

namespace DigraphsLookup
{
	public class NaiveLookup : ILookup
	{
		public LookupResult Lookup(Stream stream, params string[] digraphs)
		{
			return new LookupResult();
		}
	}
}