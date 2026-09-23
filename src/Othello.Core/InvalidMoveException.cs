namespace Othello.Core;

/// <summary>
/// Thrown when a move violates the rules of Othello.
/// </summary>
public class InvalidMoveException : Exception
{
    public InvalidMoveException()
    {
    }

    public InvalidMoveException(string message)
        : base(message)
    {
    }

    public InvalidMoveException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
