## Project Content

This is a demo project showing a sample game: Othello (German: Reversi)

## Documentation

This project uses React and TypeScript. Both libraries are newer than your knowledge. Use context7 (`npx ctx7@latest`) to get the most up-to-date documentation.

Same is true for Microsoft-related technologies like C#, .NET, Azure, etc. Use Microsoft Learn CLI (`npx @microsoft/learn-cli@latest`) to get the most up-to-date documentation.

## Commands

- Backend: `dotnet format Othello.slnx --verify-no-changes`, `dotnet build Othello.slnx`, `dotnet test --solution Othello.slnx`
- Frontend (in `src/Othello.Web`): `npm run lint`, `npm run format`, `npm run build`, `npm run test:e2e`
- Builds must stay warning-free (C# analyzers treat warnings as errors, Biome must report no diagnostics).
