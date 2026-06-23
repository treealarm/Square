import { useMemo } from 'react';
import { Box, Typography } from '@mui/material';
import SquareCameraCard from './SquareCameraCard';
import { alpha } from '@mui/material';
import { W } from './monitorStyle';

// Square-native equivalent of vms_rec's web_vms.client/src/components/MonitorGrid.tsx — same CSS
// grid math (dto/dtos.tsx:283-299), but strictly read-only: no "switch camera in this slot" menu,
// since reassigning a monitor's slots is editing vms_rec's own config, which stays vms_rec-only
// (see docs/square-integration-plan.md, Фаза 3 часть 2). No solo-slot mode either — not asked for.

export type MonitorLayout = '1x1' | '1x2' | '2x2' | '3x3';

export interface MonitorDto {
  monitorId: string;
  name: string;
  layout: MonitorLayout;
  cameraIds: string[];
  slots: { slotIndex: number; cameraId: string }[];
}

function layoutSlotCount(layout: MonitorLayout): number {
  switch (layout) {
    case '1x1': return 1;
    case '1x2': return 2;
    case '2x2': return 4;
    case '3x3': return 9;
  }
}

function layoutColumns(layout: MonitorLayout): number {
  switch (layout) {
    case '1x1': return 1;
    case '1x2': return 2;
    case '2x2': return 2;
    case '3x3': return 3;
  }
}

function layoutRows(layout: MonitorLayout): number {
  switch (layout) {
    case '1x1': return 1;
    case '1x2': return 1;
    case '2x2': return 2;
    case '3x3': return 3;
  }
}

type Props = {
  monitor: MonitorDto;
  cameraNamesById: Map<string, string>;
};

export default function MonitorGridViewer({ monitor, cameraNamesById }: Props) {
  const slotCount = layoutSlotCount(monitor.layout);
  const cols = layoutColumns(monitor.layout);
  const rows = layoutRows(monitor.layout);

  const slots = useMemo(() => Array.from({ length: slotCount }, (_, i) => i), [slotCount]);

  return (
    <Box
      sx={{
        display: 'grid',
        gap: 1,
        flex: 1,
        minHeight: 0,
        width: '100%',
        height: '100%',
        gridTemplateColumns: `repeat(${cols}, minmax(0, 1fr))`,
        gridTemplateRows: `repeat(${rows}, minmax(0, 1fr))`,
      }}
    >
      {slots.map((slotIndex) => {
        const cameraId = monitor.slots.find((s) => s.slotIndex === slotIndex)?.cameraId;
        const known = cameraId && monitor.cameraIds.indexOf(cameraId) !== -1;

        return (
          <Box key={slotIndex} sx={{ position: 'relative', minWidth: 0, minHeight: 0, display: 'flex', overflow: 'hidden' }}>
            {known ? (
              <SquareCameraCard cameraId={cameraId!} fallbackName={cameraNamesById.get(cameraId!) ?? cameraId!} />
            ) : (
              <Box
                sx={{
                  flex: 1,
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  border: '1px dashed',
                  borderColor: alpha(W, 0.2),
                  borderRadius: 1,
                }}
              >
                <Typography variant="body2" sx={{ color: alpha(W, 0.35) }}>
                  Empty slot
                </Typography>
              </Box>
            )}
          </Box>
        );
      })}
    </Box>
  );
}
