import { ref } from "vue";
import { getDocument, GlobalWorkerOptions } from "pdfjs-dist";

// Pour pdfjs-dist v5+ ESM :
GlobalWorkerOptions.workerSrc = new URL(
  "pdfjs-dist/build/pdf.worker.mjs",
  import.meta.url,
).href;

// Mapping des patterns d'extraction par type
const extractionPatterns = {
  Apport: /Apport\s*[:=-]?\s*(\d+\.?\d*)/i,
  Drain: /Drain\s*[:=-]?\s*(\d+\.?\d*)/i,
  Substrat: /Substrat\s*[:=-]?\s*(\d+\.?\d*)/i,
  "Jeune feuille": /Jeune\s*feuille\s*[:=-]?\s*(\d+\.?\d*)/i,
  "Vieille feuille": /Vieille\s*feuille\s*[:=-]?\s*(\d+\.?\d*)/i,
};

/**
 * Composable pour analyser les PDFs d'eau (Apport/Drain/Substrat)
 */
function usePdfAnalysisWater() {
  const extractedData = ref([]);
  const chartData = ref([]);

  /**
   * Lit un PDF et extrait les valeurs macro/microéléments ou un fallback
   */
  async function parsePDF(file, type) {
    const buffer = await file.arrayBuffer();
    const loadingTask = getDocument({ data: buffer });
    const pdf = await loadingTask.promise;
    let fullText = "";

    for (let i = 1; i <= pdf.numPages; i++) {
      const page = await pdf.getPage(i);
      const content = await page.getTextContent();
      fullText += content.items.map((item) => item.str).join(" ") + "\n";
    }

    if (["Apport", "Drain", "Substrat"].includes(type)) {
      const text = fullText.replace(/,/g, ".");
      const lines = text.split(/\r?\n/);

      // 1. Chercher la ligne la plus riche en valeurs
      let bestLine = "";
      let maxCount = 0;
      for (const l of lines) {
        const matches = l.match(/(<\d+(?:\.\d+)?|\d+(?:\.\d+)?)/g);
        if (matches && matches.length > maxCount) {
          maxCount = matches.length;
          bestLine = l;
        }
      }

      if (import.meta.env.DEV) {
        console.log("[PDF DEBUG] bestLine:", bestLine);
      }
      if (bestLine) {
        let found = bestLine.match(/(<\d+(?:\.\d+)?|\d+(?:\.\d+)?)/g) || [];
        if (import.meta.env.DEV) {
          console.log("[PDF DEBUG] all found values:", found);
        }

        // Choix du bon découpage en fonction du type
        if (type === "Apport") {
          found = found.slice(25, 46);
        } else if (type === "Drain") {
          found = found.slice(31, 51); // <-- AJUSTE après test sur ton PDF Drain
        } else if (type === "Substrat") {
          found = found.slice(XX, YY); // à ajuster
        } else {
          found = found.slice(0, 21);
        }
        if (import.meta.env.DEV) {
          console.log("[PDF DEBUG] sliced values for", type, ":", found);
        }

        const keys = [
          "EC",
          "pH",
          "NH4",
          "K",
          "Na",
          "Ca",
          "K/Ca",
          "Mg",
          "Si",
          "NO3",
          "Cl",
          "S",
          "HCO3",
          "P",
          "Fe",
          "Mn",
          "Zn",
          "B",
          "Cu",
          "Mo",
          "Al",
        ];
        const result = {};
        for (let i = 0; i < keys.length; i++) {
          result[keys[i]] = found[i] || null;
        }
        if (import.meta.env.DEV) {
          console.log("[PDF DEBUG] result:", result);
        }
        return result;
      }
    }

    // Fallback : un unique nombre selon pattern
    const pattern =
      extractionPatterns[type] || /Valeur\s*[:=-]?\s*(\d+\.?\d*)/i;
    const match = fullText.match(pattern);
    return { value: match ? parseFloat(match[1]) : null };
  }

  /**
   * Traite un objet { type: File } et met à jour extractedData & chartData
   * @param filesMap Object mapping types -> File
   * @param progressCb Callback progress en %
   */
  async function processFiles(filesMap, progressCb) {
    extractedData.value = [];
    chartData.value = [];
    const types = Object.keys(filesMap);
    const total = types.length;
    let count = 0;

    for (const type of types) {
      const file = filesMap[type];
      if (file) {
        const info = await parsePDF(file, type);
        extractedData.value.push({ type, info });
      }
      count++;
      if (progressCb) progressCb((count / total) * 100);
    }

    chartData.value = extractedData.value.map((item) => ({
      x: item.type,
      y: item.info.value || 0,
    }));
  }

  return { extractedData, chartData, parsePDF, processFiles };
}

export { usePdfAnalysisWater };
export default usePdfAnalysisWater;
