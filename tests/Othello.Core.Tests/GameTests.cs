namespace Othello.Core.Tests;

public class GameTests
{
    [Fact]
    public void NewGame_BlackMovesFirst()
    {
        var game = new Game();

        Assert.Equal(Player.Black, game.CurrentPlayer);
        Assert.Equal(GameStatus.InProgress, game.Status);
        Assert.False(game.IsOver);
        Assert.Null(game.Winner);
        Assert.Empty(game.History);
        Assert.Equal(4, game.ValidMoves.Count);
    }

    [Fact]
    public void Play_ValidMove_SwitchesPlayerAndRecordsTurn()
    {
        var game = new Game();

        var turns = game.Play(Position.Parse("d3"));

        Assert.Equal(Player.White, game.CurrentPlayer);
        var turn = Assert.Single(turns);
        Assert.Equal(Player.Black, turn.Player);
        Assert.Equal(Position.Parse("d3"), turn.Position);
        Assert.Equal([Position.Parse("d4")], turn.Flipped);
        Assert.False(turn.IsPass);
        Assert.Equal(turns, game.History);
    }

    [Fact]
    public void Play_InvalidMove_ThrowsAndKeepsState()
    {
        var game = new Game();

        Assert.Throws<InvalidMoveException>(() => game.Play(Position.Parse("a1")));
        Assert.Equal(Player.Black, game.CurrentPlayer);
        Assert.Equal(Board.Initial.ToString(), game.Board.ToString());
        Assert.Empty(game.History);
    }

    [Fact]
    public void IsValidMove_UsesCurrentPlayer()
    {
        var game = new Game();

        Assert.True(game.IsValidMove(Position.Parse("d3")));
        Assert.False(game.IsValidMove(Position.Parse("e3")));
    }

    [Fact]
    public void Play_OpponentHasNoMove_OpponentPassesAutomatically()
    {
        // After black plays a1, white has no valid move but black still has one (g6 flips g7).
        var board = Board.Parse(
            """
            .WB.....
            ........
            ........
            ........
            ........
            ........
            ......W.
            ......B.
            """);
        var game = new Game(board, Player.Black);
        Assert.Equal(2, game.ValidMoves.Count);

        var turns = game.Play(Position.Parse("a1"));

        Assert.Equal(2, turns.Count);
        Assert.True(turns[1].IsPass);
        Assert.Equal(Player.White, turns[1].Player);
        Assert.Equal(Player.Black, game.CurrentPlayer);
        Assert.Equal(GameStatus.InProgress, game.Status);
    }

    [Fact]
    public void Play_NoPlayerCanMove_GameIsOver()
    {
        var board = Board.Parse(
            """
            .WB.....
            ........
            ........
            ........
            ........
            ........
            ........
            ........
            """);
        var game = new Game(board, Player.Black);

        game.Play(Position.Parse("a1"));

        Assert.True(game.IsOver);
        Assert.Null(game.CurrentPlayer);
        Assert.Equal(GameStatus.BlackWins, game.Status);
        Assert.Equal(Player.Black, game.Winner);
        Assert.Empty(game.ValidMoves);
        Assert.False(game.IsValidMove(Position.Parse("d4")));
    }

    [Fact]
    public void Play_AfterGameOver_Throws()
    {
        var game = new Game(Board.FromRows([.. Enumerable.Repeat("BBBBBBBB", 8)]), Player.Black);

        Assert.Throws<InvalidMoveException>(() => game.Play(Position.Parse("a1")));
    }

    [Fact]
    public void Constructor_PlayerToMoveHasNoMove_PassesImmediately()
    {
        var board = Board.Parse(
            """
            .WBBBBBB
            ........
            ........
            ........
            ........
            ........
            ........
            ........
            """);

        var game = new Game(board, Player.White);

        Assert.Equal(Player.Black, game.CurrentPlayer);
        var pass = Assert.Single(game.History);
        Assert.True(pass.IsPass);
        Assert.Equal(Player.White, pass.Player);
    }

    [Theory]
    [InlineData("BBBBBBBB", "WWWWWWWW", GameStatus.Draw)]
    [InlineData("BBBBBBBB", "BBBBWWWW", GameStatus.BlackWins)]
    [InlineData("WWWWWWWW", "BBBBWWWW", GameStatus.WhiteWins)]
    public void Status_FullBoard_ReflectsDiscCount(string firstHalfRow, string secondHalfRow, GameStatus expected)
    {
        var rows = Enumerable.Repeat(firstHalfRow, 4).Concat(Enumerable.Repeat(secondHalfRow, 4)).ToArray();

        var game = new Game(Board.FromRows(rows), Player.Black);

        Assert.True(game.IsOver);
        Assert.Equal(expected, game.Status);
    }

    [Fact]
    public void Play_ShortestKnownGame_EndsWithBlackWipeout()
    {
        // A nine-move game in which black captures all white discs.
        var game = new Game();

        foreach (var move in new[] { "e6", "f4", "e3", "f6", "g5", "d6", "e7", "f5", "c5" })
        {
            game.Play(Position.Parse(move));
        }

        Assert.True(game.IsOver);
        Assert.Equal(GameStatus.BlackWins, game.Status);
        Assert.Equal(0, game.Board.Count(Player.White));
        Assert.Equal(13, game.Board.Count(Player.Black));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void RandomGame_PreservesInvariants(int seed)
    {
        var random = new Random(seed);
        var game = new Game();

        while (!game.IsOver)
        {
            var player = game.CurrentPlayer!.Value;
            var before = game.Board;
            var moves = game.ValidMoves;
            Assert.NotEmpty(moves);

            var move = moves[random.Next(moves.Count)];
            var turn = game.Play(move)[0];

            // Every move adds exactly one disc and flips the reported discs to the mover.
            Assert.Equal(before.EmptyCount - 1, game.Board.EmptyCount);
            Assert.Equal(before.Count(player) + 1 + turn.Flipped.Count, game.Board.Count(player));
            Assert.All(turn.Flipped, f => Assert.Equal(player, game.Board[f]));
        }

        Assert.False(game.Board.HasValidMove(Player.Black));
        Assert.False(game.Board.HasValidMove(Player.White));
        Assert.True(game.History.Count(t => !t.IsPass) <= 60);
    }
}
