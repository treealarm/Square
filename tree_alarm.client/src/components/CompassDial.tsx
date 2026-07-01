import * as React from 'react';
import { useRef } from 'react';
import { Box, TextField } from '@mui/material';
import { bearingFromDelta, offsetFromBearing } from './rotation';

export interface ICompassDialProps {
  value: number; // degrees, 0 = up/north, clockwise
  // eslint-disable-next-line no-unused-vars
  onChange: (deg: number) => void;
  size?: number;
  label?: string;
}

// Small dial + numeric field for setting an object's orientation "by hand" — dragging the
// dial's needle is friendlier than typing raw degrees, but the field stays for precise input.
export function CompassDial({ value, onChange, size = 56, label = 'Rotation' }: ICompassDialProps) {
  const svgRef = useRef<SVGSVGElement | null>(null);
  const angle = ((value % 360) + 360) % 360;

  const rotateFromPointer = (clientX: number, clientY: number) => {
    const rect = svgRef.current?.getBoundingClientRect();
    if (!rect) return;
    const cx = rect.left + rect.width / 2;
    const cy = rect.top + rect.height / 2;
    onChange(Math.round(bearingFromDelta(clientX - cx, clientY - cy)));
  };

  const onPointerDown = (e: React.PointerEvent<SVGSVGElement>) => {
    e.preventDefault();
    rotateFromPointer(e.clientX, e.clientY);
    const onMove = (ev: PointerEvent) => rotateFromPointer(ev.clientX, ev.clientY);
    const onUp = () => {
      window.removeEventListener('pointermove', onMove);
      window.removeEventListener('pointerup', onUp);
    };
    window.addEventListener('pointermove', onMove);
    window.addEventListener('pointerup', onUp);
  };

  const r = size / 2 - 4;
  const { dx, dy } = offsetFromBearing(angle, r);

  return (
    <Box display="flex" alignItems="center" gap={1}>
      <svg
        ref={svgRef}
        width={size}
        height={size}
        onPointerDown={onPointerDown}
        style={{ cursor: 'pointer', touchAction: 'none' }}
      >
        <circle cx={size / 2} cy={size / 2} r={r} fill="none" stroke="#999" strokeWidth={1} />
        <line x1={size / 2} y1={size / 2} x2={size / 2 + dx} y2={size / 2 + dy} stroke="#1976d2" strokeWidth={2} />
        <circle cx={size / 2 + dx} cy={size / 2 + dy} r={3} fill="#1976d2" />
      </svg>
      <TextField
        size="small"
        label={label}
        type="number"
        value={Math.round(angle)}
        onChange={(e) => onChange(((Number(e.target.value) % 360) + 360) % 360)}
        sx={{ width: 90 }}
      />
    </Box>
  );
}
