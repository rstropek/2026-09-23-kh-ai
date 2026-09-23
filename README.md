# Othello

A demo implementation of the board game Othello (Reversi):

- **Backend:** ASP.NET Core Minimal API on .NET 11 (RC1) containing the entire game logic and a simple computer player.
- **Frontend:** React 19 + TypeScript 7 single-page app (Vite), served by the backend.

## Project layout

| Path | Content |
| --- | --- |
| `src/Othello.Core` | Game rules (board, moves, passes, game end) and `SimpleComputerPlayer` |
| `src/Othello.Api` | Minimal API (`/api/games`) and host for the SPA (`wwwroot`) |
| `src/Othello.Web` | React frontend, Biome config and Playwright e2e tests (`e2e/`) |
| `tests/Othello.Core.Tests` | xUnit v3 tests for the game logic and computer player |
| `tests/Othello.Api.Tests` | xUnit v3 integration tests for the API (`WebApplicationFactory`) |

## Getting started

Prerequisites: .NET SDK 11.0.100-rc.1 (see `global.json`) and Node.js 24.

```bash
# Build the frontend into src/Othello.Api/wwwroot, then run the backend
cd src/Othello.Web && npm ci && npm run build && cd ../..
dotnet run --project src/Othello.Api
# open http://localhost:5240
```

For frontend development with hot reload, keep the backend running and start `npm run dev` in
`src/Othello.Web` (API calls are proxied to the backend).

## Quality checks

```bash
# Backend: formatting, build (analyzers, warnings as errors), tests with coverage
dotnet format Othello.slnx --verify-no-changes
dotnet build Othello.slnx
dotnet test --solution Othello.slnx --coverage

# Frontend: lint + formatting (Biome), type check + build, e2e tests (Playwright)
cd src/Othello.Web
npm run lint        # npm run format to fix
npm run build
npm run test:e2e    # first time: npx playwright install chromium
```

## API

| Method | Route | Description |
| --- | --- | --- |
| `POST` | `/api/games` | Start a game: `{ "mode": "humanVsComputer" \| "humanVsHuman", "humanPlayer": "black" \| "white" }` |
| `GET` | `/api/games/{id}` | Get the game state |
| `POST` | `/api/games/{id}/moves` | Place a disc for the human player: `{ "row": 2, "column": 3 }` |
| `POST` | `/api/games/{id}/computer-move` | Let the computer make its move |

Invalid moves return `400` (validation problem), moves out of turn or after the game ended return `409`.
Passes are applied automatically and recorded in the game history.
