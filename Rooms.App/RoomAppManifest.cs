﻿using System;

namespace KolibSoft.Rooms.App
{

    public class RoomAppManifest
    {
        public Guid Id { get; set; } = Guid.Empty;
        public string Name { get; set; } = string.Empty;
        public string[] Capabilities { get; set; } = Array.Empty<string>();
    }
}
