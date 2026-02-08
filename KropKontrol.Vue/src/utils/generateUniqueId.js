export function generateUniqueId(prefix = "", withRandom = true) {
  const base = `${prefix}${Date.now()}`;
  return withRandom ? `${base}-${Math.floor(Math.random() * 10000)}` : base;
}

export default generateUniqueId;
