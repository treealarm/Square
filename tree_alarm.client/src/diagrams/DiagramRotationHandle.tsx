import * as React from 'react';
import { useRef } from 'react';
import { bearingFromDelta } from '../components/rotation';

export interface IDiagramRotationHandleProps {
  // eslint-disable-next-line no-unused-vars
  onRotate: (deg: number) => void;
}

// Small drag handle rendered as a child of the diagram object's absolutely-positioned Box —
// rotates the object by dragging, around the Box's own screen center, independent of zoom.
export function DiagramRotationHandle({ onRotate }: IDiagramRotationHandleProps) {
  const ref = useRef<HTMLDivElement | null>(null);

  const onPointerDown = (e: React.PointerEvent) => {
    e.preventDefault();
    e.stopPropagation();
    const rect = ref.current?.parentElement?.getBoundingClientRect();
    if (!rect) return;
    const cx = rect.left + rect.width / 2;
    const cy = rect.top + rect.height / 2;

    const onMove = (ev: PointerEvent) => onRotate(Math.round(bearingFromDelta(ev.clientX - cx, ev.clientY - cy)));
    const onUp = () => {
      window.removeEventListener('pointermove', onMove);
      window.removeEventListener('pointerup', onUp);
    };
    window.addEventListener('pointermove', onMove);
    window.addEventListener('pointerup', onUp);
  };

  return (
    <div
      ref={ref}
      onPointerDown={onPointerDown}
      style={{
        position: 'absolute',
        top: -16,
        left: '50%',
        width: 12,
        height: 12,
        marginLeft: -6,
        borderRadius: '50%',
        background: '#1976d2',
        border: '2px solid white',
        boxShadow: '0 0 2px rgba(0,0,0,0.5)',
        cursor: 'grab',
        touchAction: 'none',
      }}
    />
  );
}
