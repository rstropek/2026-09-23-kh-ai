namespace Othello.Core.Tests;

public class BoardTests
{
    [Fact]
    public void Initial_HasStandardStartingPosition()
    {
        var board = Board.Initial;

        Assert.Equal(Player.White, board[Position.Parse("d4")]);
        Assert.Equal(Player.Black, board[Position.Parse("e4")]);
        Assert.Equal(Player.Black, board[Position.Parse("d5")]);
        Assert.Equal(Player.White, board[Position.Parse("e5")]);
        Assert.Equal(2, board.Count(Player.Black));
        Assert.Equal(2, board.Count(Player.White));
        Assert.Equal(60, board.EmptyCount);
    }

    [Fact]
    public void GetValidMoves_Initial_ReturnsFourSymmetricMovesForBlack()
    {
        var moves = Board.Initial.GetValidMoves(Player.Black).Select(m => m.ToString());

        Assert.Equal(["d3", "c4", "f5", "e6"], moves);
    }

    [Fact]
    public void GetValidMoves_Initial_ReturnsFourSymmetricMovesForWhite()
    {
        var moves = Board.Initial.GetValidMoves(Player.White).Select(m => m.ToString());

        Assert.Equal(["e3", "f4", "c5", "d6"], moves);
    }

    [Fact]
    public void Apply_ValidMove_PlacesDiscAndFlipsEnclosedDisc()
    {
        var board = Board.Initial.Apply(Position.Parse("d3"), Player.Black, out var flipped);

        Assert.Equal([Position.Parse("d4")], flipped);
        Assert.Equal(Player.Black, board[Position.Parse("d3")]);
        Assert.Equal(Player.Black, board[Position.Parse("d4")]);
        Assert.Equal(4, board.Count(Player.Black));
        Assert.Equal(1, board.Count(Player.White));
    }

    [Fact]
    public void Apply_DoesNotModifyOriginalBoard()
    {
        var original = Board.Initial;

        _ = original.Apply(Position.Parse("d3"), Player.Black);

        Assert.Equal(Board.Initial.ToString(), original.ToString());
        Assert.Null(original[Position.Parse("d3")]);
    }

    [Fact]
    public void Apply_FlipsInAllEightDirections()
    {
        var board = Board.Parse(
            """
            B..B..B.
            .W.W.W..
            ..WWW...
            BWW.WWWB
            ..WWW...
            .W.W.W..
            B..B..B.
            ........
            """);

        var result = board.Apply(Position.Parse("d4"), Player.Black, out var flipped);

        Assert.Equal(17, flipped.Count);
        Assert.Equal(0, result.Count(Player.White));
    }

    [Fact]
    public void GetFlips_DoesNotFlipLineThatIsNotClosed()
    {
        var board = Board.Parse(
            """
            ........
            ........
            ........
            .WWB....
            ........
            ........
            ........
            ........
            """);

        Assert.Equal([Position.Parse("b4"), Position.Parse("c4")], board.GetFlips(Position.Parse("a4"), Player.Black));
        Assert.Empty(board.GetFlips(Position.Parse("a4"), Player.White));
    }

    [Fact]
    public void GetFlips_DoesNotFlipAcrossEmptySquare()
    {
        var board = Board.Parse(
            """
            ........
            ........
            ........
            .W.WB...
            ........
            ........
            ........
            ........
            """);

        Assert.Empty(board.GetFlips(Position.Parse("a4"), Player.Black));
    }

    [Fact]
    public void GetFlips_DoesNotWrapAroundBoardEdge()
    {
        var board = Board.Parse(
            """
            .......W
            B.......
            ........
            ........
            ........
            ........
            ........
            ........
            """);

        Assert.Empty(board.GetFlips(Position.Parse("g1"), Player.Black));
    }

    [Theory]
    [InlineData("d4")]
    [InlineData("a1")]
    [InlineData("e3")]
    public void Apply_InvalidMove_Throws(string position)
    {
        Assert.Throws<InvalidMoveException>(() => Board.Initial.Apply(Position.Parse(position), Player.Black));
    }

    [Fact]
    public void IsValidMove_OccupiedSquare_ReturnsFalse()
    {
        Assert.False(Board.Initial.IsValidMove(Position.Parse("d4"), Player.Black));
    }

    [Fact]
    public void HasValidMove_FullBoard_ReturnsFalse()
    {
        var board = Board.FromRows([.. Enumerable.Repeat("BBBBWWWW", 8)]);

        Assert.False(board.HasValidMove(Player.Black));
        Assert.False(board.HasValidMove(Player.White));
        Assert.Empty(board.GetValidMoves(Player.Black));
    }

    [Fact]
    public void ParseAndToString_RoundTrip()
    {
        const string layout = "B.......\n.W......\n........\n...WB...\n...BW...\n........\n........\n.......W";

        Assert.Equal(layout, Board.Parse(layout).ToString());
    }

    [Theory]
    [InlineData("........")]
    [InlineData("........\n........\n........\n........\n........\n........\n........\n.......")]
    [InlineData("........\n........\n........\n........\n........\n........\n........\n.......X")]
    public void Parse_InvalidLayout_Throws(string layout)
    {
        Assert.Throws<FormatException>(() => Board.Parse(layout));
    }
}
