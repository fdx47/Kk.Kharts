export function formatLocalIso(date) {
  const pad = (n) => (n < 10 ? "0" + n : "" + n);
  return (
    `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}` +
    `T${pad(date.getHours())}:${pad(date.getMinutes())}:${pad(date.getSeconds())}`
  );
}

export default formatLocalIso;
