export function alignAndInterpolateSeries(seriesList) {
  if (!Array.isArray(seriesList)) return [];

  const allTimestamps = [
    ...new Set(
      seriesList.flatMap((s) =>
        (Array.isArray(s.data) ? s.data : [])
          .map((p) =>
            p.x instanceof Date
              ? p.x.getTime()
              : typeof p.x === "string"
                ? Date.parse(p.x)
                : +p.x,
          )
          .filter((ts) => !Number.isNaN(ts)),
      ),
    ),
  ].sort((a, b) => a - b);

  return seriesList.map((orig) => {
    const sortedData = (Array.isArray(orig.data) ? orig.data : [])
      .map((p) => ({
        x:
          p.x instanceof Date
            ? p.x.getTime()
            : typeof p.x === "string"
              ? Date.parse(p.x)
              : +p.x,
        y: p.y,
      }))
      .sort((a, b) => a.x - b.x);

    const result = allTimestamps.map((ts) => {
      const exact = sortedData.find((d) => d.x === ts);
      if (exact) return { x: ts, y: exact.y };

      const idx = sortedData.findIndex((d) => d.x > ts);
      if (idx === -1) {
        return { x: ts, y: sortedData[sortedData.length - 1]?.y ?? null };
      }
      if (idx === 0) {
        return { x: ts, y: sortedData[0]?.y ?? null };
      }
      const prev = sortedData[idx - 1];
      const next = sortedData[idx];
      const ratio = (ts - prev.x) / (next.x - prev.x);
      const y = prev.y + ratio * (next.y - prev.y);
      return { x: ts, y };
    });

    return { ...orig, data: result };
  });
}

export default alignAndInterpolateSeries;
