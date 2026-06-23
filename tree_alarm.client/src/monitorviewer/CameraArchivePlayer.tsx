import { useEffect, useMemo, useRef, useState } from 'react';
import { Alert, Box, IconButton, Stack, Tooltip, Typography } from '@mui/material';
import NavigateBeforeIcon from '@mui/icons-material/NavigateBefore';
import NavigateNextIcon from '@mui/icons-material/NavigateNext';
import Hls from 'hls.js';
import { VMS_REC_BASE_URL, vmsRecFetch, vmsRecXhrSetup } from './vmsRecFetch';

// Trimmed port of vms_rec's web_vms.client/src/components/ArchiveHlsStreamCard.tsx — same hls.js
// playback mechanics, but talks to vms_rec's API on a different origin (absolute URLs + bearer
// token via vmsRecFetch/vmsRecXhrSetup instead of vms_rec's own same-origin authFetch), and drops
// the full RecordingsList side panel for a simple prev/next range stepper (single-camera view,
// not vms_rec's multi-camera Redux-backed UI).

type RecordingDto = {
  filePath: string;
  streamName?: string;
  startTime: string;
  clipUid: string;
};
type RecordingRangeDto = { recordings: RecordingDto[] };

interface CameraArchivePlayerProps {
  vmsCameraId: string;
}

export default function CameraArchivePlayer({ vmsCameraId }: CameraArchivePlayerProps) {
  const videoRef = useRef<HTMLVideoElement>(null);
  const [hlsError, setHlsError] = useState<string | null>(null);
  const [ranges, setRanges] = useState<RecordingRangeDto[]>([]);
  const [rangeIndex, setRangeIndex] = useState(0);

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
    <Stack sx={{ width: '100%', height: '100%' }}>
      <Stack direction="row" alignItems="center" spacing={1} sx={{ p: 0.5 }}>
        <Tooltip title="Newer">
          <IconButton size="small" disabled={rangeIndex <= 0} onClick={() => setRangeIndex((i) => i - 1)}>
            <NavigateBeforeIcon fontSize="inherit" />
          </IconButton>
        </Tooltip>
        <Typography variant="caption">
          {currentClipTime ? new Date(currentClipTime).toLocaleString() : 'No recordings'}
        </Typography>
        <Tooltip title="Older">
          <IconButton
            size="small"
            disabled={rangeIndex >= ranges.length - 1}
            onClick={() => setRangeIndex((i) => i + 1)}
          >
            <NavigateNextIcon fontSize="inherit" />
          </IconButton>
        </Tooltip>
      </Stack>
      {hlsError && <Alert severity="error">{hlsError}</Alert>}
      <Box sx={{ flex: 1, minHeight: 0 }}>
        <video ref={videoRef} controls style={{ width: '100%', height: '100%', objectFit: 'contain' }} />
      </Box>
    </Stack>
  );
}
