/**
 * @file useCumulativeCalculations.js
 * @description
 * Fournit des fonctions pour calculer des variables cumulatives sur une période (ex: journée).
 * Ces calculs sont "stateful" et nécessitent un ensemble de mesures sur une journée complète.
 */

import { useComputedVars } from "./useComputedVars.js"; // Assurez-vous que le chemin est correct

/**
 * Calcule le DLI (Daily Light Integral) et le cumul de rayonnement en J/cm² pour une liste de mesures sur une journée.
 *
 * @param {Array<Object>} dailyRecords - Un tableau de mesures pour une journée, triées par timestamp.
 * Chaque objet doit avoir `timestamp` et `ModbusChannel1`.
 * @returns {Object} Un objet contenant le DLI total (`dli`) et le cumul de Joules (`joules_cm2`).
 */
export function calculateDailyCumulativeValues(dailyRecords) {
  let totalDli = 0;
  let totalJoulesPerM2 = 0; // On calcule d'abord en J/m²

  if (!dailyRecords || dailyRecords.length < 2) {
    return { dli: 0, joules_cm2: 0 };
  }

  const instantaneousVars = useComputedVars(61);

  const tsOf = (rec) =>
    rec?._ts ??
    (rec?.timestamp instanceof Date
      ? rec.timestamp.getTime()
      : Date.parse(rec?.timestamp));

  for (let i = 1; i < dailyRecords.length; i++) {
    const currentRecord = dailyRecords[i];
    const prevRecord = dailyRecords[i - 1];

    const currentTime = tsOf(currentRecord);
    const prevTime = tsOf(prevRecord);
    const deltaSeconds = (currentTime - prevTime) / 1000;

    if (deltaSeconds <= 0 || deltaSeconds > 3600) {
      continue;
    }

    // --- Calcul du DLI ---
    const par = prevRecord.ModbusChannel1;
    if (par != null && par > 0) {
      const intervalMicroMol = par * deltaSeconds;
      totalDli += intervalMicroMol / 1_000_000;
    }

    // --- Calcul du cumul de Rayonnement en Joules/m² ---
    const wattsPerM2 = instantaneousVars.parWm2.compute(prevRecord);
    if (wattsPerM2 != null && wattsPerM2 > 0) {
      const intervalJoules = wattsPerM2 * deltaSeconds;
      totalJoulesPerM2 += intervalJoules;
    }
  }

  // Conversion finale en J/cm²
  const totalJoulesPerCm2 = totalJoulesPerM2 / 10000;

  return {
    dli: totalDli,
    joules_cm2: totalJoulesPerCm2, // On retourne directement la bonne unité
  };
}

/**
 * Génère une série cumulative pour le DLI et le rayonnement sur une journée.
 * Chaque élément représente le total cumulé jusqu'au timestamp correspondant.
 *
 * @param {Array<Object>} dailyRecords - Mesures triées par timestamp.
 * @returns {Array<Object>} Tableau d'objets { timestamp, dli, joules_cm2 }
 */
export function calculateCumulativeSeries(dailyRecords) {
  const series = [];
  if (!dailyRecords || !dailyRecords.length) return series;

  let totalDli = 0;
  let totalJoulesPerM2 = 0;
  const instantaneousVars = useComputedVars(61);

  const tsOf = (rec) =>
    rec?._ts ??
    (rec?.timestamp instanceof Date
      ? rec.timestamp.getTime()
      : Date.parse(rec?.timestamp));

  // Premier point à 0 pour la première mesure
  {
    const t0 = tsOf(dailyRecords[0]);
    series.push({
      ts: t0,
      dli: 0,
      joules_cm2: 0,
    });
  }

  for (let i = 1; i < dailyRecords.length; i++) {
    const current = dailyRecords[i];
    const prev = dailyRecords[i - 1];

    const currentTime = tsOf(current);
    const prevTime = tsOf(prev);
    const deltaSeconds = (currentTime - prevTime) / 1000;

    if (deltaSeconds > 0 && deltaSeconds <= 3600) {
      const par = prev.ModbusChannel1;
      if (par != null && par > 0) {
        const intervalMicroMol = par * deltaSeconds;
        totalDli += intervalMicroMol / 1_000_000;
      }

      const wattsPerM2 = instantaneousVars.parWm2.compute(prev);
      if (wattsPerM2 != null && wattsPerM2 > 0) {
        totalJoulesPerM2 += wattsPerM2 * deltaSeconds;
      }
    }

    series.push({
      ts: currentTime,
      dli: totalDli,
      joules_cm2: totalJoulesPerM2 / 10000,
    });
  }

  return series;
}

export default calculateDailyCumulativeValues;
