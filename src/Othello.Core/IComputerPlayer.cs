namespace Othello.Core;

/// <summary>
/// Chooses moves for a computer-controlled player.
/// </summary>
public interface IComputerPlayer
{
    /// <summary>
    /// Chooses a move for <paramref name="player"/> on <paramref name="board"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException"><paramref name="player"/> has no valid move.</exception>
    Position ChooseMove(Board board, Player player);
}
