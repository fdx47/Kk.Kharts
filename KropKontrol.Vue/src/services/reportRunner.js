// src/services/reportRunner.js

import { BASE_URL } from "./config.js";
import { authenticate } from "./authService.js";
import { getDeviceLastDays } from "./apiClient.js";
import { analyzeWateringSeries } from "./dataProcessor.js";
import { generateReportHtml } from "./reportGenerator.js";

/**
 * Exécute le flow complet de génération de rapport d'irrigation.
 *
 * @param {string} devEui         Identifiant du device
 * @param {number} daysForQuery   Nombre de jours à interroger
 * @param {string} email          Email pour l’authentification
 * @param {string} password       Mot de passe pour l’authentification
 * @returns {Promise<string>}     Le HTML complet du rapport
 */
export async function runReport(devEui, daysForQuery, email, password) {
  try {
    if (import.meta.env.DEV) {
      console.log("[reportRunner] Authentification en cours…");
    }
    await authenticate(email, password);

    if (import.meta.env.DEV) {
      console.log(
        `[reportRunner] Récupération des données pour ${devEui} sur ${daysForQuery} jours…`,
      );
    }
    const payload = await getDeviceLastDays(BASE_URL, devEui, daysForQuery);
    const arr = payload.data || [];
    if (import.meta.env.DEV) {
      console.log(`[reportRunner] Points de données reçus : ${arr.length}`);
    }

    if (import.meta.env.DEV) {
      console.log("[reportRunner] Analyse des arrosages (algo6)…");
    }
    const analysis = analyzeWateringSeries(arr, "permittivite");
    const events = Array.isArray(analysis?.events) ? analysis.events : [];
    if (import.meta.env.DEV) {
      console.log(
        `[reportRunner] Arrosages détectés : ${events.length} (points analysés : ${analysis?.points?.length ?? 0})`,
      );
    }

    if (import.meta.env.DEV) {
      console.log("[reportRunner] Génération du HTML du rapport…");
    }
    const html = generateReportHtml(
      arr,
      analysis,
      "permittivite",
      daysForQuery,
    );

    return html;
  } catch (err) {
    console.error("[reportRunner] Erreur dans runReport:", err.message);
    throw err;
  }
}
