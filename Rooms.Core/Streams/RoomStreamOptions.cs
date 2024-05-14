namespace KolibSoft.Rooms.Core.Streams
{
    public sealed class RoomStreamOptions
    {

        public int MaxVerbLength { get; set; } = DefaultMaxVerbLength;
        public int MaxChannelLength { get; set; } = DefaultMaxChannelLength;
        public int MaxCountLength { get; set; } = DefaultMaxCountLength;
        public int MaxContentLength { get; set; } = DefaultMaxContentLength;

        public const int DefaultMaxVerbLength = 128;
        public const int DefaultMaxChannelLength = 32;
        public const int DefaultMaxCountLength = 32;
        public const int DefaultMaxContentLength = 4 * 1024 * 1024;

    }
}