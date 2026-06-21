using System;
using System.Collections.Generic;
using System.Text;

namespace Kosmozeki.Mobile.Services;

public interface IInspirationState
{
    int Value { get; }
    void Set(int value);
    void Change(int delta);
}
