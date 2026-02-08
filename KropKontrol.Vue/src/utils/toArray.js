export function toArray(raw) {
  return Array.isArray(raw)
    ? raw
    : raw && Array.isArray(raw.data)
      ? raw.data
      : [];
}

export default toArray;
