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
        /// Buffer to store a message at a time.
        /// </summary>
        private ArraySegment<byte> buffer;

        /// <summary>
        /// Read/Write a verb value into the internal buffer.
        /// </summary>
        public RoomVerb Verb
        {
            get
            {
                if (disposed) throw new ObjectDisposedException(null);
                var verb = buffer[..3];
                if (!RoomVerb.Verify(verb)) throw new FormatException($"Invalid verb format: {verb}");
                return new(verb);
            }
            set
            {
                if (disposed) throw new ObjectDisposedException(null);
                var verb = buffer[..3];
                if (!value.Validate()) throw new FormatException($"Invalid verb format: {value}");
                value.CopyTo(verb);
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
                var channel = buffer[4..12];
                if (!RoomChannel.Verify(channel)) throw new FormatException($"Invalid channel format: {channel}");
                return new(channel);
            }
            set
            {
                if (disposed) throw new ObjectDisposedException(null);
                var channel = buffer[4..12];
                if (!value.Validate()) throw new FormatException($"Invalid channel format: {value}");
                value.CopyTo(channel);
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
                var content = buffer[13..];
                return new(content);
            }
            set
            {
                if (disposed) throw new ObjectDisposedException(null);
                var content = buffer[13..];
                value.CopyTo(content);
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
            var result = RoomVerb.Verify(buffer[..3]) && RoomChannel.Verify(buffer[4..12]);
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
                buffer = default;
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
        /// <exception cref="ArgumentException">If buffering is less than 16.</exception>
        public RoomBuffer(int buffering = 1024)
        {
            if (buffering < 16) throw new ArgumentException("Buffering is too short (min 16)");
            var buffer = new byte[buffering];
            buffer[3] = (byte)' ';
            buffer[12] = (byte)'\n';
            this.buffer = buffer;
        }

        /// <summary>
        /// Constructs a new buffer with the specified buffer.
        /// </summary>
        /// <param name="buffer">Buffer to use.</param>
        /// <exception cref="ArgumentException">If buffer size is less than 16.</exception>
        public RoomBuffer(ArraySegment<byte> buffer)
        {
            if (buffer.Count < 16) throw new ArgumentException("Buffer is too short (min 16)");
            buffer[3] = (byte)' ';
            buffer[12] = (byte)'\n';
            this.buffer = buffer;
        }

    }

}