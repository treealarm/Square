import { useRef, useState, type PointerEvent } from 'react';
import { alpha, Box, IconButton, Tooltip, useTheme } from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import DragIndicatorIcon from '@mui/icons-material/DragIndicator';
import PtzControlPad from './PtzControlPad';

// Ported from vms_rec's web_vms.client/src/components/PtzOverlay.tsx verbatim — pure local
// drag-position state, no fetch calls of its own (those live in PtzControlPad).

function clamp(value: number, min: number, max: number) {
  return Math.min(Math.max(value, min), Math.max(min, max));
}

type Props = {
  cameraId: string;
  profileToken: string;
  onClose: () => void;
};

export default function PtzOverlay({ cameraId, profileToken, onClose }: Props) {
  const theme = useTheme();
  const overlayRef = useRef<HTMLDivElement>(null);
  const dragOffsetRef = useRef<{ x: number; y: number } | null>(null);
  const [pos, setPos] = useState<{ left: number; top: number } | null>(null);

  const onGripPointerDown = (e: PointerEvent<HTMLDivElement>) => {
    const el = overlayRef.current;
    const parent = el?.parentElement;
    if (!el || !parent) return;
    const elRect = el.getBoundingClientRect();
    const parentRect = parent.getBoundingClientRect();
    dragOffsetRef.current = { x: e.clientX - elRect.left, y: e.clientY - elRect.top };
    if (!pos) setPos({ left: elRect.left - parentRect.left, top: elRect.top - parentRect.top });
    e.currentTarget.setPointerCapture(e.pointerId);
  };

  const onGripPointerMove = (e: PointerEvent<HTMLDivElement>) => {
    const offset = dragOffsetRef.current;
    const el = overlayRef.current;
    const parent = el?.parentElement;
    if (!offset || !el || !parent) return;
    const parentRect = parent.getBoundingClientRect();
    const elRect = el.getBoundingClientRect();
    setPos({
      left: clamp(e.clientX - parentRect.left - offset.x, 0, parentRect.width - elRect.width),
      top: clamp(e.clientY - parentRect.top - offset.y, 0, parentRect.height - elRect.height),
    });
  };

  const onGripPointerUp = (e: PointerEvent<HTMLDivElement>) => {
    dragOffsetRef.current = null;
    e.currentTarget.releasePointerCapture(e.pointerId);
  };

  return (
    <Box
      ref={overlayRef}
      sx={{
        position: 'absolute',
        zIndex: 10,
        bgcolor: alpha(theme.palette.background.paper, 0.45),
        backdropFilter: 'blur(8px)',
        border: `1px solid ${theme.palette.divider}`,
        borderRadius: `${theme.shape.borderRadius}px`,
        p: 1,
        userSelect: 'none',
        ...(pos
          ? { left: pos.left, top: pos.top }
          : { left: 8, top: '50%', transform: 'translateY(-50%)' }),
      }}
    >
      {/* Drag handle + Close */}
      <Box sx={{ display: 'flex', alignItems: 'center', mb: 0.5 }}>
        <Box
          onPointerDown={onGripPointerDown}
          onPointerMove={onGripPointerMove}
          onPointerUp={onGripPointerUp}
          sx={{
            flex: 1,
            display: 'flex',
            alignItems: 'center',
            color: theme.palette.text.secondary,
            cursor: 'grab',
            touchAction: 'none',
            '&:active': { cursor: 'grabbing' },
          }}
        >
          <DragIndicatorIcon sx={{ fontSize: '1rem' }} />
        </Box>
        <Tooltip title="Close PTZ">
          <IconButton
            size="small"
            onClick={onClose}
            sx={{
              p: '2px',
              color: theme.palette.text.secondary,
              '&:hover': { color: theme.palette.text.primary },
            }}
          >
            <CloseIcon sx={{ fontSize: '0.85rem' }} />
          </IconButton>
        </Tooltip>
      </Box>

      <PtzControlPad cameraId={cameraId} profileToken={profileToken} />
    </Box>
  );
}
