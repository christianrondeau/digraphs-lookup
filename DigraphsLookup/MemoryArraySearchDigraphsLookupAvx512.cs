#if NET8_0_OR_GREATER
using System;
using System.IO;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace DigraphsLookup
{
	// Author: @Socolin
	public class MemoryArraySearchDigraphsLookupAvx512 : IDigraphsLookup
	{
		private readonly uint[] _result = new uint[256 * 256];

		private static int DigraphToIndex(string digraph)
		{
			var digraphBytes = Encoding.ASCII.GetBytes(digraph);
			return digraphBytes[0] + digraphBytes[1] * 256;
		}

		private static long RoundUpValueToBeAMultipleOf32(long value)
		{
			const int factor = 32;
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
			var bytes = new byte[RoundUpValueToBeAMultipleOf32(stream.Length) + 1];
			var read = 0;
			while (read < stream.Length)
				read = await stream.ReadAsync(bytes, read, (int)stream.Length);
			unsafe
			{
				fixed (byte* bytesPointer = new ReadOnlySpan<byte>(bytes))
				{
					var i = 0;
					while (i < bytes.Length)
					{
						var vector1 = Avx512BW.LoadVector256(i + bytesPointer);
						var vector2 = Avx512BW.LoadVector256(i + bytesPointer + 1);
						var vectore1Short = Avx512BW.ConvertToVector512Int16(vector1).AsUInt16();
						var vector2Short = Avx512BW.ConvertToVector512Int16(vector2).AsUInt16();
						var vector2ShortShifted = Avx512BW.ShiftLeftLogical(vector2Short, 8);

						var summed = Avx512BW.Add(vectore1Short, vector2ShortShifted);
						for (int k = 0; k < 32; k++)
							_result[summed[k]]++;
						i += 32;
					}

					// The last value was added when it should not has been
					_result[bytes[stream.Length - 1]]--;
				}
			}

			var result = new LookupResult[digraphs.Length];
			for (var i = 0; i < digraphs.Length; i++)
			{
				var index = DigraphToIndex(digraphs[i]);
				result[i] = new LookupResult(digraphs[i], (int)_result[index]);
			}

			return result;
		}
	}
}
#endif