import fs from "fs";
import path from "path";
import dotenv from "dotenv";

const modeArg = process.argv[2] || "production";
const envCandidates = [
  ".env",
  ".env.local",
  modeArg === "production" ? ".env.production" : `.env.${modeArg}`,
  modeArg === "production"
    ? ".env.production.local"
    : `.env.${modeArg}.local`,
].filter(Boolean);

const loadedEnvFiles = [];
for (const file of envCandidates) {
  if (file && fs.existsSync(file)) {
    dotenv.config({ path: file, override: false });
    loadedEnvFiles.push(file);
  }
}

if (loadedEnvFiles.length === 0) {
  console.warn(
    "[generate-htaccess] Aucun fichier .env spécifique trouvé, variables par défaut utilisées.",
  );
} else {
  console.log(
    `[generate-htaccess] Variables chargées depuis: ${loadedEnvFiles.join(", ")}`,
  );
}

const base = process.env.VITE_BASE_PATH || "/";

console.log(`[generate-htaccess] Base détectée: ${base}`);

const cleanBase = base.endsWith("/") ? base : base + "/";

const htaccessContent = `
RewriteEngine On
RewriteBase ${cleanBase}
RewriteRule ^index\\.html$ - [L]
RewriteCond %{REQUEST_FILENAME} !-f
RewriteCond %{REQUEST_FILENAME} !-d
RewriteRule . ${cleanBase}index.html [L]
`;

const distPath = path.resolve("./dist");

if (!fs.existsSync(distPath)) {
  console.error(
    'Erreur : le dossier "dist" n’existe pas. Faites le build d’abord.',
  );

  process.exit(1);
}

const htaccessPath = path.join(distPath, ".htaccess");
fs.writeFileSync(htaccessPath, htaccessContent.trim());

console.log(
  `Fichier .htaccess généré avec succès à l’emplacement : ${htaccessPath}`,
);
