namespace Othello.Core.Tests;

public class SimpleComputerPlayerTests
{
    [Fact]
    public void ChooseMove_InitialBoard_ReturnsValidMove()
    {
        var computer = new SimpleComputerPlayer(new Random(42));

        var move = computer.ChooseMove(Board.Initial, Player.Black);

        Assert.Contains(move, Board.Initial.GetValidMoves(Player.Black));
    }

    [Fact]
    public void ChooseMove_NoValidMove_Throws()
    {
        var board = Board.FromRows([.. Enumerable.Repeat("BBBBBBBB", 8)]);
        var computer = new SimpleComputerPlayer(new Random(42));

        Assert.Throws<InvalidOperationException>(() => computer.ChooseMove(board, Player.White));
    }

    [Fact]
    public void ChooseMove_CornerAvailable_TakesCorner()
    {
        // Black can take the a1 corner (flipping b2) or play one of the regular opening moves.
        var board = Board.Parse(
            """
            ........
            .W......
            ..B.....
            ...WB...
            ...BW...
            ........
            ........
            ........
            """);
        Assert.True(board.IsValidMove(Position.Parse("a1"), Player.Black));
        var computer = new SimpleComputerPlayer(new Random(42));

        var move = computer.ChooseMove(board, Player.Black);

        Assert.Equal(Position.Parse("a1"), move);
    }

    [Fact]
    public void ChooseMove_AvoidsSquareDiagonalToEmptyCorner()
    {
        // Black could play b2 (X-square next to the empty a1 corner), which would give white the corner.
        var board = Board.Parse(
            """
            ........
            ........
            ..W.....
            ...WB...
            ...BB...
            .....W..
            ........
            ........
            """);
        Assert.True(board.IsValidMove(Position.Parse("b2"), Player.Black));
        var computer = new SimpleComputerPlayer(new Random(42));

        var move = computer.ChooseMove(board, Player.Black);

        Assert.NotEqual(Position.Parse("b2"), move);
    }

    [Fact]
    public void ChooseMove_WipeoutAvailable_PlaysIt()
    {
        // c4 captures both white discs and wins immediately, c2 only captures d3.
        var board = Board.Parse(
            """
            ........
            ....B...
            ...W....
            ...WB...
            ........
            ........
            ........
            ........
            """);
        Assert.True(board.IsValidMove(Position.Parse("c2"), Player.Black));
        var computer = new SimpleComputerPlayer(new Random(42));

        var move = computer.ChooseMove(board, Player.Black);

        Assert.Equal(Position.Parse("c4"), move);
        Assert.Equal(0, board.Apply(move, Player.Black).Count(Player.White));
    }

    [Fact]
    public void ChooseMove_PrefersMoveThatLeavesOpponentFewerOptions()
    {
        var board = Board.Initial.Apply(Position.Parse("d3"), Player.Black);
        var computer = new SimpleComputerPlayer(new Random(42));

        var move = computer.ChooseMove(board, Player.White);

        var chosenScore = SimpleComputerPlayer.Evaluate(board.Apply(move, Player.White), Player.White);
        Assert.All(
            board.GetValidMoves(Player.White),
            m => Assert.True(SimpleComputerPlayer.Evaluate(board.Apply(m, Player.White), Player.White) <= chosenScore));
    }

    [Fact]
    public void ChooseMove_SameSeed_IsDeterministic()
    {
        var first = new SimpleComputerPlayer(new Random(7));
        var second = new SimpleComputerPlayer(new Random(7));
        var board = Board.Initial;

        for (var i = 0; i < 10; i++)
        {
            Assert.Equal(first.ChooseMove(board, Player.Black), second.ChooseMove(board, Player.Black));
        }
    }

    [Fact]
    public void Evaluate_LostGame_IsWorseThanAnyRunningGame()
    {
        var lost = Board.FromRows([.. Enumerable.Repeat("WWWWWWWB", 8)]);

        Assert.True(SimpleComputerPlayer.Evaluate(lost, Player.Black) < -SimpleComputerPlayer.WinScore);
        Assert.True(SimpleComputerPlayer.Evaluate(lost, Player.White) > SimpleComputerPlayer.WinScore);
    }

    [Fact]
    public void PositionalScore_XSquareNextToOccupiedCorner_IsNotPenalized()
    {
        var withEmptyCorner = Board.Parse("........\n.B......\n........\n........\n........\n........\n........\n........");
        var withOwnedCorner = Board.Parse("W.......\n.B......\n........\n........\n........\n........\n........\n........");

        Assert.True(SimpleComputerPlayer.PositionalScore(withEmptyCorner, Player.Black) < 0);
        Assert.Equal(-100, SimpleComputerPlayer.PositionalScore(withOwnedCorner, Player.Black));
    }

    [Theory]
    [InlineData(Player.Black)]
    [InlineData(Player.White)]
    public void FullGames_AgainstRandomPlayer_OnlyValidMovesAndMostlyWins(Player computerColor)
    {
        const int games = 40;
        var wins = 0;
        for (var seed = 0; seed < games; seed++)
        {
            var computer = new SimpleComputerPlayer(new Random(seed));
            var random = new Random(seed + 1000);
            var game = new Game();

            while (game.CurrentPlayer is { } player)
            {
                Position move;
                if (player == computerColor)
                {
                    move = computer.ChooseMove(game.Board, player);
                    Assert.True(game.IsValidMove(move));
                }
                else
                {
                    var moves = game.ValidMoves;
                    move = moves[random.Next(moves.Count)];
                }

                game.Play(move);
            }

            if (game.Winner == computerColor)
            {
                wins++;
            }
        }

        Assert.True(wins >= games * 3 / 4, $"Computer won only {wins} of {games} games.");
    }
}
