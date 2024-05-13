using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace KolibSoft.Rooms.Core.Protocol
{
    public abstract class RoomStream : IRoomStream
    {

        public int MaxVerbLength { get; set; } = DefaultMaxVerbLength;
        public int MaxChannelLength { get; set; } = DefaultMaxChannelLength;
        public int MaxCountLength { get; set; } = DefaultMaxCountLength;
        public int MaxContentLength { get; set; } = DefaultMaxContentLength;
        public bool IsDisposed => _disposed;

        protected abstract ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken token = default);

        private async ValueTask<ReadOnlyMemory<byte>> GetChunkAsync(CancellationToken token = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RoomStream));
            if (_position == _length)
            {
                _position = 0;
                _length = await ReadAsync(_readBuffer, token);
                if (_length < 1)
                    return default;
            }
            var slice = _readBuffer.AsMemory().Slice(_position, _length - _position);
            return slice;
        }

        private async ValueTask<RoomVerb> ReadVerbAsync(CancellationToken token)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RoomStream));
            _data.SetLength(0);
            while (true)
            {
                var chunk = await GetChunkAsync(token);
                if (_length < 1) throw new IOException("Room verb broken");
                var length = DataUtils.ScanWord(chunk.Span);
                if (length < chunk.Length)
                    length += DataUtils.IsBlank(chunk.Span[length]) ? 1 : 0;
                _position += length;
                if (_data.Length + length > MaxVerbLength) throw new IOException("Room verb too large");
                if (_position < _length)
                {
                    if (_data.Length > 0)
                    {
                        await _data.WriteAsync(chunk.Slice(0, length));
                        return new RoomVerb(_data.ToArray());
                    }
                    if (length > 0)
                        return new RoomVerb(chunk.Slice(0, length).ToArray());
                    return default;
                }
                await _data.WriteAsync(chunk.Slice(0, length));
            }
        }

        private async ValueTask<RoomChannel> ReadChannelAsync(CancellationToken token)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RoomStream));
            _data.SetLength(0);
            while (true)
            {
                var chunk = await GetChunkAsync(token);
                if (_length < 1) throw new IOException("Room channel broken");
                var length = DataUtils.IsSign(chunk.Span[0]) ? 1 : 0;
                if (length < chunk.Length)
                    length += DataUtils.ScanHexadecimal(chunk.Slice(length).Span);
                if (length < chunk.Length)
                    length += DataUtils.IsBlank(chunk.Span[length]) ? 1 : 0;
                _position += length;
                if (_data.Length + length > MaxChannelLength) throw new IOException("Room channel too large");
                if (_position < _length)
                {
                    if (_data.Length > 0)
                    {
                        await _data.WriteAsync(chunk.Slice(0, length));
                        return new RoomChannel(_data.ToArray());
                    }
                    if (length > 0)
                        return new RoomChannel(chunk.Slice(0, length).ToArray());
                    return default;
                }
                await _data.WriteAsync(chunk.Slice(0, length));
            }
        }

        private async ValueTask<RoomCount> ReadCountAsync(CancellationToken token)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RoomStream));
            _data.SetLength(0);
            while (true)
            {
                var chunk = await GetChunkAsync(token);
                if (_length < 1) throw new IOException("Room count broken");
                var length = DataUtils.ScanDigit(chunk.Span);
                if (length < chunk.Length)
                    length += DataUtils.IsBlank(chunk.Span[length]) ? 1 : 0;
                _position += length;
                if (_data.Length + length > MaxCountLength) throw new IOException("Room count too large");
                if (_position < _length)
                {
                    if (_data.Length > 0)
                    {
                        await _data.WriteAsync(chunk.Slice(0, length));
                        return new RoomCount(_data.ToArray());
                    }
                    if (length > 0)
                        return new RoomCount(chunk.Slice(0, length).ToArray());
                    return default;
                }
                await _data.WriteAsync(chunk.Slice(0, length));
            }
        }

        public async ValueTask ReadProtocolAsync(RoomProtocol protocol, CancellationToken token = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RoomStream));
            var verb = await ReadVerbAsync(token);
            if (verb.Length == 0) throw new IOException("Room verb not found");
            var channel = await ReadChannelAsync(token);
            if (channel.Length == 0) throw new IOException("Room channel not found");
            var count = await ReadCountAsync(token);
            if (count.Length == 0) throw new IOException("Room count not found");
            protocol.Verb = verb;
            protocol.Channel = channel;
            protocol.Count = count;
        }

        public async ValueTask ReadContentAsync(long count, Stream content, CancellationToken token = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RoomStream));
            if (count == 0) return;
            if (count > MaxContentLength) throw new IOException("Room content too large");
            var index = 0L;
            while (index < count)
            {
                var chunk = await GetChunkAsync(token);
                if (_length < 1) throw new IOException("Room content broken");
                var length = (int)Math.Min(chunk.Length, count - index);
                await content.WriteAsync(chunk.Slice(0, length), token);
                index += length;
                _position += length;
            }
        }

        protected abstract ValueTask<int> WriteAsync(Memory<byte> buffer, CancellationToken token);

        private async ValueTask WriteVerbAsync(RoomVerb verb, CancellationToken token)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RoomStream));
            if (verb.Length > MaxVerbLength) throw new IOException("Room verb too large");
            var index = 0;
            while (index < verb.Length)
            {
                var length = await WriteAsync(verb.Data.AsMemory().Slice(index), token);
                if (length < 1) throw new IOException("Room verb broken");
                index += length;
            }
        }

        private async ValueTask WriteChannelAsync(RoomChannel channel, CancellationToken token)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RoomStream));
            if (channel.Length > MaxChannelLength) throw new IOException("Room channel too large");
            var index = 0;
            while (index < channel.Length)
            {
                var length = await WriteAsync(channel.Data.AsMemory().Slice(index), token);
                if (length < 1) throw new IOException("Room channel broken");
                index += length;
            }
        }

        private async ValueTask WriteCountAsync(RoomCount count, CancellationToken token)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RoomStream));
            if (count.Length > MaxCountLength) throw new IOException("Room count too large");
            var index = 0;
            while (index < count.Length)
            {
                var length = await WriteAsync(count.Data.AsMemory().Slice(index), token);
                if (length < 1) throw new IOException("Room count broken");
                index += length;
            }
        }

        public async ValueTask WriteProtocolAsync(RoomProtocol protocol, CancellationToken token = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RoomStream));
            if (protocol.Verb.Length == 0) throw new IOException("Room verb not found");
            await WriteVerbAsync(protocol.Verb, token);
            if (protocol.Channel.Length == 0) throw new IOException("Room channel not found");
            await WriteChannelAsync(protocol.Channel, token);
            if (protocol.Count.Length == 0) throw new IOException("Room count not found");
            await WriteCountAsync(protocol.Count, token);
        }

        public async ValueTask WriteContentAsync(long count, Stream content, CancellationToken token = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RoomStream));
            if (count > MaxContentLength) throw new IOException("Room content too large");
            var index = 0L;
            while (index < count)
            {
                var _count = await content.ReadAsync(_writeBuffer, token);
                var slice = _writeBuffer.Slice(0, _count);
                var _index = 0;
                while (_index < slice.Count)
                {
                    var length = await WriteAsync(slice.Slice(_index), token);
                    if (length < 1) throw new IOException("Room content broken");
                    _index += length;
                }
                index += slice.Count;
            }
        }

        protected virtual async ValueTask DisposeAsync(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                    await _data.DisposeAsync();
                _readBuffer = default;
                _writeBuffer = default;
                _disposed = true;
            }
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

        protected RoomStream(ArraySegment<byte> readBuffer, ArraySegment<byte> writeBuffer)
        {
            _readBuffer = readBuffer;
            _writeBuffer = writeBuffer;
        }

        private MemoryStream _data = new MemoryStream();
        private ArraySegment<byte> _readBuffer = default;
        private ArraySegment<byte> _writeBuffer = default;
        private int _position = 0;
        private int _length = 0;
        private bool _disposed = false;

        public const int DefaultMaxVerbLength = 128;
        public const int DefaultMaxChannelLength = 32;
        public const int DefaultMaxCountLength = 32;
        public const int DefaultMaxContentLength = 4 * 1024 * 1024;

    }
}