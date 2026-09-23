using Othello.Core;

namespace Othello.Api.Games;

/// <summary>
/// A running game together with its configuration. All access to <see cref="Game"/> must hold <see cref="Lock"/>.
/// </summary>
public sealed class GameSession(Guid id, GameMode mode, Player? computerPlayer)
{
    public Guid Id { get; } = id;

    public GameMode Mode { get; } = mode;

    /// <summary>
    /// Gets the color played by the computer, or <see langword="null"/> for human vs. human games.
    /// </summary>
    public Player? ComputerPlayer { get; } = computerPlayer;

    public Game Game { get; } = new();

    public Lock Lock { get; } = new();

    public bool IsComputerTurn => ComputerPlayer is not null && Game.CurrentPlayer == ComputerPlayer;

    public GameDto ToDto() => new(
        Id,
        Mode,
        ComputerPlayer,
        Game.Board.ToRows(),
        Game.CurrentPlayer,
        Game.Status,
        new ScoreDto(Game.Board.Count(Player.Black), Game.Board.Count(Player.White)),
        [.. Game.ValidMoves.Select(PositionDto.From)],
        [.. Game.History.Select(t => new TurnDto(
            t.Player,
            t.Position is { } p ? PositionDto.From(p) : null,
            [.. t.Flipped.Select(PositionDto.From)]))]);
}
