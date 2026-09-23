export type Player = "black" | "white";
export type GameMode = "humanVsComputer" | "humanVsHuman";
export type GameStatus = "inProgress" | "blackWins" | "whiteWins" | "draw";

export interface Position {
  row: number;
  column: number;
}

export interface Turn {
  player: Player;
  /** `null` if the player had to pass. */
  position: Position | null;
  flipped: Position[];
}

export interface Game {
  id: string;
  mode: GameMode;
  computerPlayer: Player | null;
  /** Eight rows of eight characters: `.` (empty), `B` (black) or `W` (white). */
  board: string[];
  currentPlayer: Player | null;
  status: GameStatus;
  score: { black: number; white: number };
  validMoves: Position[];
  history: Turn[];
}

export interface NewGameOptions {
  mode: GameMode;
  humanPlayer: Player;
}

export class ApiError extends Error {
  readonly status: number;

  constructor(status: number, message: string) {
    super(message);
    this.name = "ApiError";
    this.status = status;
  }
}

async function request(url: string, init?: RequestInit): Promise<Game> {
  const response = await fetch(url, {
    ...init,
    headers: { "Content-Type": "application/json", Accept: "application/json" },
  });
  if (!response.ok) {
    const problem = (await response.json().catch(() => null)) as {
      title?: string;
      detail?: string;
    } | null;
    throw new ApiError(
      response.status,
      problem?.detail ?? problem?.title ?? `Request failed with status ${response.status}.`,
    );
  }

  return (await response.json()) as Game;
}

export const createGame = (options: NewGameOptions) =>
  request("/api/games", { method: "POST", body: JSON.stringify(options) });

export const getGame = (id: string) => request(`/api/games/${encodeURIComponent(id)}`);

export const playMove = (id: string, position: Position) =>
  request(`/api/games/${encodeURIComponent(id)}/moves`, {
    method: "POST",
    body: JSON.stringify(position),
  });

export const playComputerMove = (id: string) =>
  request(`/api/games/${encodeURIComponent(id)}/computer-move`, { method: "POST" });
