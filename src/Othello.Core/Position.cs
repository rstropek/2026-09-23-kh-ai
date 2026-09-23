namespace Othello.Core;

/// <summary>
/// A square on the Othello board. Row 0 is the top row ("1"), column 0 is the leftmost column ("a").
/// </summary>
public readonly record struct Position
{
    public Position(int row, int column)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(row);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(row, Board.Size);
        ArgumentOutOfRangeException.ThrowIfNegative(column);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(column, Board.Size);

        Row = row;
        Column = column;
    }

    public int Row { get; }

    public int Column { get; }

    internal int Index => (Row * Board.Size) + Column;

    /// <summary>
    /// Gets all 64 squares in row-major order.
    /// </summary>
    public static IEnumerable<Position> All
    {
        get
        {
            for (var row = 0; row < Board.Size; row++)
            {
                for (var column = 0; column < Board.Size; column++)
                {
                    yield return new Position(row, column);
                }
            }
        }
    }

    public static bool IsOnBoard(int row, int column) =>
        row is >= 0 and < Board.Size && column is >= 0 and < Board.Size;

    /// <summary>
    /// Parses algebraic notation like <c>d3</c> (column letter a-h, row number 1-8).
    /// </summary>
    public static Position Parse(string notation)
    {
        ArgumentNullException.ThrowIfNull(notation);
        if (notation.Length != 2)
        {
            throw new FormatException($"'{notation}' is not a valid board position.");
        }

        var column = char.ToLowerInvariant(notation[0]) - 'a';
        var row = notation[1] - '1';
        if (!IsOnBoard(row, column))
        {
            throw new FormatException($"'{notation}' is not a valid board position.");
        }

        return new Position(row, column);
    }

    public override string ToString() => $"{(char)('a' + Column)}{Row + 1}";
}
