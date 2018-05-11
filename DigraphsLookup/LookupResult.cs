namespace DigraphsLookup
{
	public struct LookupResult
	{
		public string Digraph { get; set; }
		public int Count { get; set; }

		public LookupResult(string digraph, int count)
		{
			Digraph = digraph;
			Count = count;
		}

		public override string ToString()
		{
			return $"{Digraph}: {Count}";
		}
	}
}