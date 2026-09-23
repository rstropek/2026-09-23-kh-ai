using Microsoft.Extensions.Caching.Memory;

using Othello.Core;

namespace Othello.Api.Games;

/// <summary>
/// In-memory storage for running games. Games that are not accessed for a while are evicted.
/// </summary>
public sealed class GameStore(IMemoryCache cache)
{
    public static readonly TimeSpan SlidingExpiration = TimeSpan.FromHours(2);

    public GameSession Create(GameMode mode, Player humanPlayer)
    {
        var computerPlayer = mode == GameMode.HumanVsComputer ? humanPlayer.Opponent : (Player?)null;
        var session = new GameSession(Guid.NewGuid(), mode, computerPlayer);
        cache.Set(session.Id, session, new MemoryCacheEntryOptions { SlidingExpiration = SlidingExpiration });
        return session;
    }

    public GameSession? Find(Guid id) => cache.TryGetValue(id, out GameSession? session) ? session : null;
}
