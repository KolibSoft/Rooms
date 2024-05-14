using System;
using System.IO;

namespace KolibSoft.Rooms.Core.Services
{
    public sealed class RoomServiceOptions
    {

        public int MaxFastBuffering { get; set; } = DefaultMaxFastBuffering;
        public int MaxStreamRate { get; set; } = DefaultMaxStreamRate;
        public string TempContentFolderPath { get; set; } = DefaultTempContentFolderPath;

        public const int DefaultMaxFastBuffering = 1024 * 1024;
        public const int DefaultMaxStreamRate = 1024 * 1024;
        public static readonly string DefaultTempContentFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache), "Content");

    }
}