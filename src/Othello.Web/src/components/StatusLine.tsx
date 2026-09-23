import type { Game, Player } from "../api";
import { capitalize, humanPlayerOf, isComputerTurn } from "../othello";

function nameOf(game: Game, player: Player) {
  if (game.mode === "humanVsHuman") {
    return capitalize(player);
  }

  return humanPlayerOf(game) === player ? "You" : "The computer";
}

function resultText(game: Game) {
  const { black, white } = game.score;
  const high = Math.max(black, white);
  const low = Math.min(black, white);
  if (game.status === "draw") {
    return `Draw, ${black} to ${white}.`;
  }

  const winner: Player = game.status === "blackWins" ? "black" : "white";
  const verb = nameOf(game, winner) === "You" ? "win" : "wins";
  return `${nameOf(game, winner)} ${verb} ${high} to ${low}.`;
}

function turnText(game: Game) {
  const player = game.currentPlayer ?? "black";
  if (isComputerTurn(game)) {
    return "The computer is thinking…";
  }

  if (game.mode === "humanVsHuman") {
    return `${capitalize(player)} to move.`;
  }

  return `Your move. Place a ${player} disc on a marked square.`;
}

function passText(game: Game) {
  const last = game.history.at(-1);
  if (!last || last.position !== null || game.status !== "inProgress") {
    return null;
  }

  const name = nameOf(game, last.player);
  return `${name} had no legal move, so the turn passes back.`;
}

export function StatusLine({ game }: { game: Game }) {
  const over = game.status !== "inProgress";
  const pass = passText(game);

  return (
    <div className="status" aria-live="polite">
      <p className="status-main" data-testid="status" data-status={game.status}>
        {over ? resultText(game) : turnText(game)}
      </p>
      {pass && (
        <p className="status-note" data-testid="pass-note">
          {pass}
        </p>
      )}
    </div>
  );
}
