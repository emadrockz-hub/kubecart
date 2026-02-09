import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      "/api/catalog": { target: "http://localhost:5254", changeOrigin: true },
      "/api/orders": { target: "http://localhost:5102", changeOrigin: true },
      "/api/auth": { target: "http://localhost:5276", changeOrigin: true }
    }
  }
});
