namespace Othello.Core;

/// <summary>
/// A simple one-ply computer player. It evaluates every valid move by
/// <list type="bullet">
/// <item>the positional value of the resulting board (corners are valuable, squares next to empty corners are dangerous),</item>
/// <item>the number of moves left for the opponent (fewer is better), and</item>
/// <item>the final result if the move ends the game.</item>
/// </list>
/// Ties are broken randomly.
/// </summary>
public sealed class SimpleComputerPlayer(Random? random = null) : IComputerPlayer
{
    internal const int WinScore = 100_000;
    internal const int MobilityWeight = 5;

    private static readonly int[] PositionWeights =
    [
        100, -20, 10, 5, 5, 10, -20, 100,
        -20, -50, -2, -2, -2, -2, -50, -20,
        10, -2, 1, 1, 1, 1, -2, 10,
        5, -2, 1, 0, 0, 1, -2, 5,
        5, -2, 1, 0, 0, 1, -2, 5,
        10, -2, 1, 1, 1, 1, -2, 10,
        -20, -50, -2, -2, -2, -2, -50, -20,
        100, -20, 10, 5, 5, 10, -20, 100,
    ];

    private readonly Random _random = random ?? Random.Shared;

    public Position ChooseMove(Board board, Player player)
    {
        ArgumentNullException.ThrowIfNull(board);

        var bestScore = int.MinValue;
        var bestMoves = new List<Position>();
        foreach (var move in board.GetValidMoves(player))
        {
            var score = Evaluate(board.Apply(move, player), player);
            if (score > bestScore)
            {
                bestScore = score;
                bestMoves.Clear();
            }

            if (score == bestScore)
            {
                bestMoves.Add(move);
            }
        }

        if (bestMoves.Count == 0)
        {
            throw new InvalidOperationException($"{player} has no valid move.");
        }

        return bestMoves[_random.Next(bestMoves.Count)];
    }

    /// <summary>
    /// Evaluates <paramref name="board"/> from the perspective of <paramref name="player"/>
    /// after <paramref name="player"/> has moved. Higher is better.
    /// </summary>
    internal static int Evaluate(Board board, Player player)
    {
        var opponent = player.Opponent;
        var opponentMobility = board.GetValidMoves(opponent).Count;
        if (opponentMobility == 0 && !board.HasValidMove(player))
        {
            // The game is over, only the result counts.
            var difference = board.Count(player) - board.Count(opponent);
            return difference == 0 ? 0 : Math.Sign(difference) * (WinScore + Math.Abs(difference));
        }

        return PositionalScore(board, player) - (MobilityWeight * opponentMobility);
    }

    internal static int PositionalScore(Board board, Player player)
    {
        var score = 0;
        foreach (var position in Position.All)
        {
            var owner = board[position];
            if (owner is null)
            {
                continue;
            }

            var weight = PositionWeights[position.Index];

            // Squares next to a corner are only dangerous while the corner is still empty.
            if (weight < 0 && board[NearestCorner(position)] is not null)
            {
                weight = 0;
            }

            score += owner == player ? weight : -weight;
        }

        return score;
    }

    private static Position NearestCorner(Position position) => new(
        position.Row < Board.Size / 2 ? 0 : Board.Size - 1,
        position.Column < Board.Size / 2 ? 0 : Board.Size - 1);
}
