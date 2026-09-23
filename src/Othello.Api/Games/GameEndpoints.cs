using Microsoft.AspNetCore.Http.HttpResults;

using Othello.Core;

namespace Othello.Api.Games;

public static class GameEndpoints
{
    public static IEndpointRouteBuilder MapGameEndpoints(this IEndpointRouteBuilder app)
    {
        var games = app.MapGroup("/api/games");

        games.MapPost("/", CreateGame);
        games.MapGet("/{id:guid}", GetGame);
        games.MapPost("/{id:guid}/moves", PlayMove);
        games.MapPost("/{id:guid}/computer-move", PlayComputerMove);

        return app;
    }

    internal static Created<GameDto> CreateGame(CreateGameRequest? request, GameStore store)
    {
        request ??= new CreateGameRequest();
        var session = store.Create(request.Mode, request.HumanPlayer);
        return TypedResults.Created($"/api/games/{session.Id}", session.ToDto());
    }

    internal static Results<Ok<GameDto>, NotFound> GetGame(Guid id, GameStore store)
    {
        if (store.Find(id) is not { } session)
        {
            return TypedResults.NotFound();
        }

        lock (session.Lock)
        {
            return TypedResults.Ok(session.ToDto());
        }
    }

    internal static Results<Ok<GameDto>, NotFound, ValidationProblem, ProblemHttpResult> PlayMove(
        Guid id, PositionDto move, GameStore store)
    {
        if (store.Find(id) is not { } session)
        {
            return TypedResults.NotFound();
        }

        if (!Position.IsOnBoard(move.Row, move.Column))
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                ["position"] = [$"Row and column must be between 0 and {Board.Size - 1}."],
            });
        }

        lock (session.Lock)
        {
            if (session.Game.IsOver)
            {
                return Conflict("The game is already over.");
            }

            if (session.IsComputerTurn)
            {
                return Conflict("It is the computer's turn.");
            }

            var position = new Position(move.Row, move.Column);
            if (!session.Game.IsValidMove(position))
            {
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["position"] = [$"{session.Game.CurrentPlayer} cannot place a disc on {position}."],
                });
            }

            session.Game.Play(position);
            return TypedResults.Ok(session.ToDto());
        }
    }

    internal static Results<Ok<GameDto>, NotFound, ProblemHttpResult> PlayComputerMove(
        Guid id, GameStore store, IComputerPlayer computer)
    {
        if (store.Find(id) is not { } session)
        {
            return TypedResults.NotFound();
        }

        lock (session.Lock)
        {
            if (!session.IsComputerTurn)
            {
                return Conflict(session.Game.IsOver ? "The game is already over." : "It is not the computer's turn.");
            }

            var player = session.Game.CurrentPlayer!.Value;
            session.Game.Play(computer.ChooseMove(session.Game.Board, player));
            return TypedResults.Ok(session.ToDto());
        }
    }

    private static ProblemHttpResult Conflict(string detail) =>
        TypedResults.Problem(detail, statusCode: StatusCodes.Status409Conflict, title: "Move not possible");
}
