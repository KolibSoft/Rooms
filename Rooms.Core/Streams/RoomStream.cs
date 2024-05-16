using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KolibSoft.Rooms.Core.Protocol;

namespace KolibSoft.Rooms.Core.Streams
{
    public abstract class RoomStream : IRoomStream
    {

        public RoomStreamOptions Options { get; private set; } = new RoomStreamOptions();
        public abstract bool IsAlive { get; }
        protected bool IsDisposed => _disposed;

        protected abstract ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken token = default);

        private async ValueTask<ReadOnlyMemory<byte>> GetChunkAsync(CancellationToken token = default)
        {
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
            _data.SetLength(0);
            var done = false;
            while (true)
            {
                var chunk = await GetChunkAsync(token);
                if (_length < 1) throw new IOException("Room verb broken");
                var length = DataUtils.ScanWord(chunk.Span);
                if (length < chunk.Length)
                    length += (done = DataUtils.IsBlank(chunk.Span[length])) ? 1 : 0;
                _position += length;
                if (_data.Length + length > Options.MaxVerbLength) throw new IOException("Room verb too large");
                if (_position < _length || done)
                {
                    if (_data.Length > 0)
                    {
                        await _data.WriteAsync(chunk.Slice(0, length - 1));
                        var verb = new RoomVerb(_data.ToArray());
                        return verb;
                    }
                    if (length > 0)
                    {
                        var verb = new RoomVerb(chunk.Slice(0, length - 1).ToArray());
                        return verb;
                    }
                    return default;
                }
                await _data.WriteAsync(chunk.Slice(0, length));
            }
        }

        private async ValueTask<RoomChannel> ReadChannelAsync(CancellationToken token)
        {
            _data.SetLength(0);
            var done = false;
            while (true)
            {
                var chunk = await GetChunkAsync(token);
                if (_length < 1) throw new IOException("Room channel broken");
                var length = DataUtils.IsSign(chunk.Span[0]) ? 1 : 0;
                if (length < chunk.Length)
                    length += DataUtils.ScanHexadecimal(chunk.Slice(length).Span);
                if (length < chunk.Length)
                    length += (done = DataUtils.IsBlank(chunk.Span[length])) ? 1 : 0;
                _position += length;
                if (_data.Length + length > Options.MaxChannelLength) throw new IOException("Room channel too large");
                if (_position < _length || done)
                {
                    if (_data.Length > 0)
                    {
                        await _data.WriteAsync(chunk.Slice(0, length - 1));
                        var channel = new RoomChannel(_data.ToArray());
                        return channel;
                    }
                    if (length > 0)
                    {
                        var channel = new RoomChannel(chunk.Slice(0, length - 1).ToArray());
                        return channel;
                    }
                    return default;
                }
                await _data.WriteAsync(chunk.Slice(0, length));
            }
        }

        private async ValueTask<RoomCount> ReadCountAsync(CancellationToken token)
        {
            _data.SetLength(0);
            var done = false;
            while (true)
            {
                var chunk = await GetChunkAsync(token);
                if (_length < 1) throw new IOException("Room count broken");
                var length = DataUtils.ScanDigit(chunk.Span);
                if (length < chunk.Length)
                    length += (done = DataUtils.IsBlank(chunk.Span[length])) ? 1 : 0;
                _position += length;
                if (_data.Length + length > Options.MaxCountLength) throw new IOException("Room count too large");
                if (_position < _length || done)
                {
                    if (_data.Length > 0)
                    {
                        await _data.WriteAsync(chunk.Slice(0, length - 1));
                        var count = new RoomCount(_data.ToArray());
                        return count;
                    }
                    if (length > 0)
                    {
                        var count = new RoomCount(chunk.Slice(0, length - 1).ToArray());
                        return count;
                    }
                    return default;
                }
                await _data.WriteAsync(chunk.Slice(0, length));
            }
        }

        private async ValueTask<Stream> ReadContentAsync(CancellationToken token)
        {
            var count = await ReadCountAsync(token);
            var _count = (long)count;
            if (_count == 0) return Stream.Null;
            if (_count > Options.MaxContentLength) throw new IOException("Room content too large");
            var content = Options.CreateContentStream(_count);
            var index = 0L;
            while (index < _count)
            {
                var chunk = await GetChunkAsync(token);
                if (_length < 1) throw new IOException("Room content broken");
                var length = (int)Math.Min(chunk.Length, _count - index);
                await content.WriteAsync(chunk.Slice(0, length), token);
                index += length;
                _position += length;
            }
            content.Seek(0, SeekOrigin.Begin);
            return content;
        }

        public async ValueTask<RoomMessage> ReadMessageAsync(CancellationToken token = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RoomStream));
            var verb = await ReadVerbAsync(token);
            var channel = await ReadChannelAsync(token);
            var content = await ReadContentAsync(token);
            var message = new RoomMessage
            {
                Verb = verb.ToString(),
                Channel = (int)channel,
                Content = content
            };
            return message;
        }

        protected abstract ValueTask<int> WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken token);

        private async ValueTask WriteVerbAsync(RoomVerb verb, CancellationToken token)
        {
            if (verb.Length > Options.MaxVerbLength) throw new IOException("Room verb too large");
            var index = 0;
            while (index < verb.Length)
            {
                var length = await WriteAsync(verb.Data.Slice(index), token);
                if (length < 1) throw new IOException("Room verb broken");
                index += length;
            }
            await WriteAsync(Blank, token);
        }

        private async ValueTask WriteChannelAsync(RoomChannel channel, CancellationToken token)
        {
            if (channel.Length > Options.MaxChannelLength) throw new IOException("Room channel too large");
            var index = 0;
            while (index < channel.Length)
            {
                var length = await WriteAsync(channel.Data.Slice(index), token);
                if (length < 1) throw new IOException("Room channel broken");
                index += length;
            }
            await WriteAsync(Blank, token);
        }

        private async ValueTask WriteCountAsync(RoomCount count, CancellationToken token)
        {
            if (count.Length > Options.MaxCountLength) throw new IOException("Room count too large");
            var index = 0;
            while (index < count.Length)
            {
                var length = await WriteAsync(count.Data.Slice(index), token);
                if (length < 1) throw new IOException("Room count broken");
                index += length;
            }
            await WriteAsync(Blank, token);
        }

        private async ValueTask WriteContentAsync(Stream content, CancellationToken token)
        {
            if (content.Length > Options.MaxContentLength) throw new IOException("Room content too large");
            var count = (RoomCount)content.Length;
            await WriteCountAsync(count, token);
            content.Seek(0, SeekOrigin.Begin);
            var index = 0L;
            while (index < content.Length)
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

        public async ValueTask WriteMessageAsync(RoomMessage message, CancellationToken token = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RoomStream));
            var verb = RoomVerb.Parse(message.Verb);
            var channel = (RoomChannel)message.Channel;
            var content = message.Content;
            await WriteVerbAsync(verb, token);
            await WriteChannelAsync(channel, token);
            await WriteContentAsync(content, token);
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

        protected RoomStream(ArraySegment<byte> readBuffer, ArraySegment<byte> writeBuffer, RoomStreamOptions? options = null)
        {
            _readBuffer = readBuffer;
            _writeBuffer = writeBuffer;
            Options = options ?? new RoomStreamOptions();
        }

        private MemoryStream _data = new MemoryStream();
        private ArraySegment<byte> _readBuffer = default;
        private ArraySegment<byte> _writeBuffer = default;
        private int _position = 0;
        private int _length = 0;
        private bool _disposed = false;

        private static readonly byte[] Blank = Encoding.UTF8.GetBytes(" ");

    }
}