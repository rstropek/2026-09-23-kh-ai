import react from "@vitejs/plugin-react";
import { defineConfig } from "vite";

// The ASP.NET Core backend (src/Othello.Api) serves the production build from its wwwroot folder.
// During development, API calls are proxied to the backend.
export default defineConfig({
  plugins: [react()],
  build: {
    outDir: "../Othello.Api/wwwroot",
    emptyOutDir: true,
  },
  server: {
    proxy: {
      "/api": "http://localhost:5240",
    },
  },
});
