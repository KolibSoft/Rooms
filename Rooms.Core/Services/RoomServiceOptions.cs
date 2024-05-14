namespace KolibSoft.Rooms.Core.Services
{
    public sealed class RoomServiceOptions
    {

        public int MaxFastBuffering { get; set; } = DefaultMaxFastBuffering;

        public const int DefaultMaxFastBuffering = 1024 * 1024;

    }
}