using Microsoft.IO;
using System.Buffers;

namespace Pomegranate.Transport.WebSocket
{
    public static class Pools
    {
        public static readonly ArrayPool<byte> _BUFFER_POOL = ArrayPool<byte>.Shared;
        public static readonly RecyclableMemoryStreamManager _MEMORY_STREAM_POOL = new();
    }
}
