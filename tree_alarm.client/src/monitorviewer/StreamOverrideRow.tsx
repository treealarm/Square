import { useEffect, useState } from 'react';
import { CircularProgress, MenuItem, Select, Stack, Typography } from '@mui/material';
import { vmsRecFetch } from './vmsRecFetch';
import type { RecordingProfileDto } from './StreamProfileRow';

// Ported from vms_rec's web_vms.client/src/components/StreamOverrideRow.tsx — same mechanics, no
// Redux: direct vmsRecFetch POST/DELETE to /api/recording-profiles/stream-override.

type StreamLike = {
  streamName: string;
  overrideProfile?: { id: string };
};

type Props = {
  cameraId: string;
  stream: StreamLike | null;
  onChanged: () => void;
};

export default function StreamOverrideRow({ cameraId, stream, onChanged }: Props) {
  const [presets, setPresets] = useState<RecordingProfileDto[]>([]);
  const [busy, setBusy] = useState(false);
  const [pendingValue, setPendingValue] = useState<string | null>(null);

  useEffect(() => {
    vmsRecFetch('/api/recording-profiles')
      .then((r) => (r.ok ? (r.json() as Promise<RecordingProfileDto[]>) : []))
      .then((all) => setPresets(all.filter((p) => p.isOverridePreset)))
      .catch(() => {});
  }, []);

  if (!stream) return null;

  const isOverride = !!stream.overrideProfile;
  const overrideValue = pendingValue !== null ? pendingValue : (stream.overrideProfile?.id ?? '');

  const handleChange = async (profileId: string) => {
    setPendingValue(profileId);
    setBusy(true);
    try {
      if (profileId === '') {
        const q = new URLSearchParams({ cameraId, streamName: stream.streamName });
        await vmsRecFetch(`/api/recording-profiles/stream-override?${q.toString()}`, { method: 'DELETE' });
      } else {
        await vmsRecFetch('/api/recording-profiles/stream-override', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ cameraId, streamName: stream.streamName, profileId }),
        });
      }
      onChanged();
    } finally {
      setBusy(false);
      setPendingValue(null);
    }
  };

  return (
    <Stack direction="row" alignItems="center" spacing={1} sx={{ flexShrink: 0, px: 0.25 }}>
      {isOverride && (
        <Typography variant="caption" sx={{ color: 'primary.main', fontSize: '0.68rem', flexShrink: 0 }}>
          Override
        </Typography>
      )}

      <Select
        size="small"
        displayEmpty
        value={overrideValue}
        onChange={(e) => void handleChange(e.target.value)}
        disabled={busy || presets.length === 0}
        sx={{ flex: 1, fontSize: '0.72rem', height: 22, minWidth: 96, '.MuiSelect-select': { py: 0, px: 0.75 } }}
        renderValue={(v) =>
          v === '' ? (
            <Typography variant="caption" sx={{ color: 'text.secondary' }}>No override</Typography>
          ) : (
            <Typography variant="caption">{presets.find((p) => p.id === v)?.name ?? v}</Typography>
          )
        }
      >
        {isOverride && (
          <MenuItem value="" sx={{ fontSize: '0.72rem' }}>
            <em>Clear override</em>
          </MenuItem>
        )}
        {presets.map((p) => (
          <MenuItem key={p.id} value={p.id} sx={{ fontSize: '0.72rem' }}>
            {p.name}
          </MenuItem>
        ))}
      </Select>

      {busy && <CircularProgress size={14} />}
    </Stack>
  );
}
