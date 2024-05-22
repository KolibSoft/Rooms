using System;
using System.IO;

namespace KolibSoft.Rooms.Core.Services
{
    public sealed class RoomServiceOptions
    {

        public int MaxStreamRate { get; set; } = DefaultMaxStreamRate;

        public const int DefaultMaxStreamRate = 1024 * 1024;

    }
}