using System.Text;

namespace Othello.Core;

/// <summary>
/// Immutable 8x8 Othello board.
/// </summary>
public sealed class Board
{
    public const int Size = 8;

    private static readonly (int Row, int Column)[] Directions =
    [
        (-1, -1), (-1, 0), (-1, 1),
        (0, -1), (0, 1),
        (1, -1), (1, 0), (1, 1),
    ];

    private readonly Player?[] _cells;

    private Board(Player?[] cells)
    {
        _cells = cells;
    }

    /// <summary>
    /// Gets the standard starting position: white on d4 and e5, black on e4 and d5.
    /// </summary>
    public static Board Initial { get; } = Parse(
        """
        ........
        ........
        ........
        ...WB...
        ...BW...
        ........
        ........
        ........
        """);

    public Player? this[Position position] => _cells[position.Index];

    public Player? this[int row, int column] => this[new Position(row, column)];

    /// <summary>
    /// Creates a board from eight lines of eight characters each:
    /// <c>.</c> (empty), <c>B</c> (black) or <c>W</c> (white).
    /// </summary>
    public static Board Parse(string layout)
    {
        ArgumentNullException.ThrowIfNull(layout);
        var rows = layout.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        return FromRows(rows);
    }

    /// <summary>
    /// Creates a board from eight rows of eight characters each (see <see cref="Parse"/>).
    /// </summary>
    public static Board FromRows(IReadOnlyList<string> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);
        if (rows.Count != Size || rows.Any(r => r.Length != Size))
        {
            throw new FormatException($"A board layout must consist of {Size} rows with {Size} cells each.");
        }

        var cells = new Player?[Size * Size];
        for (var row = 0; row < Size; row++)
        {
            for (var column = 0; column < Size; column++)
            {
                cells[(row * Size) + column] = char.ToUpperInvariant(rows[row][column]) switch
                {
                    '.' => null,
                    'B' => Player.Black,
                    'W' => Player.White,
                    var c => throw new FormatException($"Invalid cell character '{c}'."),
                };
            }
        }

        return new Board(cells);
    }

    public int Count(Player player) => _cells.Count(c => c == player);

    public int EmptyCount => _cells.Count(c => c is null);

    /// <summary>
    /// Gets the opponent discs that would be flipped if <paramref name="player"/> placed a disc
    /// on <paramref name="position"/>. An empty result means that the move is not valid.
    /// </summary>
    public IReadOnlyList<Position> GetFlips(Position position, Player player)
    {
        if (this[position] is not null)
        {
            return [];
        }

        var flips = new List<Position>();
        var opponent = player.Opponent;
        foreach (var (dRow, dColumn) in Directions)
        {
            var row = position.Row + dRow;
            var column = position.Column + dColumn;
            var candidates = new List<Position>();
            while (Position.IsOnBoard(row, column) && this[row, column] == opponent)
            {
                candidates.Add(new Position(row, column));
                row += dRow;
                column += dColumn;
            }

            if (candidates.Count > 0 && Position.IsOnBoard(row, column) && this[row, column] == player)
            {
                flips.AddRange(candidates);
            }
        }

        return flips;
    }

    public bool IsValidMove(Position position, Player player) => GetFlips(position, player).Count > 0;

    /// <summary>
    /// Gets all squares on which <paramref name="player"/> may place a disc, in row-major order.
    /// </summary>
    public IReadOnlyList<Position> GetValidMoves(Player player) =>
        [.. Position.All.Where(p => IsValidMove(p, player))];

    public bool HasValidMove(Player player) => Position.All.Any(p => IsValidMove(p, player));

    /// <summary>
    /// Places a disc of <paramref name="player"/> on <paramref name="position"/> and flips the enclosed discs.
    /// </summary>
    /// <exception cref="InvalidMoveException">The move is not allowed.</exception>
    public Board Apply(Position position, Player player) => Apply(position, player, out _);

    /// <inheritdoc cref="Apply(Position, Player)"/>
    public Board Apply(Position position, Player player, out IReadOnlyList<Position> flipped)
    {
        flipped = GetFlips(position, player);
        if (flipped.Count == 0)
        {
            throw new InvalidMoveException($"{player} cannot place a disc on {position}.");
        }

        var newCells = (Player?[])_cells.Clone();
        newCells[position.Index] = player;
        foreach (var flip in flipped)
        {
            newCells[flip.Index] = player;
        }

        return new Board(newCells);
    }

    /// <summary>
    /// Gets the board as eight strings (see <see cref="Parse"/> for the format).
    /// </summary>
    public IReadOnlyList<string> ToRows()
    {
        var rows = new string[Size];
        var builder = new StringBuilder(Size);
        for (var row = 0; row < Size; row++)
        {
            builder.Clear();
            for (var column = 0; column < Size; column++)
            {
                builder.Append(this[row, column] switch
                {
                    Player.Black => 'B',
                    Player.White => 'W',
                    _ => '.',
                });
            }

            rows[row] = builder.ToString();
        }

        return rows;
    }

    public override string ToString() => string.Join('\n', ToRows());
}
