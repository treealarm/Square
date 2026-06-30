import { useEffect, useState, type ReactNode } from 'react';
import { Box, CircularProgress, Typography } from '@mui/material';

const POLL_INTERVAL_MS = 1000;

// Right after the dev stack starts, Vite already serves the SPA before the .NET backend has
// finished booting and started listening — hitting login (or AuthGuard's validateToken) in that
// window fails outright, which looks like a real login error. Poll a dependency-free anonymous
// endpoint until the backend actually answers, and hold rendering everything else (including
// AuthGuard) until then.
export function BackendReadyGate({ children }: { children: ReactNode }) {
  const [ready, setReady] = useState(false);

  useEffect(() => {
    let cancelled = false;
    let timer: ReturnType<typeof setTimeout> | null = null;

    const check = async () => {
      try {
        const res = await fetch('/api/Auth/Ping', { cache: 'no-store' });
        if (!cancelled && res.ok) {
          setReady(true);
          return;
        }
      } catch {
        // backend not reachable yet — keep polling
      }
      if (!cancelled) {
        timer = setTimeout(check, POLL_INTERVAL_MS);
      }
    };

    check();

    return () => {
      cancelled = true;
      if (timer) clearTimeout(timer);
    };
  }, []);

  if (!ready) {
    return (
      <Box
        minHeight="100vh"
        display="flex"
        flexDirection="column"
        alignItems="center"
        justifyContent="center"
        gap={2}
        bgcolor="background.default"
      >
        <CircularProgress />
        <Typography color="text.secondary">Connecting to server...</Typography>
      </Box>
    );
  }

  return <>{children}</>;
}
