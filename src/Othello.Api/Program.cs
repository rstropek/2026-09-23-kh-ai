using System.Text.Json;
using System.Text.Json.Serialization;

using Othello.Api.Games;
using Othello.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<GameStore>();
builder.Services.AddSingleton<IComputerPlayer>(_ => new SimpleComputerPlayer());
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

// The React single-page app is built into wwwroot (see src/Othello.Web).
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapHealthChecks("/api/health");
app.MapGameEndpoints();

// Unknown API routes must not be answered with the SPA's index.html.
app.Map("/api/{**path}", () => TypedResults.NotFound());
app.MapFallbackToFile("index.html");

app.Run();
