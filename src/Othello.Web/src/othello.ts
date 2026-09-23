import type { Game, Player, Position } from "./api";

export const BOARD_SIZE = 8;
export const COLUMNS = ["a", "b", "c", "d", "e", "f", "g", "h"] as const;

export const toNotation = ({ row, column }: Position) => `${COLUMNS[column]}${row + 1}`;

export const samePosition = (a: Position | null | undefined, b: Position) =>
  a != null && a.row === b.row && a.column === b.column;

export function discAt(game: Game, { row, column }: Position): Player | null {
  const cell = game.board[row]?.[column];
  return cell === "B" ? "black" : cell === "W" ? "white" : null;
}

export const opponentOf = (player: Player): Player => (player === "black" ? "white" : "black");

export const isComputerTurn = (game: Game) =>
  game.status === "inProgress" &&
  game.computerPlayer !== null &&
  game.currentPlayer === game.computerPlayer;

export const humanPlayerOf = (game: Game): Player | null =>
  game.computerPlayer === null ? null : opponentOf(game.computerPlayer);

/** The last placed disc (ignoring passes). */
export function lastMove(game: Game) {
  for (let i = game.history.length - 1; i >= 0; i--) {
    const turn = game.history[i];
    if (turn?.position) {
      return turn;
    }
  }

  return null;
}

export const capitalize = (text: string) => text.charAt(0).toUpperCase() + text.slice(1);
