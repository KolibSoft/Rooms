using System;
using System.IO;

namespace KolibSoft.Rooms.Core.Streams
{
    public sealed class RoomStreamOptions
    {

        public int MaxVerbLength { get; set; } = DefaultMaxVerbLength;
        public int MaxChannelLength { get; set; } = DefaultMaxChannelLength;
        public int MaxCountLength { get; set; } = DefaultMaxCountLength;
        public int MaxContentLength { get; set; } = DefaultMaxContentLength;
        public int MaxFastBuffering { get; set; } = DefaultMaxFastBuffering;
        public string TempContentFolderPath { get; set; } = DefaultTempContentFolderPath;

        public Stream CreateContentStream(long count)
        {
            if (count < 1) return Stream.Null;
            if (count <= MaxFastBuffering) return new MemoryStream((int)count);
            var path = Path.Combine(TempContentFolderPath, $"{DateTime.UtcNow.Ticks}");
            return new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose);
        }

        public const int DefaultMaxVerbLength = 128;
        public const int DefaultMaxChannelLength = 32;
        public const int DefaultMaxCountLength = 32;
        public const int DefaultMaxContentLength = 4 * 1024 * 1024;
        public const int DefaultMaxFastBuffering = 1024 * 1024;
        public static readonly string DefaultTempContentFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache), "Content");

    }
}