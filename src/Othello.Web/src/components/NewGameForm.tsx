import { useState } from "react";
import type { GameMode, NewGameOptions, Player } from "../api";

interface NewGameFormProps {
  initial: NewGameOptions;
  disabled: boolean;
  onStart: (options: NewGameOptions) => void;
}

export function NewGameForm({ initial, disabled, onStart }: NewGameFormProps) {
  const [mode, setMode] = useState<GameMode>(initial.mode);
  const [humanPlayer, setHumanPlayer] = useState<Player>(initial.humanPlayer);

  return (
    <form
      className="new-game"
      aria-label="New game"
      onSubmit={(e) => {
        e.preventDefault();
        onStart({ mode, humanPlayer });
      }}
    >
      <fieldset className="choice">
        <legend>Opponent</legend>
        <label>
          <input
            type="radio"
            name="mode"
            value="humanVsComputer"
            checked={mode === "humanVsComputer"}
            onChange={() => setMode("humanVsComputer")}
          />
          <span>Computer</span>
        </label>
        <label>
          <input
            type="radio"
            name="mode"
            value="humanVsHuman"
            checked={mode === "humanVsHuman"}
            onChange={() => setMode("humanVsHuman")}
          />
          <span>Friend on this screen</span>
        </label>
      </fieldset>

      {mode === "humanVsComputer" && (
        <fieldset className="choice">
          <legend>You play</legend>
          <label>
            <input
              type="radio"
              name="humanPlayer"
              value="black"
              checked={humanPlayer === "black"}
              onChange={() => setHumanPlayer("black")}
            />
            <span>Black, moves first</span>
          </label>
          <label>
            <input
              type="radio"
              name="humanPlayer"
              value="white"
              checked={humanPlayer === "white"}
              onChange={() => setHumanPlayer("white")}
            />
            <span>White</span>
          </label>
        </fieldset>
      )}

      <button type="submit" className="start-button" disabled={disabled}>
        Start new game
      </button>
    </form>
  );
}
