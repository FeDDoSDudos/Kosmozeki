namespace Kosmozeki.Mobile.Services;

public sealed class PlayerIdentity : IPlayerIdentity
{
    private const string PlayerIdKey = "player-id";
    public Guid PlayerId { get; }

    public PlayerIdentity()
    {
        var raw = Preferences.Default.Get(PlayerIdKey, string.Empty);

        if (!Guid.TryParse(raw, out var playerId))
        {
            playerId = Guid.NewGuid();
            Preferences.Default.Set(PlayerIdKey, playerId.ToString("D"));
        }

        PlayerId = playerId;
    }
}