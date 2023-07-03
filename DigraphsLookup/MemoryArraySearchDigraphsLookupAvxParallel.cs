using System;
using System.IO;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DigraphsLookup
{
	// Author: @Socolin
	public class MemoryArraySearchDigraphsLookupAvxParallel : IDigraphsLookup
	{
		private readonly uint[] _result = new uint[256 * 256];

		private static int DigraphToIndex(string digraph)
		{
			var digraphBytes = Encoding.ASCII.GetBytes(digraph);
			return digraphBytes[0] + digraphBytes[1] * 256;
		}

		private static long RoundUpValueToBeAMultipleOf16(long value)
		{
			const int factor = 16;
			long roundedValue = (value + (factor - 1)) & ~(factor - 1);
			return roundedValue;
		}
		public async Task<LookupResult[]> LookupAsync(Stream stream, params string[] digraphs)
		{
			if (stream.Length < 2) return digraphs.Select(d => new LookupResult(d, 0)).ToArray();

			for (var i = 0; i < _result.Length; i++)
			{
				_result[i] = 0;
			}

			// '\0' is not a valid character, so we can use them to simplify code by getting an array with size % 32
			var bytes = new byte[RoundUpValueToBeAMultipleOf16(stream.Length) + 1];
			var read = 0;
			while (read < stream.Length)
				read = await stream.ReadAsync(bytes, read, (int)stream.Length);

			var memor = new ReadOnlyMemory<byte>(bytes);
			unsafe
			{
				Parallel.For(0,
					bytes.Length / 16,
					i =>
					{
						var pos = i * 16;
						fixed (byte* bytesPointer = memor.Span.Slice(pos, 16))
						{
							var vector1 = Avx2.LoadVector128(bytesPointer);
							var vector2 = Avx2.LoadVector128(bytesPointer + 1);
							var vectore1Short = Avx2.ConvertToVector256Int16(vector1).AsUInt16();
							var vector2Short = Avx2.ConvertToVector256Int16(vector2).AsUInt16();
							var vector2ShortShifted = Avx2.ShiftLeftLogical(vector2Short, 8);

							var summed = Avx2.Add(vectore1Short, vector2ShortShifted);
							for (int k = 0; k < 16; k++)
								Interlocked.Increment(ref _result[summed[k]]);
						}
					});
			}

			// The last value was added when it should not has been
			_result[bytes[stream.Length - 1]]--;

			var result = new LookupResult[digraphs.Length];
			for (var i = 0;
			     i < digraphs.Length;
			     i++)
			{
				var index = DigraphToIndex(digraphs[i]);
				result[i] = new LookupResult(digraphs[i], (int)_result[index]);
			}

			return result;
		}
	}
}