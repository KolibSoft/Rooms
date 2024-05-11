using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace KolibSoft.Rooms.Core.Protocol
{
    public abstract class RoomStream : IAsyncDisposable, IDisposable
    {

        public bool IsDisposed => _disposed;

        protected abstract ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken token);

        private async Task<Memory<byte>> GetChunkAsync(CancellationToken token)
        {
            if (_position == _length)
            {
                _position = 0;
                _length = await ReadAsync(_buffer, token);
                if (_length < 1)
                    return default;
            }
            var slice = _buffer.AsMemory().Slice(_position, _length - _position);
            return slice;
        }

        private async Task<RoomVerb> ReadVerbAsync(CancellationToken token)
        {
            var data = new MemoryStream();
            do
            {
                var chunk = await GetChunkAsync(token);
                if (_length < 1) throw new IOException("Room verb broken");
                var length = RoomVerb.Scan(chunk.Span);
                await data.WriteAsync(chunk.Slice(0, length));
                _position += length;
            } while (_position == _length);
            if (data.Length == 0) return default;
            var verb = new RoomVerb(data.ToArray());
            return verb;
        }

        private async Task<RoomChannel> ReadChannelAsync(CancellationToken token)
        {
            var data = new MemoryStream();
            do
            {
                var chunk = await GetChunkAsync(token);
                if (_length < 1) throw new IOException("Room channel broken");
                var length = RoomChannel.Scan(chunk.Span);
                await data.WriteAsync(chunk.Slice(0, length));
                _position += length;
            } while (_position == _length);
            if (data.Length == 0) return default;
            var channel = new RoomChannel(data.ToArray());
            return channel;
        }

        private async Task<RoomCount> ReadCountAsync(CancellationToken token)
        {
            var data = new MemoryStream();
            do
            {
                var chunk = await GetChunkAsync(token);
                if (_length < 1) throw new IOException("Room count broken");
                var length = RoomCount.Scan(chunk.Span);
                await data.WriteAsync(chunk.Slice(0, length));
                _position += length;
            } while (_position == _length);
            if (data.Length == 0) return default;
            var count = new RoomCount(data.ToArray());
            return count;
        }

        private async Task<RoomContent> ReadContentAsync(int count, CancellationToken token)
        {
            if (count == 0) return default;
            var data = new byte[count];
            var index = 0;
            while (index < count)
            {
                var chunk = await GetChunkAsync(token);
                if (_length < 1) throw new IOException("Room content broken");
                var length = Math.Min(chunk.Length, count - index);
                chunk.Slice(0, length).CopyTo(data.AsMemory().Slice(index, length));
                index += length;
                _position += length;
            }
            return new RoomContent(data);
        }

        public async Task ReadMessageAsync(RoomMessage message, CancellationToken token = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RoomStream));
            var verb = await ReadVerbAsync(token);
            if (verb.Length == 0) throw new IOException("Room verb not found");
            var channel = await ReadChannelAsync(token);
            if (channel.Length == 0) throw new IOException("Room channel not found");
            var count = await ReadCountAsync(token);
            if (count.Length == 0) throw new IOException("Room count not found");
            var length = (int)count;
            var content = await ReadContentAsync(length, token);
            if (content.Length != length) throw new IOException("Room content not found");
            message.Verb = verb;
            message.Channel = channel;
            message.Content = content;
        }

        protected abstract ValueTask<int> WriteAsync(Memory<byte> buffer, CancellationToken token);

        private async Task WriteVerbAsync(RoomVerb verb, CancellationToken token)
        {
            var index = 0;
            while (index < verb.Length)
            {
                var length = await WriteAsync(verb.Data.Slice(index), token);
                if (length < 1) throw new IOException("Room verb broken");
                index += length;
            }
        }

        private async Task WriteChannelAsync(RoomChannel channel, CancellationToken token)
        {
            var index = 0;
            while (index < channel.Length)
            {
                var length = await WriteAsync(channel.Data.Slice(index), token);
                if (length < 1) throw new IOException("Room channel broken");
                index += length;
            }
        }

        private async Task WriteCountAsync(RoomCount count, CancellationToken token)
        {
            var index = 0;
            while (index < count.Length)
            {
                var length = await WriteAsync(count.Data.Slice(index), token);
                if (length < 1) throw new IOException("Room count broken");
                index += length;
            }
        }

        private async Task WriteContentAsync(RoomContent content, CancellationToken token)
        {
            var index = 0;
            while (index < content.Length)
            {
                var length = await WriteAsync(content.Data.Slice(index), token);
                if (length < 1) throw new IOException("Room content broken");
                index += length;
            }
        }

        public async Task WriteMessageAsync(RoomMessage message, CancellationToken token = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RoomStream));
            if (message.Verb.Length == 0) throw new IOException("Room verb not found");
            if (message.Channel.Length == 0) throw new IOException("Room channel not found");
            var count = (RoomCount)message.Content.Length;
            await WriteVerbAsync(message.Verb, token);
            await WriteChannelAsync(message.Channel, token);
            await WriteCountAsync(count, token);
            await WriteContentAsync(message.Content, token);
        }

        protected virtual ValueTask DisposeAsync(bool disposing)
        {
            if (!_disposed)
            {
                _buffer = default;
                _disposed = true;
            }
            return ValueTask.CompletedTask;
        }

        public void Dispose()
        {
            _ = DisposeAsync(disposing: true);
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsync(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected RoomStream(ArraySegment<byte> buffer) => _buffer = buffer;

        private ArraySegment<byte> _buffer;
        private int _position = 0;
        private int _length = 0;
        private bool _disposed = false;

    }
}