import { useEffect, useMemo, useRef, useState } from 'react';
import { alpha, Alert, Box, Divider, IconButton, List, ListItemButton, ListItemText, ListSubheader, Stack, Tooltip, Typography } from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import MenuIcon from '@mui/icons-material/Menu';
import Hls from 'hls.js';
import { VMS_REC_BASE_URL, vmsRecFetch, vmsRecXhrSetup } from './vmsRecFetch';
import { W } from './monitorStyle';

// Trimmed port of vms_rec's web_vms.client/src/components/ArchiveHlsStreamCard.tsx +
// RecordingsList.tsx — same hls.js playback mechanics and the same dark collapsible clip-list
// panel look, but talks to vms_rec's API on a different origin (absolute URLs + bearer token via
// vmsRecFetch/vmsRecXhrSetup instead of vms_rec's own same-origin authFetch) and drops the
// calendar date-jump/stream-filter controls (single-camera view, no Redux store to back them).

type RecordingDto = {
  filePath: string;
  streamName?: string;
  startTime: string;
  clipUid: string;
};
type RecordingRangeDto = { recordings: RecordingDto[] };

const PANEL_BG = { bgcolor: '#434343', backdropFilter: 'blur(6px)' } as const;
const BTN = {
  p: '3px',
  color: alpha(W, 0.75),
  borderRadius: '4px',
  '&:hover': { color: W, bgcolor: alpha(W, 0.12) },
} as const;

function fmtTime(iso: string) {
  const d = new Date(iso);
  return Number.isNaN(d.getTime()) ? iso : d.toLocaleTimeString(undefined, { hour: '2-digit', minute: '2-digit', second: '2-digit', hour12: false });
}
function fmtDay(iso: string) {
  const d = new Date(iso);
  return Number.isNaN(d.getTime()) ? '' : d.toLocaleDateString('sv-SE');
}

interface CameraArchivePlayerProps {
  vmsCameraId: string;
}

export default function CameraArchivePlayer({ vmsCameraId }: CameraArchivePlayerProps) {
  const videoRef = useRef<HTMLVideoElement>(null);
  const [hlsError, setHlsError] = useState<string | null>(null);
  const [ranges, setRanges] = useState<RecordingRangeDto[]>([]);
  const [rangeIndex, setRangeIndex] = useState(0);
  const [listOpen, setListOpen] = useState(true);

  useEffect(() => {
    setRanges([]);
    setRangeIndex(0);
    if (!vmsCameraId) return;
    let cancelled = false;
    vmsRecFetch('/api/fs/recording-ranges', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ cameraId: vmsCameraId, limit: 50 }),
    })
      .then((res) => (res.ok ? res.json() : { ranges: [] }))
      .then((data: { ranges: RecordingRangeDto[] }) => {
        if (!cancelled) setRanges(data.ranges ?? []);
      })
      .catch(() => {
        if (!cancelled) setRanges([]);
      });
    return () => {
      cancelled = true;
    };
  }, [vmsCameraId]);

  const playlistUrl = useMemo(() => {
    const range = ranges[rangeIndex];
    const firstClip = range?.recordings[0];
    if (!vmsCameraId || !firstClip?.clipUid) return null;
    const q = new URLSearchParams({
      cameraId: vmsCameraId,
      startClipUid: firstClip.clipUid,
      count: String(range.recordings.length),
    });
    return `/api/arch/hls/from-files.m3u8?${q.toString()}`;
  }, [vmsCameraId, ranges, rangeIndex]);

  const currentClipTime = ranges[rangeIndex]?.recordings[0]?.startTime;
  const formattedTime = currentClipTime ? new Date(currentClipTime).toLocaleString() : null;

  useEffect(() => {
    const video = videoRef.current;
    setHlsError(null);
    if (!video || !playlistUrl) return;

    let hls: Hls | null = null;
    const tryPlay = () => void video.play().catch(() => {});

    if (Hls.isSupported()) {
      hls = new Hls({ enableWorker: true, lowLatencyMode: false, xhrSetup: vmsRecXhrSetup });
      video.addEventListener('canplay', tryPlay, { once: true });
      video.addEventListener('loadeddata', tryPlay, { once: true });
      hls.on(Hls.Events.MEDIA_ATTACHED, () => hls?.loadSource(`${VMS_REC_BASE_URL}${playlistUrl}`));
      hls.on(Hls.Events.MANIFEST_PARSED, () => tryPlay());
      hls.on(Hls.Events.ERROR, (_, data) => {
        if (data.fatal) {
          if (data.type === Hls.ErrorTypes.MEDIA_ERROR) {
            try { hls?.recoverMediaError(); } catch { /* ignore */ }
          }
          setHlsError(`${data.type}: ${data.details ?? ''}`);
        }
      });
      hls.attachMedia(video);
    } else {
      setHlsError('HLS not supported in this browser');
      return;
    }

    return () => {
      hls?.destroy();
    };
  }, [playlistUrl]);

  return (
    <Box sx={{ width: '100%', height: '100%', display: 'flex', overflow: 'hidden' }}>
      {/* ── Video area ── */}
      <Box sx={{ flex: 1, minWidth: 0, position: 'relative', bgcolor: '#000' }}>
        {hlsError && (
          <Alert
            severity="error"
            onClose={() => setHlsError(null)}
            sx={{ position: 'absolute', top: 4, left: 4, right: listOpen ? 'auto' : 32, zIndex: 2, fontSize: '0.72rem', py: 0.25 }}
          >
            {hlsError}
          </Alert>
        )}
        {!playlistUrl ? (
          <Box sx={{ width: '100%', height: '100%', display: 'flex', alignItems: 'center', justifyContent: 'center', color: alpha(W, 0.35), fontSize: '0.78rem' }}>
            No recordings
          </Box>
        ) : (
          <video ref={videoRef} controls playsInline muted style={{ width: '100%', height: '100%', objectFit: 'contain', display: 'block' }} />
        )}
      </Box>

      {/* ── Right: compact strip when collapsed ── */}
      {!listOpen && (
        <Stack alignItems="center" spacing={0.25} sx={{ ...PANEL_BG, width: 28, flexShrink: 0, py: 0.5 }}>
          <Box sx={{ flex: 1 }} />
          <Tooltip title={formattedTime ? `Archive · ${formattedTime}` : 'Open recordings'} placement="left">
            <IconButton size="small" onClick={() => setListOpen(true)} sx={BTN}>
              <MenuIcon sx={{ fontSize: '0.95rem' }} />
            </IconButton>
          </Tooltip>
        </Stack>
      )}

      {/* ── Right: clip list panel ── */}
      <Stack
        sx={{
          ...PANEL_BG,
          width: listOpen ? 170 : 0,
          flexShrink: 0,
          minHeight: 0,
          overflow: 'hidden',
          transition: 'width 0.15s ease',
          borderLeft: listOpen ? `1px solid ${alpha(W, 0.08)}` : 'none',
        }}
      >
        <Stack direction="row" alignItems="center" sx={{ px: 1, pt: 0.75, pb: 0.5, flexShrink: 0 }}>
          <Typography sx={{ flex: 1, color: W, fontWeight: 600, fontSize: '0.78rem' }}>Archive</Typography>
          {formattedTime && (
            <Tooltip title={formattedTime} placement="left">
              <Typography sx={{ fontSize: '0.6rem', color: alpha(W, 0.45), mr: 0.75, whiteSpace: 'nowrap' }}>
                {fmtDay(currentClipTime!)}
              </Typography>
            </Tooltip>
          )}
          <IconButton size="small" onClick={() => setListOpen(false)} sx={{ p: '2px', color: alpha(W, 0.45), '&:hover': { color: W } }}>
            <CloseIcon sx={{ fontSize: '0.9rem' }} />
          </IconButton>
        </Stack>

        <Divider sx={{ borderColor: alpha(W, 0.1), flexShrink: 0 }} />

        <Box sx={{ flex: 1, minHeight: 0, overflowY: 'auto' }}>
          {ranges.length === 0 ? (
            <Typography sx={{ px: 1, pt: 1.5, fontSize: '0.7rem', color: alpha(W, 0.45), textAlign: 'center' }}>
              No recordings
            </Typography>
          ) : (
            <List dense disablePadding>
              {ranges.map((range, index) => {
                const first = range.recordings[0];
                if (!first) return null;
                const prevFirst = ranges[index - 1]?.recordings[0];
                const showDate = index === 0 || fmtDay(first.startTime) !== fmtDay(prevFirst?.startTime ?? '');
                const isSelected = index === rangeIndex;

                return (
                  <div key={`${first.startTime}:${index}`}>
                    {showDate && (
                      <ListSubheader disableSticky sx={{ lineHeight: '22px', fontSize: '0.65rem', opacity: 0.6, bgcolor: 'transparent', color: alpha(W, 0.55) }}>
                        {fmtDay(first.startTime)}
                      </ListSubheader>
                    )}
                    <ListItemButton
                      selected={isSelected}
                      onClick={() => setRangeIndex(index)}
                      sx={{
                        py: 0.3, px: 0.75,
                        ...(isSelected ? { bgcolor: alpha(W, 0.12) } : {}),
                        '&:hover': { bgcolor: alpha(W, 0.08) },
                      }}
                    >
                      <ListItemText
                        primary={fmtTime(first.startTime)}
                        primaryTypographyProps={{
                          variant: 'body2',
                          noWrap: true,
                          sx: {
                            fontVariantNumeric: 'tabular-nums',
                            fontSize: '0.76rem', lineHeight: 1.25,
                            color: isSelected ? W : alpha(W, 0.8),
                          },
                        }}
                      />
                    </ListItemButton>
                  </div>
                );
              })}
            </List>
          )}
        </Box>
      </Stack>
    </Box>
  );
}
