using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace KolibSoft.Rooms.Core.Protocol
{
    public abstract class RoomStream
    {

        protected abstract Task<int> ReadAsync(Memory<byte> buffer, CancellationToken token = default);
        public async Task ReadMessageAsync(RoomMessage message, CancellationToken token = default)
        {
            // READ HEAD
            // READ CONTENT
        }

        protected abstract Task<int> WriteAsync(Memory<byte> buffer, CancellationToken token = default);
        public async Task WriteMessageAsync(RoomMessage message, CancellationToken token = default)
        {
            // WRITE HEAD
            // WRITE CONTENT
        }

    }
}