export function downsampleLTTB(data, threshold) {
  if (!Array.isArray(data) || threshold <= 0 || data.length <= threshold) {
    return data;
  }

  const sampled = [];
  const dataLength = data.length;
  const every = (dataLength - 2) / (threshold - 2);
  let a = 0;
  sampled.push(data[a]);

  for (let i = 0; i < threshold - 2; i++) {
    const avgStart = Math.floor((i + 1) * every) + 1;
    const avgEnd = Math.min(Math.floor((i + 2) * every) + 1, dataLength);
    const avgLength = avgEnd - avgStart;

    let avgX = 0;
    let avgY = 0;
    for (let j = avgStart; j < avgEnd; j++) {
      const p = data[j];
      if (!p) continue;
      avgX += p.x;
      avgY += p.y;
    }
    if (avgLength) {
      avgX /= avgLength;
      avgY /= avgLength;
    }

    const rangeOffs = Math.floor(i * every) + 1;
    const rangeTo = Math.min(Math.floor((i + 1) * every) + 1, dataLength);

    let maxArea = -1;
    let maxAreaIndex = rangeOffs;
    const pointA = data[a];

    for (let j = rangeOffs; j < rangeTo; j++) {
      const point = data[j];
      if (!point) continue;
      const area =
        Math.abs(
          (pointA.x - avgX) * (point.y - pointA.y) -
            (pointA.x - point.x) * (avgY - pointA.y),
        ) * 0.5;
      if (area > maxArea) {
        maxArea = area;
        maxAreaIndex = j;
      }
    }

    sampled.push(data[maxAreaIndex]);
    a = maxAreaIndex;
  }

  sampled.push(data[dataLength - 1]);
  return sampled;
}

export default downsampleLTTB;
