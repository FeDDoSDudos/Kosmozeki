using System;
using System.Collections.Generic;
using System.Text;

namespace Kosmozeki.Mobile.Services;

public interface IRoomContext
{
    Guid CurrentRoomId { get; }
}
