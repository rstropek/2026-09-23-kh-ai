namespace Othello.Core.Tests;

public class PositionTests
{
    [Theory]
    [InlineData("a1", 0, 0)]
    [InlineData("h1", 0, 7)]
    [InlineData("a8", 7, 0)]
    [InlineData("d3", 2, 3)]
    [InlineData("H8", 7, 7)]
    public void Parse_ValidNotation_ReturnsPosition(string notation, int row, int column)
    {
        var position = Position.Parse(notation);

        Assert.Equal(new Position(row, column), position);
    }

    [Theory]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("a0")]
    [InlineData("a9")]
    [InlineData("i1")]
    [InlineData("a10")]
    public void Parse_InvalidNotation_Throws(string notation)
    {
        Assert.Throws<FormatException>(() => Position.Parse(notation));
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(8, 0)]
    [InlineData(0, 8)]
    public void Constructor_OutOfBoard_Throws(int row, int column)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Position(row, column));
    }

    [Fact]
    public void ToString_ReturnsAlgebraicNotation()
    {
        Assert.Equal("d3", new Position(2, 3).ToString());
    }

    [Fact]
    public void All_ContainsEverySquareOnceInRowMajorOrder()
    {
        var all = Position.All.ToList();

        Assert.Equal(64, all.Count);
        Assert.Equal(64, all.Distinct().Count());
        Assert.Equal(new Position(0, 0), all[0]);
        Assert.Equal(new Position(0, 1), all[1]);
        Assert.Equal(new Position(7, 7), all[^1]);
    }
}
