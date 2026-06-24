import { alpha } from '@mui/material';

// Mirrors vms_rec's web_vms.client/src/components/CameraUnifiedCard.tsx /
// ArchiveHlsStreamCard.tsx video-overlay styling (netTerrainTheme panel.elevated) so the Monitor
// tab in Square looks like the same screen, not a generic MUI list+player.
export const W = '#fff';
export const B = '#000';

export const STRIP_BG = { bgcolor: '#434343', backdropFilter: 'blur(6px)' } as const;
export const STRIP_W = 28;

export const BTN_SX = {
  p: '3px',
  color: alpha(W, 0.8),
  borderRadius: '4px',
  '&:hover': { color: W, bgcolor: alpha(W, 0.12) },
} as const;

export const NAME_LABEL_SX = {
  position: 'absolute' as const,
  bottom: 4,
  left: 4,
  zIndex: 2,
  fontSize: '0.65rem',
  color: alpha(W, 0.75),
  bgcolor: alpha(B, 0.45),
  px: 0.6,
  py: 0.15,
  borderRadius: 0.5,
  pointerEvents: 'none' as const,
  lineHeight: 1.4,
  maxWidth: 'calc(100% - 8px)',
  overflow: 'hidden',
  textOverflow: 'ellipsis',
  whiteSpace: 'nowrap' as const,
};
