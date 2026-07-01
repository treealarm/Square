// Shared angle math for "rotate by dragging a handle away from a center point" controls,
// used by both the map marker handle and the diagram object handle. Convention matches the
// existing __image_rotate / CSS `rotate()` usage: 0deg = up, increasing clockwise — i.e. a
// standard compass bearing.
export function bearingFromDelta(dx: number, dy: number): number {
  const deg = (Math.atan2(dx, -dy) * 180) / Math.PI;
  return (deg + 360) % 360;
}

export function offsetFromBearing(angleDeg: number, radius: number): { dx: number; dy: number } {
  const rad = (angleDeg * Math.PI) / 180;
  return { dx: radius * Math.sin(rad), dy: -radius * Math.cos(rad) };
}
