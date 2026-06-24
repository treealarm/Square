import { useEffect, useState } from 'react';
import { CircularProgress, MenuItem, Select } from '@mui/material';
import { vmsRecFetch } from './vmsRecFetch';

// Ported from vms_rec's web_vms.client/src/components/StreamProfileRow.tsx — same fetch/select
// mechanics, but no Redux: GET /api/recording-profiles and the POST mutation both go through
// vmsRecFetch directly, and onChanged() lets the parent re-poll the camera's state instead of a
// dispatch(fetchCameraState(...)) thunk.

export type RecordingProfileDto = {
  id: string;
  name: string;
  mode: 'off' | 'continuous' | 'prealarm';
  isOverridePreset: boolean;
};

type StreamLike = {
  streamName: string;
  recordingProfile?: { id: string };
};

type Props = {
  cameraId: string;
  stream: StreamLike | null;
  onChanged: () => void;
};

export default function StreamProfileRow({ cameraId, stream, onChanged }: Props) {
  const [profiles, setProfiles] = useState<RecordingProfileDto[]>([]);
  const [busy, setBusy] = useState(false);
  const [pendingValue, setPendingValue] = useState<string | null>(null);

  useEffect(() => {
    vmsRecFetch('/api/recording-profiles')
      .then((r) => (r.ok ? (r.json() as Promise<RecordingProfileDto[]>) : []))
      .then(setProfiles)
      .catch(() => {});
  }, []);

  if (!stream) return null;

  const currentValue = pendingValue !== null ? pendingValue : (stream.recordingProfile?.id ?? '');

  const handleChange = async (profileId: string) => {
    if (!profileId) return;
    setPendingValue(profileId);
    setBusy(true);
    try {
      await vmsRecFetch('/api/recording-profiles/stream-profile', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ cameraId, streamName: stream.streamName, profileId }),
      });
      onChanged();
    } finally {
      setBusy(false);
      setPendingValue(null);
    }
  };

  return (
    <>
      <Select
        size="small"
        displayEmpty
        value={currentValue}
        onChange={(e) => void handleChange(e.target.value)}
        disabled={busy || profiles.length === 0}
        sx={{ flex: 1, fontSize: '0.72rem', height: 22, minWidth: 96, '.MuiSelect-select': { py: 0, px: 0.75 } }}
        renderValue={(v) =>
          profiles.find((p) => p.id === v)?.name ?? (
            <span style={{ color: 'inherit', opacity: 0.5 }}>—</span>
          )
        }
      >
        {profiles.map((p) => (
          <MenuItem key={p.id} value={p.id} sx={{ fontSize: '0.72rem' }}>
            {p.name}
          </MenuItem>
        ))}
      </Select>
      {busy && <CircularProgress size={14} />}
    </>
  );
}
