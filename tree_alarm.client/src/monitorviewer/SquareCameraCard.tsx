import { useEffect, useRef, useState } from 'react';
import { alpha, Box, IconButton, Stack, Tooltip, Typography } from '@mui/material';
import LiveTvIcon from '@mui/icons-material/LiveTv';
import VideoLibraryIcon from '@mui/icons-material/VideoLibrary';
import ControlCameraIcon from '@mui/icons-material/ControlCamera';
import SecurityIcon from '@mui/icons-material/Security';
import FiberManualRecordIcon from '@mui/icons-material/FiberManualRecord';
import CameraLivePlayer from './CameraLivePlayer';
import CameraArchivePlayer from './CameraArchivePlayer';
import PtzOverlay from './PtzOverlay';
import StreamProfileRow from './StreamProfileRow';
import StreamOverrideRow from './StreamOverrideRow';
import { vmsRecFetch } from './vmsRecFetch';
import { BTN_SX, NAME_LABEL_SX, STRIP_BG, STRIP_W, W } from './monitorStyle';

// Square-native equivalent of vms_rec's web_vms.client/src/components/CameraUnifiedCard.tsx — same
// icon-strip controls (mode toggle, PTZ, arm/disarm, continuous-recording) and the same two-poll
// pattern vms_rec's own card uses (GET /api/live/cameras/{id}/state for stream/profile data, GET
// /api/cameras/{id}/state for armed — confirmed two separate endpoints, see liveSlice.ts's
// fetchCameraState thunk), just with vmsRecFetch instead of same-origin fetch and local useState
// instead of Redux (monitorUiSlice/liveSlice). RTSP-copy and multi-stream cycling are intentionally
// not ported — not part of what was asked for, easy to add later if needed.

// Well-known built-in recording-profile preset ids — stable, see vms_rec's dto/dtos.tsx:106-108
// (VmsCfg migration 0001_init.sql). Duplicated here rather than shared across repos.
const RECORDING_PROFILE_CONTINUOUS_ID = '00000000-0000-0000-0000-000000000001';
const RECORDING_PROFILE_PREALARM_ID = '00000000-0000-0000-0000-000000000002';
const RECORDING_PROFILE_OFF_ID = '00000000-0000-0000-0000-000000000003';

interface StreamDetail {
  streamName: string;
  webRtc: boolean;
  profileToken?: string;
  disabled?: boolean;
  recordingProfile?: { id: string; name: string; mode: 'off' | 'continuous' | 'prealarm' };
  overrideProfile?: { id: string; name: string; mode: 'off' | 'continuous' | 'prealarm' };
}

interface CameraDetail {
  cameraId: string;
  name: string;
  disabled?: boolean;
  onvifConn?: unknown;
  onvifManaged?: boolean;
  activeRetentionWindow?: boolean;
  mediaMtxWebrtcHost?: string;
  mediaMtxWebrtcPort?: number;
  streams?: StreamDetail[];
}

type Props = {
  cameraId: string;
  fallbackName: string;
};

export default function SquareCameraCard({ cameraId, fallbackName }: Props) {
  const [mode, setMode] = useState<'live' | 'archive'>('live');
  const [ptzOpen, setPtzOpen] = useState(false);
  const [detail, setDetail] = useState<CameraDetail | null>(null);
  const [armed, setArmedState] = useState(false);
  const [hasPtz, setHasPtz] = useState<boolean | null>(null);
  const [pendingArmed, setPendingArmed] = useState<boolean | null>(null);
  const [pendingContinuous, setPendingContinuous] = useState(false);
  const reloadRef = useRef<() => void>(() => {});

  // Mirrors CameraUnifiedCard.tsx's own 5s polling — GET /api/live/cameras/{id}/state also
  // triggers EnsureWebRtcStreamingAsync server-side (see CameraLivePlayer.tsx, which polls the
  // same endpoint independently for that side effect) and GET /api/cameras/{id}/state for armed.
  useEffect(() => {
    let cancelled = false;
    const load = async () => {
      try {
        const [camRes, armedRes] = await Promise.all([
          vmsRecFetch(`/api/live/cameras/${encodeURIComponent(cameraId)}/state`),
          vmsRecFetch(`/api/cameras/${encodeURIComponent(cameraId)}/state`),
        ]);
        if (cancelled) return;
        if (camRes.ok) setDetail(await camRes.json());
        if (armedRes.ok) setArmedState((await armedRes.json()).armed ?? false);
      } catch {
        // best-effort poll, keep showing last known state on transient failure
      }
    };
    reloadRef.current = load;
    void load();
    const interval = window.setInterval(load, 5000);
    return () => {
      cancelled = true;
      window.clearInterval(interval);
    };
  }, [cameraId]);

  useEffect(() => {
    if (!detail?.onvifConn) { setHasPtz(null); return; }
    let cancelled = false;
    vmsRecFetch(`/api/onvif/cameras/${encodeURIComponent(cameraId)}/capabilities`)
      .then((r) => (r.ok ? r.json() : Promise.reject()))
      .then((d: { hasPtz: boolean }) => { if (!cancelled) setHasPtz(d.hasPtz); })
      .catch(() => { if (!cancelled) setHasPtz(null); });
    return () => { cancelled = true; };
  }, [cameraId, Boolean(detail?.onvifConn)]);

  const name = detail?.name || fallbackName;
  const isDisabled = detail?.disabled === true;
  const isOnvifManaged = detail?.onvifManaged ?? false;
  const effectiveArmed = pendingArmed !== null ? pendingArmed : armed;
  const webRtcStreams = (detail?.streams ?? []).filter((s) => s.webRtc);
  const activeStreamObj = webRtcStreams[0] ?? (detail?.streams ?? [])[0] ?? null;
  const isStreamRecording = (s: StreamDetail) => ((s.overrideProfile ?? s.recordingProfile)?.mode ?? 'off') !== 'off';
  const activeStreamContinuous = activeStreamObj?.recordingProfile?.mode === 'continuous';
  const showPtzControl = Boolean(detail?.onvifConn) && mode === 'live' && !isDisabled && (hasPtz ?? false);

  const primaryStream = webRtcStreams[0];
  const primaryStreamName = primaryStream ? `${cameraId}/${primaryStream.streamName}` : undefined;
  // MediaMTX's WebRTC listener has no TLS configured in this setup, same as MonitorViewer.tsx.
  const liveBaseUrl = detail?.mediaMtxWebrtcHost
    ? `http://${detail.mediaMtxWebrtcHost}:${detail.mediaMtxWebrtcPort ?? 8889}`
    : null;

  const armDisarm = async (enabled: boolean) => {
    if (isDisabled) return;
    setPendingArmed(enabled);
    try {
      await vmsRecFetch(`/api/cameras/${encodeURIComponent(cameraId)}/state`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ armed: enabled }),
      });
      // Mirrors CameraUnifiedCard.tsx's armDisarm cascade: arming ensures non-disabled streams
      // are at least on the prealarm profile; disarming reverts a prealarm-only stream to off.
      for (const s of detail?.streams ?? []) {
        if (s.disabled) continue;
        if (enabled) {
          if (!isStreamRecording(s)) {
            await vmsRecFetch('/api/recording-profiles/stream-profile', {
              method: 'POST',
              headers: { 'Content-Type': 'application/json' },
              body: JSON.stringify({ cameraId, streamName: s.streamName, profileId: RECORDING_PROFILE_PREALARM_ID }),
            });
          }
        } else if (s.recordingProfile?.id === RECORDING_PROFILE_PREALARM_ID) {
          await vmsRecFetch('/api/recording-profiles/stream-profile', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ cameraId, streamName: s.streamName, profileId: RECORDING_PROFILE_OFF_ID }),
          });
        }
      }
    } finally {
      setPendingArmed(null);
      reloadRef.current();
    }
  };

  const toggleContinuousRecording = async () => {
    if (!activeStreamObj || isDisabled) return;
    const profileId = activeStreamContinuous
      ? (effectiveArmed ? RECORDING_PROFILE_PREALARM_ID : RECORDING_PROFILE_OFF_ID)
      : RECORDING_PROFILE_CONTINUOUS_ID;
    setPendingContinuous(true);
    try {
      await vmsRecFetch('/api/recording-profiles/stream-profile', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ cameraId, streamName: activeStreamObj.streamName, profileId }),
      });
    } finally {
      setPendingContinuous(false);
      reloadRef.current();
    }
  };

  return (
    <Box sx={{ height: '100%', width: '100%', overflow: 'hidden', display: 'flex', flexDirection: 'row', borderRadius: 1 }}>
      {/* ── Left: icon strip — mirrors CameraUnifiedCard.tsx's compact strip ── */}
      <Stack alignItems="center" spacing={0.25} sx={{ ...STRIP_BG, width: STRIP_W, flexShrink: 0, py: 0.5, zIndex: 1 }}>
        <Tooltip title={mode === 'live' ? 'Switch to archive' : 'Switch to live'} placement="right">
          <IconButton size="small" onClick={() => setMode(mode === 'live' ? 'archive' : 'live')} sx={BTN_SX}>
            {mode === 'live' ? <LiveTvIcon sx={{ fontSize: '0.95rem' }} /> : <VideoLibraryIcon sx={{ fontSize: '0.95rem' }} />}
          </IconButton>
        </Tooltip>

        {showPtzControl && (
          <Tooltip title={ptzOpen ? 'Close PTZ' : 'Open PTZ'} placement="right">
            <IconButton
              size="small"
              onClick={() => setPtzOpen(!ptzOpen)}
              sx={{ ...BTN_SX, ...(ptzOpen ? { color: W, bgcolor: alpha(W, 0.2) } : {}) }}
            >
              <ControlCameraIcon sx={{ fontSize: '0.95rem' }} />
            </IconButton>
          </Tooltip>
        )}

        {isOnvifManaged && (
          <Tooltip title={isDisabled ? 'Camera disabled' : (effectiveArmed ? 'Armed' : 'Disarmed')} placement="right">
            <span>
              <IconButton
                size="small"
                disabled={pendingArmed !== null || isDisabled}
                onClick={() => void armDisarm(!effectiveArmed)}
                sx={{
                  ...BTN_SX,
                  ...(effectiveArmed && !isDisabled ? { color: '#ff9800' } : {}),
                  ...((pendingArmed !== null || isDisabled) ? { opacity: 0.35 } : {}),
                }}
              >
                <SecurityIcon sx={{ fontSize: '0.95rem' }} />
              </IconButton>
            </span>
          </Tooltip>
        )}

        {activeStreamObj && (
          <Tooltip title={activeStreamContinuous ? 'Continuous recording on' : 'Continuous recording off'} placement="right">
            <span>
              <IconButton
                size="small"
                disabled={pendingContinuous || isDisabled}
                onClick={() => void toggleContinuousRecording()}
                sx={{
                  ...BTN_SX,
                  ...(activeStreamContinuous ? { color: 'error.main' } : {}),
                  ...((pendingContinuous || isDisabled) ? { opacity: 0.35 } : {}),
                }}
              >
                <FiberManualRecordIcon sx={{ fontSize: '0.95rem' }} />
              </IconButton>
            </span>
          </Tooltip>
        )}
      </Stack>

      {/* ── Right: video / archive ── */}
      <Box sx={{ flex: 1, minWidth: 0, minHeight: 0, display: 'flex', overflow: 'hidden', position: 'relative', bgcolor: '#000' }}>
        <Typography sx={NAME_LABEL_SX}>{name}</Typography>
        {mode === 'live' && liveBaseUrl && primaryStreamName && (
          <CameraLivePlayer cameraId={cameraId} baseUrl={liveBaseUrl} streamName={primaryStreamName} />
        )}
        {mode === 'live' && (!liveBaseUrl || !primaryStreamName) && (
          <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center', width: '100%', height: '100%' }}>
            <Typography sx={{ fontSize: '0.78rem', color: alpha(W, 0.35) }}>No live stream info yet.</Typography>
          </Box>
        )}
        {mode === 'archive' && <CameraArchivePlayer vmsCameraId={cameraId} />}

        {showPtzControl && ptzOpen && (
          <PtzOverlay
            cameraId={cameraId}
            profileToken={activeStreamObj?.profileToken ?? ''}
            onClose={() => setPtzOpen(false)}
          />
        )}

        {mode === 'live' && activeStreamObj && (
          <Stack
            direction="row"
            spacing={0.5}
            alignItems="center"
            sx={{ position: 'absolute', bottom: 4, right: 4, zIndex: 2, bgcolor: alpha('#000', 0.45), borderRadius: 0.5, px: 0.5, py: 0.25 }}
          >
            <StreamProfileRow cameraId={cameraId} stream={activeStreamObj} onChanged={() => reloadRef.current()} />
            <StreamOverrideRow cameraId={cameraId} stream={activeStreamObj} onChanged={() => reloadRef.current()} />
          </Stack>
        )}
      </Box>
    </Box>
  );
}
