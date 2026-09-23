using Othello.Core;

namespace Othello.Api.Games;

public enum GameMode
{
    HumanVsComputer,
    HumanVsHuman,
}

public sealed record CreateGameRequest(GameMode Mode = GameMode.HumanVsComputer, Player HumanPlayer = Player.Black);

public sealed record PositionDto(int Row, int Column)
{
    public static PositionDto From(Position position) => new(position.Row, position.Column);
}

public sealed record ScoreDto(int Black, int White);

public sealed record TurnDto(Player Player, PositionDto? Position, IReadOnlyList<PositionDto> Flipped);

public sealed record GameDto(
    Guid Id,
    GameMode Mode,
    Player? ComputerPlayer,
    IReadOnlyList<string> Board,
    Player? CurrentPlayer,
    GameStatus Status,
    ScoreDto Score,
    IReadOnlyList<PositionDto> ValidMoves,
    IReadOnlyList<TurnDto> History);
