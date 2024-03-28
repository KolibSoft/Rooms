using System;
using System.Text;

namespace KolibSoft.Rooms.Core.Protocol
{

    public class RoomBuffer : IDisposable
    {

        private bool disposed;

        public ArraySegment<byte> Buffer { get; private set; }

        private RoomVerb? verb;
        public RoomVerb Verb
        {
            get
            {
                if (disposed) throw new ObjectDisposedException(null);
                if (verb == null)
                {
                    var end = RoomVerb.Scan(Buffer[0..]);
                    verb = new RoomVerb(Buffer[0..end]);
                }
                return verb.Value;
            }
            set
            {
                if (disposed) throw new ObjectDisposedException(null);
                value.data.CopyTo(Buffer[0..]);
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
                    var start = ChannelOffset;
                    var end = start + RoomChannel.Scan(Buffer[start..]);
                    channel = new RoomChannel(Buffer[start..end]);
                }
                return channel.Value;
            }
            set
            {
                if (disposed) throw new ObjectDisposedException(null);
                var start = ChannelOffset;
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
                    var start = LengthOffset;
                    var end = start + RoomLength.Scan(Buffer[start..]);
                    length = new RoomLength(Buffer[start..end]);
                }
                return length.Value;
            }
            set
            {
                if (disposed) throw new ObjectDisposedException(null);
                var start = LengthOffset;
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
                    var start = ContentOffset;
                    var end = start + (int)Length;
                    content = new RoomContent(Buffer[start..end]);
                }
                return content.Value;
            }
            set
            {
                if (disposed) throw new ObjectDisposedException(null);
                var start = ContentOffset;
                value.data.CopyTo(Buffer[start..]);
            }
        }

        public override string ToString()
        {
            var end = Verb.data.Count + Channel.data.Count + Length.data.Count + Content.data.Count;
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