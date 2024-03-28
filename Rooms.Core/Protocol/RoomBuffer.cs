using System;
using System.Text;

namespace KolibSoft.Rooms.Core.Protocol
{

    public class RoomBuffer : IDisposable
    {
        private bool disposed;

        public ArraySegment<byte> Buffer { get; private set; }

        private int VerbOffset => 0;
        private RoomVerb? verb;
        public RoomVerb Verb
        {
            get
            {
                if (disposed) throw new ObjectDisposedException(null);
                if (verb == null)
                {
                    var start = BlankOffset(VerbOffset);
                    var end = start + RoomVerb.Scan(Buffer[start..]);
                    verb = new RoomVerb(Buffer[start..end]);
                }
                return verb.Value;
            }
            set
            {
                if (disposed) throw new ObjectDisposedException(null);
                var start = VerbOffset;
                var end = start + value.data.Count;
                verb = new RoomVerb(Buffer[start..end]);
                value.data.CopyTo(verb.Value.data);
                Buffer.AsSpan()[end] = (byte)' ';
            }
        }

        private int ChannelOffset => Verb.data.Offset + Verb.data.Count + 1;
        private RoomChannel? channel;
        public RoomChannel Channel
        {
            get
            {
                if (disposed) throw new ObjectDisposedException(null);
                if (channel == null)
                {
                    var start = BlankOffset(ChannelOffset);
                    var end = start + RoomChannel.Scan(Buffer[start..]);
                    channel = new RoomChannel(Buffer[start..end]);
                }
                return channel.Value;
            }
            set
            {
                if (disposed) throw new ObjectDisposedException(null);
                var start = ChannelOffset;
                var end = start + value.data.Count;
                channel = new RoomChannel(Buffer[start..end]);
                value.data.CopyTo(channel.Value.data);
                Buffer.AsSpan()[end] = (byte)' ';
            }
        }

        private int ContentOffset => Channel.data.Offset + Channel.data.Count + 1;
        private RoomContent? content;
        public RoomContent Content
        {
            get
            {
                if (disposed) throw new ObjectDisposedException(null);
                if (content == null)
                {
                    var start = BlankOffset(ContentOffset);
                    var end = start + RoomContent.Scan(Buffer[start..]);
                    content = new RoomContent(Buffer[start..end]);
                }
                return content.Value;
            }
            set
            {
                if (disposed) throw new ObjectDisposedException(null);
                var start = ContentOffset;
                var end = start + value.data.Count;
                content = new RoomContent(Buffer[start..end]);
                value.data.CopyTo(content.Value.data);
            }
        }

        private int BlankOffset(int offset)
        {
            var index = offset;
            while (index < Buffer.Count && char.IsWhiteSpace((char)Buffer[index]))
                index++;
            return index;
        }

        public override string ToString() => Encoding.UTF8.GetString(Buffer.Slice(0, Content.data.Offset + Content.data.Count));

        public RoomBuffer(ArraySegment<byte> buffer)
        {
            Buffer = buffer;
        }

        public RoomBuffer(int buffering = 1024)
        {
            Buffer = new byte[buffering];
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                Buffer = default;
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

    }

}