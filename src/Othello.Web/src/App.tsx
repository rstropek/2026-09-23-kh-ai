import { Board } from "./components/Board";
import { NewGameForm } from "./components/NewGameForm";
import { Scoreboard } from "./components/Scoreboard";
import { StatusLine } from "./components/StatusLine";
import { DEFAULT_OPTIONS, useOthelloGame } from "./useOthelloGame";

export default function App() {
  const { game, error, busy, newGame, play } = useOthelloGame();

  return (
    <div className="layout">
      <header className="masthead">
        <h1>Othello</h1>
        <p className="tagline">Outflank your opponent. The most discs at the end wins.</p>
      </header>

      <main className="table">
        <div className="board-area">
          {game ? (
            <Board game={game} disabled={busy} onPlay={play} />
          ) : (
            <div className="board-frame board-frame--empty" aria-busy="true" />
          )}
        </div>

        <aside className="panel">
          {game && <Scoreboard game={game} />}
          {game && <StatusLine game={game} />}
          {error && (
            <p className="error" role="alert">
              {error}
            </p>
          )}
          <NewGameForm
            initial={DEFAULT_OPTIONS}
            disabled={busy && game === null && error === null}
            onStart={newGame}
          />
        </aside>
      </main>
    </div>
  );
}
