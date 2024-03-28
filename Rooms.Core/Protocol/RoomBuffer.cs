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
                value.data.CopyTo(Buffer[start..]);
            }
        }

        private int ChannelOffset => Verb.data.Offset + Verb.data.Count;
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
                Buffer.AsSpan()[ChannelOffset] = (byte)' ';
                var start = ChannelOffset + 1;
                value.data.CopyTo(Buffer[start..]);
            }
        }

        private int LengthOffset => Channel.data.Offset + Channel.data.Count;
        private RoomLength? length;
        public RoomLength Length
        {
            get
            {
                if (disposed) throw new ObjectDisposedException(null);
                if (length == null)
                {
                    var start = BlankOffset(LengthOffset);
                    var end = start + RoomLength.Scan(Buffer[start..]);
                    length = new RoomLength(Buffer[start..end]);
                }
                return length.Value;
            }
            set
            {
                if (disposed) throw new ObjectDisposedException(null);
                Buffer.AsSpan()[LengthOffset] = (byte)' ';
                var start = LengthOffset + 1;
                value.data.CopyTo(Buffer[start..]);
            }
        }

        private int ContentOffset => Length.data.Offset + Length.data.Count;
        private RoomContent? content;
        public RoomContent Content
        {
            get
            {
                if (disposed) throw new ObjectDisposedException(null);
                if (content == null)
                {
                    var start = BlankOffset(ContentOffset);
                    var end = start + (int)Length;
                    content = new RoomContent(Buffer[start..end]);
                }
                return content.Value;
            }
            set
            {
                if (disposed) throw new ObjectDisposedException(null);
                Buffer.AsSpan()[ContentOffset] = (byte)'\n';
                var start = ContentOffset + 1;
                value.data.CopyTo(Buffer[start..]);
            }
        }

        private int BlankOffset(int offset)
        {
            var index = offset;
            while (index < Buffer.Count && Lookup((char)Buffer[index]))
                index++;
            return index;
            static bool Lookup(char c) => char.IsWhiteSpace(c);
        }

        public override string ToString()
        {
            var end = Content.data.Offset + Content.data.Count;
            var text = Encoding.UTF8.GetString(Buffer[0..end]);
            return text;
        }

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