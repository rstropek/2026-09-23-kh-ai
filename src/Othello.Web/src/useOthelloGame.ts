import { useCallback, useEffect, useEffectEvent, useState } from "react";
import {
  ApiError,
  createGame,
  type Game,
  getGame,
  type NewGameOptions,
  type Position,
  playComputerMove,
  playMove,
} from "./api";
import { isComputerTurn } from "./othello";

const STORAGE_KEY = "othello.gameId";

/** Pause before the computer answers so that the player can follow the board. */
export const COMPUTER_DELAY_MS = 700;

export const DEFAULT_OPTIONS: NewGameOptions = { mode: "humanVsComputer", humanPlayer: "black" };

function describeError(error: unknown) {
  if (error instanceof ApiError) {
    return error.message;
  }

  return "The game server can't be reached. Check that the backend is running, then start a new game.";
}

function rememberGame(id: string | null) {
  try {
    if (id) {
      localStorage.setItem(STORAGE_KEY, id);
    } else {
      localStorage.removeItem(STORAGE_KEY);
    }
  } catch {
    // Storage may be unavailable (e.g. private mode); resuming games is optional.
  }
}

function storedGameId() {
  try {
    return localStorage.getItem(STORAGE_KEY);
  } catch {
    return null;
  }
}

export function useOthelloGame() {
  const [game, setGame] = useState<Game | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(true);

  const run = useCallback(async (action: () => Promise<Game>) => {
    setBusy(true);
    setError(null);
    try {
      const result = await action();
      setGame(result);
      rememberGame(result.id);
    } catch (e) {
      setError(describeError(e));
    } finally {
      setBusy(false);
    }
  }, []);

  const newGame = useCallback((options: NewGameOptions) => run(() => createGame(options)), [run]);

  const play = useCallback(
    (position: Position) => {
      if (game && !busy) {
        void run(() => playMove(game.id, position));
      }
    },
    [game, busy, run],
  );

  // Resume the previous game after a reload, otherwise start a new one.
  const loadInitialGame = useEffectEvent(() => {
    const id = storedGameId();
    void run(async () => {
      if (id) {
        try {
          return await getGame(id);
        } catch {
          rememberGame(null);
        }
      }

      return createGame(DEFAULT_OPTIONS);
    });
  });

  useEffect(() => {
    loadInitialGame();
  }, []);

  const letComputerMove = useEffectEvent((id: string) => {
    void run(() => playComputerMove(id));
  });

  useEffect(() => {
    if (!game || !isComputerTurn(game)) {
      return;
    }

    const timer = setTimeout(() => letComputerMove(game.id), COMPUTER_DELAY_MS);
    return () => clearTimeout(timer);
  }, [game]);

  return { game, error, busy, newGame, play };
}
