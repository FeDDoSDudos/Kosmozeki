using System;
using System.Collections.Generic;
using System.Text;

namespace Kosmozeki.Mobile.Services;

public sealed class RoomContext : IRoomContext
{
    public Guid CurrentRoomId { get; set; } =
        Guid.Parse("11111111-1111-1111-1111-111111111111");
}
