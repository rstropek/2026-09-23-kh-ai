import { expect, type Page, test } from "@playwright/test";

const cell = (page: Page, notation: string) => page.getByTestId(`cell-${notation}`);
const board = (page: Page) => page.getByRole("region", { name: "Othello board" });
const status = (page: Page) => page.getByTestId("status");

async function startNewGame(
  page: Page,
  opponent: "Computer" | "Friend on this screen",
  color?: "Black, moves first" | "White",
) {
  await page.getByRole("radio", { name: opponent, exact: true }).check();
  if (color) {
    await page.getByRole("radio", { name: color, exact: true }).check();
  }

  await page.getByRole("button", { name: "Start new game" }).click();
}

test.beforeEach(async ({ page }) => {
  await page.goto("/");
  // Always begin with a fresh game instead of a resumed one.
  await startNewGame(page, "Computer", "Black, moves first");
  await expect(board(page)).toHaveAttribute("data-turns", "0");
});

test("shows the starting position", async ({ page }) => {
  await expect(page).toHaveTitle("Othello");
  await expect(cell(page, "d4")).toHaveAttribute("data-disc", "white");
  await expect(cell(page, "e4")).toHaveAttribute("data-disc", "black");
  await expect(cell(page, "d5")).toHaveAttribute("data-disc", "black");
  await expect(cell(page, "e5")).toHaveAttribute("data-disc", "white");
  await expect(page.getByTestId("score-black")).toContainText("2");
  await expect(page.getByTestId("score-white")).toContainText("2");
  await expect(status(page)).toHaveText("Your move. Place a black disc on a marked square.");

  await expect(page.locator('[data-valid="true"]:enabled')).toHaveCount(4);
  await expect(cell(page, "a1")).toBeDisabled();
});

test("flips discs and lets the computer answer", async ({ page }) => {
  await cell(page, "d3").click();

  await expect(cell(page, "d3")).toHaveAttribute("data-disc", "black");
  await expect(cell(page, "d4")).toHaveAttribute("data-disc", "black");

  // The computer (white) answers after a short pause.
  await expect(board(page)).toHaveAttribute("data-turns", "2");
  await expect(status(page)).toHaveText("Your move. Place a black disc on a marked square.");
  await expect(page.locator('[data-last="true"]')).toHaveAttribute("data-disc", "white");
  await expect(page.locator('[data-disc="black"]')).toHaveCount(3);
  await expect(page.locator('[data-disc="white"]')).toHaveCount(3);
});

test("computer opens when the player chooses white", async ({ page }) => {
  await startNewGame(page, "Computer", "White");

  await expect(board(page)).toHaveAttribute("data-turns", "1");
  await expect(page.getByTestId("score-black")).toContainText("4");
  await expect(page.getByTestId("score-white")).toContainText("1");
  await expect(status(page)).toHaveText("Your move. Place a white disc on a marked square.");
});

test("resumes the current game after a reload", async ({ page }) => {
  await cell(page, "d3").click();
  await expect(board(page)).toHaveAttribute("data-turns", "2");

  await page.reload();

  await expect(board(page)).toHaveAttribute("data-turns", "2");
  await expect(cell(page, "d3")).toHaveAttribute("data-disc", "black");
});

test("two players can play a full game on one screen", async ({ page }) => {
  test.slow();
  await startNewGame(page, "Friend on this screen");
  await expect(status(page)).toHaveText("Black to move.");

  for (let turns = 0; (await status(page).getAttribute("data-status")) === "inProgress"; ) {
    await page.locator('[data-valid="true"]:enabled').first().click();
    await expect(board(page)).not.toHaveAttribute("data-turns", String(turns));
    turns = Number(await board(page).getAttribute("data-turns"));
  }

  await expect(status(page)).toHaveText(/^(Black wins|White wins|Draw)/);
  await expect(page.locator('[data-valid="true"]:enabled')).toHaveCount(0);
  const black = Number(await page.getByTestId("score-black").locator(".score-count").textContent());
  const white = Number(await page.getByTestId("score-white").locator(".score-count").textContent());
  expect(black + white).toBeGreaterThan(4);
  expect(black + white).toBeLessThanOrEqual(64);
});
