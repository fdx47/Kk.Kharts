//vite.config.js

import { defineConfig, loadEnv } from "vite";
import vue from "@vitejs/plugin-vue";
import path from "path";

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd());

  return {
    base: env.VITE_BASE_PATH || "/",
    plugins: [vue()],
    build: {
      chunkSizeWarningLimit: 1024,
      rollupOptions: {
        output: {
          manualChunks: {
            vue: ["vue", "vue-router"],
            apexcharts: ["apexcharts"],
            pdfjs: ["pdfjs-dist"],
            vendor: ["axios", "lodash", "luxon", "idb"],
          },
        },
      },
    },
    server: {
      open: true, // Ouvre le navigateur automatiquement
    },
    resolve: {
      alias: {
        "@": path.resolve(__dirname, "src"),
      },
    },
    define: {
      __API_BASE_URL__: JSON.stringify(
        env.VITE_API_BASE_URL || "https://kropkontrol.premiumasp.net",
      ),
    },
  };
});
