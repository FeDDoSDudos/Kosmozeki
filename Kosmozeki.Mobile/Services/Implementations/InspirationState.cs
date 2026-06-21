using System;
using System.Collections.Generic;
using System.Text;

namespace Kosmozeki.Mobile.Services.Implementations;

public sealed class InspirationState : IInspirationState
{
    private const string InspirationKey = "player-inspiration";

    public int Value => Preferences.Default.Get(InspirationKey, 0);

    public void Set(int value)
    {
        Preferences.Default.Set(InspirationKey, Math.Max(0, value));
    }

    public void Change(int delta)
    {
        Set(Value + delta);
    }
}