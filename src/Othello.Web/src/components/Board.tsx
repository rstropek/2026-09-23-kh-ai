import type { Game, Position } from "../api";
import {
  BOARD_SIZE,
  COLUMNS,
  discAt,
  isComputerTurn,
  lastMove,
  samePosition,
  toNotation,
} from "../othello";

interface BoardProps {
  game: Game;
  disabled: boolean;
  onPlay: (position: Position) => void;
}

const indices = Array.from({ length: BOARD_SIZE }, (_, i) => i);

export function Board({ game, disabled, onPlay }: BoardProps) {
  const last = lastMove(game);
  const canPlay = !disabled && game.status === "inProgress" && !isComputerTurn(game);
  const hintColor = game.currentPlayer ?? "black";

  return (
    <div className="board-frame">
      <div className="board-labels board-labels--columns" aria-hidden="true">
        {COLUMNS.map((c) => (
          <span key={c}>{c}</span>
        ))}
      </div>
      <div className="board-labels board-labels--rows" aria-hidden="true">
        {indices.map((r) => (
          <span key={r}>{r + 1}</span>
        ))}
      </div>
      <section className="board" aria-label="Othello board" data-turns={game.history.length}>
        {indices.map((row) =>
          indices.map((column) => {
            const position = { row, column };
            const notation = toNotation(position);
            const disc = discAt(game, position);
            const valid = game.validMoves.some((m) => samePosition(m, position));
            const playable = canPlay && valid;
            const isLast = samePosition(last?.position, position);
            const label = disc
              ? `${notation}, ${disc} disc${isLast ? ", last move" : ""}`
              : `${notation}, empty${playable ? ", playable" : ""}`;

            return (
              <div key={notation} className="cell">
                <button
                  type="button"
                  className="cell-button"
                  data-testid={`cell-${notation}`}
                  data-disc={disc ?? "empty"}
                  data-valid={valid}
                  data-last={isLast}
                  aria-label={label}
                  disabled={!playable}
                  onClick={() => onPlay(position)}
                >
                  {disc && (
                    <span className={`disc disc--${disc}`}>
                      <span className="disc-face disc-face--black" />
                      <span className="disc-face disc-face--white" />
                    </span>
                  )}
                  {!disc && playable && <span className={`hint hint--${hintColor}`} />}
                  {isLast && <span className="last-marker" />}
                </button>
              </div>
            );
          }),
        )}
        <span className="star star--1" aria-hidden="true" />
        <span className="star star--2" aria-hidden="true" />
        <span className="star star--3" aria-hidden="true" />
        <span className="star star--4" aria-hidden="true" />
      </section>
    </div>
  );
}
