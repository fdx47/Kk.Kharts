import { generateUniqueId } from "../utils/generateUniqueId.js";

export function useCharts(chartsRef, options = {}) {
  const idField = options.idField || "id";
  const duplicateId = options.duplicateId || (() => generateUniqueId());

  function addChart(chart) {
    chartsRef.value.push(chart);
  }

  function removeChart(id) {
    const index = chartsRef.value.findIndex((c) => c[idField] === id);
    if (index !== -1) chartsRef.value.splice(index, 1);
  }

  function duplicateChart(id) {
    const index = chartsRef.value.findIndex((c) => c[idField] === id);
    if (index === -1) return;
    const copy = {
      ...chartsRef.value[index],
      [idField]: duplicateId(chartsRef.value[index]),
    };
    chartsRef.value.push(copy);
  }

  function updateChartConfig(id, payload) {
    const index = chartsRef.value.findIndex((c) => c[idField] === id);
    if (index === -1) return;
    Object.assign(chartsRef.value[index], payload);
  }

  return { addChart, removeChart, duplicateChart, updateChartConfig };
}

export default useCharts;
