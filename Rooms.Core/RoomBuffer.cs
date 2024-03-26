using System;

namespace KolibSoft.Rooms.Core
{

    /// <summary>
    /// Buffer to store one message at a time.
    /// </summary>
    public class RoomBuffer : IDisposable
    {

        /// <summary>
        /// Disposed flag.
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// Internal buffer to store a message at a time.
        /// </summary>
        public ArraySegment<byte> Buffer { get; private set; }

        /// <summary>
        /// Gets the current content length.
        /// </summary>
        public int ContentLength { get; private set; }

        /// <summary>
        /// Read/Write a verb value into the internal buffer.
        /// </summary>
        public RoomVerb Verb
        {
            get
            {
                if (disposed) throw new ObjectDisposedException(null);
                var verb = Buffer[0..3];
                if (!RoomVerb.Verify(verb)) throw new FormatException($"Invalid verb format: {verb}");
                return new(verb);
            }
            set
            {
                if (disposed) throw new ObjectDisposedException(null);
                var verb = Buffer[0..3];
                if (!value.Validate()) throw new FormatException($"Invalid verb format: {value}");
                value.Data.CopyTo(verb);
            }
        }

        /// <summary>
        /// Read/Write a channel value into the internal buffer.
        /// </summary>
        public RoomChannel Channel
        {
            get
            {
                if (disposed) throw new ObjectDisposedException(null);
                var channel = Buffer[4..12];
                if (!RoomChannel.Verify(channel)) throw new FormatException($"Invalid channel format: {channel}");
                return new(channel);
            }
            set
            {
                if (disposed) throw new ObjectDisposedException(null);
                var channel = Buffer[4..12];
                if (!value.Validate()) throw new FormatException($"Invalid channel format: {value}");
                value.Data.CopyTo(channel);
            }
        }

        /// <summary>
        /// Read/Write a content value into the internal buffer.
        /// </summary>
        public RoomContent Content
        {
            get
            {
                if (disposed) throw new ObjectDisposedException(null);
                var content = Buffer[13..];
                return new(content.Slice(0, ContentLength));
            }
            set
            {
                if (disposed) throw new ObjectDisposedException(null);
                var content = Buffer[13..];
                value.Data.CopyTo(content);
                ContentLength = value.Length;
            }
        }

        /// <summary>
        /// Validate if the current buffer data is a valid message.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        public bool Validate()
        {
            if (disposed) throw new ObjectDisposedException(null);
            var result = RoomMessage.Verify(Buffer);
            return result;
        }

        /// <summary>
        /// Internal dispose implementation.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                Buffer = default;
                ContentLength = 0;
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// Constructs a new buffer with the specified buffering size.
        /// </summary>
        /// <param name="buffering">Amount of bytes to buffer.</param>
        /// <exception cref="ArgumentException">If buffering is less than 12.</exception>
        public RoomBuffer(int buffering = 1024)
        {
            if (buffering < 12) throw new ArgumentException("Buffering is too short (min 12)");
            var buffer = new byte[buffering];
            buffer[3] = (byte)' ';
            if (buffering > 12) buffer[12] = (byte)'\n';
            Buffer = buffer;
        }

        /// <summary>
        /// Constructs a new buffer with the specified buffer.
        /// </summary>
        /// <param name="buffer">Buffer to use.</param>
        /// <exception cref="ArgumentException">If buffer size is less than 12.</exception>
        public RoomBuffer(ArraySegment<byte> buffer)
        {
            if (buffer.Count < 12) throw new ArgumentException("Buffer is too short (min 12)");
            buffer[3] = (byte)' ';
            if (buffer.Count > 12) buffer[12] = (byte)'\n';
            Buffer = buffer;
        }

    }

}