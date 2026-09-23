import type { Game, Player } from "../api";
import { capitalize, humanPlayerOf } from "../othello";

function playerName(game: Game, player: Player) {
  if (game.mode === "humanVsHuman") {
    return capitalize(player);
  }

  return humanPlayerOf(game) === player ? "You" : "Computer";
}

export function Scoreboard({ game }: { game: Game }) {
  const total = game.score.black + game.score.white;
  const blackShare = total === 0 ? 50 : (game.score.black / total) * 100;

  return (
    <section className="scoreboard" aria-label="Score">
      <div className="score-row">
        {(["black", "white"] as const).map((player) => (
          <div
            key={player}
            className="score"
            data-testid={`score-${player}`}
            data-active={game.currentPlayer === player}
          >
            <span className={`score-disc score-disc--${player}`} aria-hidden="true" />
            <span className="score-count">{game.score[player]}</span>
            <span className="score-name">
              {playerName(game, player)}
              <span className="visually-hidden"> ({player})</span>
            </span>
          </div>
        ))}
      </div>
      <div
        className="share-bar"
        role="img"
        aria-label={`Black holds ${game.score.black} of ${total} discs`}
      >
        <span className="share-bar-black" style={{ width: `${blackShare}%` }} />
      </div>
    </section>
  );
}
