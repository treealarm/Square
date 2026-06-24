import { useEffect, useMemo, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { Alert, alpha, Box, IconButton, List, ListItemButton, ListItemText, Stack, Tooltip, Typography } from '@mui/material';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import LiveTvIcon from '@mui/icons-material/LiveTv';
import VideoLibraryIcon from '@mui/icons-material/VideoLibrary';
import { DoFetch } from '../store/Fetcher';
import { ApiIntegroRootString } from '../store/constants';
import { requestMarkersByIds } from '../store/MarkersStates';
import { IIntegroDTO } from '../store/Marker';
import CameraLivePlayer from './CameraLivePlayer';
import CameraArchivePlayer from './CameraArchivePlayer';
import MonitorGridViewer, { type MonitorDto } from './MonitorGridViewer';
import { VMS_REC_BASE_URL, vmsRecFetch } from './vmsRecFetch';
import { BTN_SX, NAME_LABEL_SX, STRIP_BG, STRIP_W, W } from './monitorStyle';

// vms_rec registers itself with this Dapr app-id/i_name (see docs/square-integration-plan.md) —
// hardcoded here rather than configurable, since this tab is specifically vms_rec's Monitor, not
// a generic camera browser for arbitrary future producers.
const VMS_APP_ID = 'vmscfg';

interface CameraEntry {
  // Square's object id and vms_rec's camera_id are the same GUID (see
  // SquareIntegrationGrpcClient.ToObjectId in vms_rec) — no separate property needed to recover
  // vms_rec's id, the object's own id already is the camera_id.
  id: string;
  name: string;
}

// Minimal mirror of vms_rec's web_vms.client/src/dto/dtos.tsx CameraSetupDto/CameraStreamDto —
// only the fields the single-camera quick-view player needs. Fetched directly from vms_rec's own
// GET /api/live/cameras (already CORS+JWT enabled, see vmsRecFetch.ts). The full monitor grid
// (SquareCameraCard.tsx) does its own richer per-camera polling instead of using this bulk map.
interface VmsLiveCamera {
  cameraId: string;
  mediaMtxWebrtcHost?: string;
  mediaMtxWebrtcPort?: number;
  streams?: { streamName: string; webRtc: boolean }[];
}

export function MonitorViewer() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const [cameras, setCameras] = useState<CameraEntry[]>([]);
  const [monitors, setMonitors] = useState<MonitorDto[]>([]);
  const [liveCamerasById, setLiveCamerasById] = useState<Map<string, VmsLiveCamera>>(new Map());
  const [error, setError] = useState<string | null>(null);
  // Seeded from ?cameraId=<id> when arriving via "Open in Monitor" on a map pin (see
  // tree/ObjectProperties.tsx) — jumps straight to that one camera, no list browsing needed. Takes
  // priority over any selected monitor below (ad hoc single-camera view wins).
  const [selectedId, setSelectedId] = useState<string | null>(() => searchParams.get('cameraId'));
  const [selectedMonitorId, setSelectedMonitorId] = useState<string | null>(null);
  const [mode, setMode] = useState<'live' | 'archive'>('live');

  useEffect(() => {
    let cancelled = false;

    async function load() {
      try {
        const [integroRes, monitorRes, liveRes] = await Promise.all([
          DoFetch(`${ApiIntegroRootString}/GetListByType?i_name=${encodeURIComponent(VMS_APP_ID)}&i_type=camera`),
          vmsRecFetch('/api/monitor').catch(() => null),
          vmsRecFetch('/api/live/cameras').catch(() => null),
        ]);

        if (monitorRes?.ok) {
          const monitorList: MonitorDto[] = await monitorRes.json();
          if (!cancelled) setMonitors(monitorList);
        }

        if (liveRes?.ok) {
          const liveCameras: VmsLiveCamera[] = await liveRes.json();
          if (!cancelled) setLiveCamerasById(new Map(liveCameras.map((c) => [c.cameraId, c])));
        }

        if (!integroRes.ok) throw new Error(integroRes.statusText);
        const integros: IIntegroDTO[] = await integroRes.json();
        const ids = integros.map((i) => i.id).filter((id): id is string => !!id);
        if (ids.length === 0) {
          if (!cancelled) setCameras([]);
          return;
        }

        const figures = await requestMarkersByIds(ids);
        const entries: CameraEntry[] = (figures.figs ?? [])
          .map((fig) => ({ id: fig.id ?? '', name: fig.name }))
          .filter((entry) => entry.id);

        if (!cancelled) {
          setCameras(entries);
          setError(null);
        }
      } catch (err: unknown) {
        if (!cancelled) setError(err instanceof Error ? err.message : 'Failed to load cameras');
      }
    }

    void load();
    return () => {
      cancelled = true;
    };
  }, []);

  const cameraNamesById = useMemo(() => new Map(cameras.map((c) => [c.id, c.name])), [cameras]);
  const selectedMonitor = useMemo(
    () => (selectedId ? null : monitors.find((m) => m.monitorId === selectedMonitorId) ?? null),
    [monitors, selectedMonitorId, selectedId]
  );

  const selected = useMemo(() => cameras.find((c) => c.id === selectedId) ?? null, [cameras, selectedId]);
  const selectedLive = selected ? liveCamerasById.get(selected.id) : undefined;
  const primaryStream = selectedLive?.streams?.find((s) => s.webRtc);
  const primaryStreamName = primaryStream ? `${selected?.id}/${primaryStream.streamName}` : undefined;
  // MediaMTX's WebRTC listener has no TLS configured in this setup (matches vms_rec's own
  // frontend, which builds this URL from window.location.protocol — for Square there's no
  // equivalent "current page protocol" to borrow, so plain http is the correct default here).
  const liveBaseUrl = selectedLive?.mediaMtxWebrtcHost
    ? `http://${selectedLive.mediaMtxWebrtcHost}:${selectedLive.mediaMtxWebrtcPort ?? 8889}`
    : null;

  return (
    <Box sx={{ display: 'flex', height: '98vh', width: '100%' }}>
      <Box sx={{ width: 280, borderRight: 1, borderColor: 'divider', overflow: 'auto' }}>
        <Stack direction="row" alignItems="center" spacing={1} sx={{ p: 1 }}>
          <Tooltip title="Back to map">
            <IconButton size="small" onClick={() => navigate('/')}>
              <ArrowBackIcon fontSize="inherit" />
            </IconButton>
          </Tooltip>
          <Typography variant="subtitle1">Monitor</Typography>
        </Stack>
        {error && <Alert severity="error">{error}</Alert>}

        <Typography variant="caption" sx={{ px: 2, color: 'text.secondary' }}>
          Monitors
        </Typography>
        {monitors.length === 0 && (
          <Typography variant="body2" sx={{ p: 2 }} color="text.secondary">
            No monitors from vms_rec yet.
          </Typography>
        )}
        <List dense>
          {monitors.map((mon) => (
            <ListItemButton
              key={mon.monitorId}
              selected={!selectedId && mon.monitorId === selectedMonitorId}
              onClick={() => { setSelectedId(null); setSelectedMonitorId(mon.monitorId); }}
            >
              <ListItemText primary={mon.name} secondary={`${mon.layout} · ${mon.cameraIds.length} cam`} />
            </ListItemButton>
          ))}
        </List>

        <Typography variant="caption" sx={{ px: 2, color: 'text.secondary' }}>
          Cameras (quick view)
        </Typography>
        {!error && cameras.length === 0 && (
          <Typography variant="body2" sx={{ p: 2 }} color="text.secondary">
            No cameras from vms_rec yet.
          </Typography>
        )}
        <List dense>
          {cameras.map((cam) => (
            <ListItemButton
              key={cam.id}
              selected={cam.id === selectedId}
              onClick={() => setSelectedId(cam.id)}
            >
              <ListItemText primary={cam.name} secondary={cam.id} />
            </ListItemButton>
          ))}
        </List>
      </Box>

      <Box sx={{ flex: 1, display: 'flex', minWidth: 0, p: 1 }}>
        {selected ? (
          <Box
            sx={{
              flex: 1,
              minWidth: 0,
              display: 'flex',
              flexDirection: 'row',
              overflow: 'hidden',
              borderRadius: 1,
            }}
          >
            {/* ── Left: icon strip — mirrors CameraUnifiedCard.tsx's compact strip ── */}
            <Stack
              alignItems="center"
              spacing={0.25}
              sx={{ ...STRIP_BG, width: STRIP_W, flexShrink: 0, py: 0.5, zIndex: 1 }}
            >
              <Tooltip title={mode === 'live' ? 'Switch to archive' : 'Switch to live'} placement="right">
                <IconButton size="small" onClick={() => setMode(mode === 'live' ? 'archive' : 'live')} sx={BTN_SX}>
                  {mode === 'live'
                    ? <LiveTvIcon sx={{ fontSize: '0.95rem' }} />
                    : <VideoLibraryIcon sx={{ fontSize: '0.95rem' }} />}
                </IconButton>
              </Tooltip>
            </Stack>

            {/* ── Right: video / archive — fills remaining space ── */}
            <Box sx={{ flex: 1, minWidth: 0, minHeight: 0, display: 'flex', overflow: 'hidden', position: 'relative', bgcolor: '#000' }}>
              <Typography sx={NAME_LABEL_SX}>{selected.name}</Typography>
              {mode === 'live' && liveBaseUrl && primaryStreamName && (
                <CameraLivePlayer cameraId={selected.id} baseUrl={liveBaseUrl} streamName={primaryStreamName} />
              )}
              {mode === 'live' && (!liveBaseUrl || !primaryStreamName) && (
                <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center', width: '100%', height: '100%' }}>
                  <Typography sx={{ fontSize: '0.78rem', color: alpha(W, 0.35) }}>
                    No live stream info available for this camera from vms_rec yet.
                  </Typography>
                </Box>
              )}
              {mode === 'archive' && !VMS_REC_BASE_URL && (
                <Alert severity="warning" sx={{ m: 2 }}>
                  VITE_VMS_REC_BASE_URL is not configured — archive playback is unavailable.
                </Alert>
              )}
              {mode === 'archive' && VMS_REC_BASE_URL && (
                <CameraArchivePlayer vmsCameraId={selected.id} />
              )}
            </Box>
          </Box>
        ) : selectedMonitor ? (
          <MonitorGridViewer monitor={selectedMonitor} cameraNamesById={cameraNamesById} />
        ) : (
          <Typography variant="body2" sx={{ p: 2 }} color="text.secondary">
            Select a monitor or a camera from the list.
          </Typography>
        )}
      </Box>
    </Box>
  );
}
