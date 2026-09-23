namespace Othello.Core;

public enum GameStatus
{
    InProgress,
    BlackWins,
    WhiteWins,
    Draw,
}

/// <summary>
/// A single entry in the game history. A turn without position is a pass.
/// </summary>
public sealed record Turn(Player Player, Position? Position, IReadOnlyList<Position> Flipped)
{
    public bool IsPass => Position is null;
}

/// <summary>
/// An Othello game. Passes are applied automatically: if the player to move has no valid move
/// but the opponent has, the player passes. If neither player can move, the game is over.
/// </summary>
/// <remarks>Instances are not thread-safe.</remarks>
public sealed class Game
{
    private readonly List<Turn> _history = [];

    public Game()
        : this(Board.Initial, Player.Black)
    {
    }

    public Game(Board board, Player playerToMove)
    {
        ArgumentNullException.ThrowIfNull(board);
        Board = board;
        CurrentPlayer = playerToMove;
        ResolvePasses(playerToMove);
    }

    public Board Board { get; private set; }

    /// <summary>
    /// Gets the player whose turn it is, or <see langword="null"/> if the game is over.
    /// </summary>
    public Player? CurrentPlayer { get; private set; }

    public bool IsOver => CurrentPlayer is null;

    public IReadOnlyList<Turn> History => _history;

    public GameStatus Status
    {
        get
        {
            if (!IsOver)
            {
                return GameStatus.InProgress;
            }

            var black = Board.Count(Player.Black);
            var white = Board.Count(Player.White);
            return black > white ? GameStatus.BlackWins
                : white > black ? GameStatus.WhiteWins
                : GameStatus.Draw;
        }
    }

    public Player? Winner => Status switch
    {
        GameStatus.BlackWins => Player.Black,
        GameStatus.WhiteWins => Player.White,
        _ => null,
    };

    public IReadOnlyList<Position> ValidMoves =>
        CurrentPlayer is { } player ? Board.GetValidMoves(player) : [];

    public bool IsValidMove(Position position) =>
        CurrentPlayer is { } player && Board.IsValidMove(position, player);

    /// <summary>
    /// Places a disc of the current player on <paramref name="position"/>.
    /// </summary>
    /// <returns>The turns that were added to the history (the move plus a potential pass of the opponent).</returns>
    /// <exception cref="InvalidMoveException">The game is over or the move is not allowed.</exception>
    public IReadOnlyList<Turn> Play(Position position)
    {
        if (CurrentPlayer is not { } player)
        {
            throw new InvalidMoveException("The game is already over.");
        }

        var historyLength = _history.Count;
        Board = Board.Apply(position, player, out var flipped);
        _history.Add(new Turn(player, position, flipped));
        ResolvePasses(player.Opponent);
        return _history[historyLength..];
    }

    private void ResolvePasses(Player playerToMove)
    {
        if (Board.HasValidMove(playerToMove))
        {
            CurrentPlayer = playerToMove;
        }
        else if (Board.HasValidMove(playerToMove.Opponent))
        {
            _history.Add(new Turn(playerToMove, null, []));
            CurrentPlayer = playerToMove.Opponent;
        }
        else
        {
            CurrentPlayer = null;
        }
    }
}
