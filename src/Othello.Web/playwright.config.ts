import { defineConfig, devices } from "@playwright/test";

const port = 5299;
const baseURL = `http://127.0.0.1:${port}`;

/**
 * End-to-end tests run against the ASP.NET Core backend serving the production build of the
 * frontend. Run `npm run build` before `npx playwright test` (or use `npm run test:e2e`).
 */
export default defineConfig({
  testDir: "./e2e",
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 1 : 0,
  reporter: process.env.CI ? [["list"], ["html", { open: "never" }]] : "list",
  use: {
    baseURL,
    trace: "on-first-retry",
  },
  projects: [
    { name: "chromium", use: { ...devices["Desktop Chrome"] } },
    { name: "mobile", use: { ...devices["Pixel 7"] } },
  ],
  webServer: {
    command: `dotnet run --project ../Othello.Api --no-launch-profile --urls ${baseURL}`,
    url: `${baseURL}/api/health`,
    reuseExistingServer: !process.env.CI,
    timeout: 180_000,
    env: { ASPNETCORE_ENVIRONMENT: "Production" },
  },
});
