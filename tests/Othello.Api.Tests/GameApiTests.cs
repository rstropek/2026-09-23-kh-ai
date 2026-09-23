using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;

using Othello.Api.Games;
using Othello.Core;

namespace Othello.Api.Tests;

public class GameApiTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    private readonly HttpClient _client = factory.CreateClient();

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    [Fact]
    public async Task CreateGame_Defaults_HumanPlaysBlackAgainstComputer()
    {
        var response = await _client.PostAsJsonAsync("/api/games", new { }, CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var game = await ReadGameAsync(response);
        Assert.Equal($"/api/games/{game.Id}", response.Headers.Location?.OriginalString);
        Assert.Equal(GameMode.HumanVsComputer, game.Mode);
        Assert.Equal(Player.White, game.ComputerPlayer);
        Assert.Equal(Player.Black, game.CurrentPlayer);
        Assert.Equal(GameStatus.InProgress, game.Status);
        Assert.Equal(Board.Initial.ToRows(), game.Board);
        Assert.Equal(new ScoreDto(2, 2), game.Score);
        Assert.Equal(4, game.ValidMoves.Count);
        Assert.Empty(game.History);
    }

    [Fact]
    public async Task CreateGame_SerializesEnumsAsCamelCaseStrings()
    {
        var response = await _client.PostAsJsonAsync("/api/games", new { mode = "humanVsHuman" }, CancellationToken);

        var json = await response.Content.ReadAsStringAsync(CancellationToken);
        Assert.Contains("\"mode\":\"humanVsHuman\"", json, StringComparison.Ordinal);
        Assert.Contains("\"currentPlayer\":\"black\"", json, StringComparison.Ordinal);
        Assert.Contains("\"status\":\"inProgress\"", json, StringComparison.Ordinal);
        Assert.Contains("\"computerPlayer\":null", json, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetGame_ReturnsStoredGame()
    {
        var created = await CreateGameAsync(new CreateGameRequest());

        var game = await _client.GetFromJsonAsync<GameDto>($"/api/games/{created.Id}", JsonOptions, CancellationToken);

        Assert.NotNull(game);
        Assert.Equal(created.Id, game.Id);
    }

    [Fact]
    public async Task GetGame_Unknown_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/games/{Guid.NewGuid()}", CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PlayMove_ValidMove_UpdatesBoardAndSwitchesPlayer()
    {
        var created = await CreateGameAsync(new CreateGameRequest());

        var response = await PlayAsync(created.Id, new PositionDto(2, 3));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var game = await ReadGameAsync(response);
        Assert.Equal(Player.White, game.CurrentPlayer);
        Assert.Equal("...B....", game.Board[2]);
        Assert.Equal("...BB...", game.Board[3]);
        Assert.Equal(new ScoreDto(4, 1), game.Score);
        var turn = Assert.Single(game.History);
        Assert.Equal(new PositionDto(2, 3), turn.Position);
        Assert.Equal([new PositionDto(3, 3)], turn.Flipped);
    }

    [Fact]
    public async Task PlayMove_InvalidMove_ReturnsValidationProblem()
    {
        var created = await CreateGameAsync(new CreateGameRequest());

        var response = await PlayAsync(created.Id, new PositionDto(0, 0));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(CancellationToken);
        Assert.NotNull(problem);
        Assert.Contains("position", problem.Errors.Keys);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, 8)]
    public async Task PlayMove_OutsideOfBoard_ReturnsValidationProblem(int row, int column)
    {
        var created = await CreateGameAsync(new CreateGameRequest());

        var response = await PlayAsync(created.Id, new PositionDto(row, column));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PlayMove_UnknownGame_ReturnsNotFound()
    {
        var response = await PlayAsync(Guid.NewGuid(), new PositionDto(2, 3));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PlayMove_ComputersTurn_ReturnsConflict()
    {
        var created = await CreateGameAsync(new CreateGameRequest(GameMode.HumanVsComputer, Player.White));
        Assert.Equal(Player.Black, created.CurrentPlayer);

        var response = await PlayAsync(created.Id, new PositionDto(2, 3));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task ComputerMove_ComputersTurn_PlaysValidMove()
    {
        var created = await CreateGameAsync(new CreateGameRequest(GameMode.HumanVsComputer, Player.White));

        var response = await _client.PostAsync($"/api/games/{created.Id}/computer-move", null, CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var game = await ReadGameAsync(response);
        Assert.Equal(Player.White, game.CurrentPlayer);
        var turn = Assert.Single(game.History);
        Assert.Equal(Player.Black, turn.Player);
        Assert.Contains(turn.Position, created.ValidMoves);
    }

    [Fact]
    public async Task ComputerMove_HumansTurn_ReturnsConflict()
    {
        var created = await CreateGameAsync(new CreateGameRequest());

        var response = await _client.PostAsync($"/api/games/{created.Id}/computer-move", null, CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task ComputerMove_HumanVsHuman_ReturnsConflict()
    {
        var created = await CreateGameAsync(new CreateGameRequest(GameMode.HumanVsHuman));

        var response = await _client.PostAsync($"/api/games/{created.Id}/computer-move", null, CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task HumanVsHuman_BothColorsCanMove()
    {
        var created = await CreateGameAsync(new CreateGameRequest(GameMode.HumanVsHuman));

        var afterBlack = await ReadGameAsync(await PlayAsync(created.Id, new PositionDto(2, 3)));
        var afterWhite = await ReadGameAsync(await PlayAsync(created.Id, afterBlack.ValidMoves[0]));

        Assert.Equal(Player.Black, afterWhite.CurrentPlayer);
        Assert.Equal(2, afterWhite.History.Count);
    }

    [Theory]
    [InlineData(Player.Black)]
    [InlineData(Player.White)]
    public async Task FullGame_AgainstComputer_EndsWithResult(Player humanPlayer)
    {
        var game = await CreateGameAsync(new CreateGameRequest(GameMode.HumanVsComputer, humanPlayer));

        for (var i = 0; i < 200 && game.Status == GameStatus.InProgress; i++)
        {
            var response = game.CurrentPlayer == humanPlayer
                ? await PlayAsync(game.Id, game.ValidMoves[0])
                : await _client.PostAsync($"/api/games/{game.Id}/computer-move", null, CancellationToken);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            game = await ReadGameAsync(response);
        }

        Assert.NotEqual(GameStatus.InProgress, game.Status);
        Assert.Null(game.CurrentPlayer);
        Assert.Empty(game.ValidMoves);

        var afterEnd = await PlayAsync(game.Id, new PositionDto(0, 0));
        Assert.Equal(HttpStatusCode.Conflict, afterEnd.StatusCode);
    }

    [Fact]
    public async Task UnknownApiRoute_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/does-not-exist", CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Health_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/api/health", CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync(CancellationToken));
    }

    private static async Task<GameDto> ReadGameAsync(HttpResponseMessage response)
    {
        var game = await response.Content.ReadFromJsonAsync<GameDto>(JsonOptions, CancellationToken);
        Assert.NotNull(game);
        return game;
    }

    private async Task<GameDto> CreateGameAsync(CreateGameRequest request)
    {
        var response = await _client.PostAsJsonAsync("/api/games", request, JsonOptions, CancellationToken);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await ReadGameAsync(response);
    }

    private Task<HttpResponseMessage> PlayAsync(Guid id, PositionDto move) =>
        _client.PostAsJsonAsync($"/api/games/{id}/moves", move, JsonOptions, CancellationToken);
}
