namespace Othello.Core;

/// <summary>
/// One of the two Othello players. Black always moves first.
/// </summary>
public enum Player
{
    Black,
    White,
}

public static class PlayerExtensions
{
    extension(Player player)
    {
        /// <summary>
        /// Gets the other player.
        /// </summary>
        public Player Opponent => player == Player.Black ? Player.White : Player.Black;
    }
}
