import { useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Alert, Box, IconButton, List, ListItemButton, ListItemText, Stack, ToggleButton, ToggleButtonGroup, Tooltip, Typography } from '@mui/material';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import { DoFetch } from '../store/Fetcher';
import { ApiIntegroRootString } from '../store/constants';
import { requestMarkersByIds } from '../store/MarkersStates';
import { ICommonFig, IIntegroDTO } from '../store/Marker';
import CameraLivePlayer from './CameraLivePlayer';
import CameraArchivePlayer from './CameraArchivePlayer';
import { VMS_REC_BASE_URL } from './vmsRecFetch';

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
  webrtcHost?: string;
  webrtcPort?: string;
  primaryStreamName?: string;
}

function extraProp(fig: ICommonFig, name: string): string | undefined {
  return fig.extra_props?.find((p) => p.prop_name === name)?.str_val || undefined;
}

export function MonitorViewer() {
  const navigate = useNavigate();
  const [cameras, setCameras] = useState<CameraEntry[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [mode, setMode] = useState<'live' | 'archive'>('live');

  useEffect(() => {
    let cancelled = false;

    async function load() {
      try {
        const res = await DoFetch(
          `${ApiIntegroRootString}/GetListByType?i_name=${encodeURIComponent(VMS_APP_ID)}&i_type=camera`
        );
        if (!res.ok) throw new Error(res.statusText);
        const integros: IIntegroDTO[] = await res.json();
        const ids = integros.map((i) => i.id).filter((id): id is string => !!id);
        if (ids.length === 0) {
          if (!cancelled) setCameras([]);
          return;
        }

        const figures = await requestMarkersByIds(ids);
        const entries: CameraEntry[] = (figures.figs ?? [])
          .map((fig) => ({
            id: fig.id ?? '',
            name: fig.name,
            webrtcHost: extraProp(fig, 'webrtc_host'),
            webrtcPort: extraProp(fig, 'webrtc_port'),
            primaryStreamName: extraProp(fig, 'primary_stream_name'),
          }))
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

  const selected = useMemo(() => cameras.find((c) => c.id === selectedId) ?? null, [cameras, selectedId]);
  // MediaMTX's WebRTC listener has no TLS configured in this setup (matches vms_rec's own
  // frontend, which builds this URL from window.location.protocol — for Square there's no
  // equivalent "current page protocol" to borrow, so plain http is the correct default here).
  const liveBaseUrl = selected?.webrtcHost
    ? `http://${selected.webrtcHost}:${selected.webrtcPort ?? 8889}`
    : null;

  return (
    <Box sx={{ display: 'flex', height: '100vh', width: '100vw' }}>
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

      <Box sx={{ flex: 1, display: 'flex', flexDirection: 'column', minWidth: 0 }}>
        {selected ? (
          <>
            <Stack direction="row" justifyContent="center" sx={{ p: 1 }}>
              <ToggleButtonGroup size="small" value={mode} exclusive onChange={(_, v) => v && setMode(v)}>
                <ToggleButton value="live">Live</ToggleButton>
                <ToggleButton value="archive">Archive</ToggleButton>
              </ToggleButtonGroup>
            </Stack>
            <Box sx={{ flex: 1, minHeight: 0 }}>
              {mode === 'live' && liveBaseUrl && selected.primaryStreamName && (
                <CameraLivePlayer cameraId={selected.id} baseUrl={liveBaseUrl} streamName={selected.primaryStreamName} />
              )}
              {mode === 'live' && (!liveBaseUrl || !selected.primaryStreamName) && (
                <Alert severity="info" sx={{ m: 2 }}>
                  No live stream info pushed for this camera yet.
                </Alert>
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
          </>
        ) : (
          <Typography variant="body2" sx={{ p: 2 }} color="text.secondary">
            Select a camera from the list.
          </Typography>
        )}
      </Box>
    </Box>
  );
}
