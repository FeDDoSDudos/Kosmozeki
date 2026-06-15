using System;
using System.Collections.Generic;
using System.Text;

namespace Kosmozeki.Mobile.Options;

public sealed class ServerOptions
{
    public const string SectionName = "Server";
    public string BaseUrl { get; set; } = string.Empty;
}
