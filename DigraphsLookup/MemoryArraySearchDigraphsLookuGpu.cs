using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ILGPU;
using ILGPU.Runtime;

namespace DigraphsLookup
{
	// Author: @Socolin
	public class MemoryArraySearchDigraphsLookupGpu : IDigraphsLookup, IDisposable
	{
		private readonly int[] _result = new int[256 * 256];
		private readonly Context _context;
		private readonly Accelerator _accelerator;
		private readonly Action<Index1D, ArrayView<byte>, ArrayView<int>> _loadedKernel;

		public MemoryArraySearchDigraphsLookupGpu()
		{
			_context = Context.Create(builder => builder.AllAccelerators());
			var device = _context.GetPreferredDevice(preferCPU: false);
			_accelerator = device.CreateAccelerator(_context);
			_loadedKernel = _accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView<byte>, ArrayView<int>>(Kernel);
		}

		private static int DigraphToIndex(string digraph)
		{
			var digraphBytes = Encoding.ASCII.GetBytes(digraph);
			return digraphBytes[0] + digraphBytes[1] * 256;
		}

		public async Task<LookupResult[]> LookupAsync(Stream stream, params string[] digraphs)
		{
			if (stream.Length < 2) return digraphs.Select(d => new LookupResult(d, 0)).ToArray();

			var bytes = new byte[stream.Length];
			var read = 0;
			while (read < stream.Length)
				read = await stream.ReadAsync(bytes, read, (int)stream.Length);

			var bytesMemoryBuffer = _accelerator.Allocate1D(bytes);
			bytesMemoryBuffer.CopyFromCPU(bytes);
			var resultMemoryBuffer = _accelerator.Allocate1D(_result);
			resultMemoryBuffer.CopyFromCPU(_result);

			_loadedKernel((int)bytesMemoryBuffer.Length - 1, bytesMemoryBuffer.View, resultMemoryBuffer.View);
			_accelerator.Synchronize();

			resultMemoryBuffer.CopyToCPU(_result);

			var result = new LookupResult[digraphs.Length];
			for (var i = 0; i < digraphs.Length; i++)
			{
				var index = DigraphToIndex(digraphs[i]);
				result[i] = new LookupResult(digraphs[i], _result[index]);
			}

			return result;
		}

		private static void Kernel(Index1D index, ArrayView<byte> bytes, ArrayView<int> result)
		{
			var resultIndex = bytes[index.X] + 256 * bytes[index.X + 1];
			Interlocked.Increment(ref result[resultIndex]);
		}

		public void Dispose()
		{
			_accelerator.Dispose();
			_context.Dispose();
		}
	}
}