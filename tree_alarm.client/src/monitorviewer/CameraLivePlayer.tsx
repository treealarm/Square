import { Box } from '@mui/material';
import { useEffect, useRef } from 'react';
import { vmsRecFetch } from './vmsRecFetch';

// Ported from vms_rec's web_vms.client/src/components/CameraStream.tsx — same WHEP/WebRTC
// mechanics and the same plain video element (no on-screen debug overlay; the WHEP URL goes to
// the console only, exactly like the original). MediaMTX's WHEP endpoint sends permissive CORS
// headers by default (the protocol is designed to be embeddable from any origin), so no vms_rec
// backend change is needed for live video specifically — only the archive/recording-ranges API
// needs CORS (see vmsRecFetch.ts and Program.cs's SQUARE_FRONTEND_ORIGIN gate).
interface CameraLivePlayerProps {
  cameraId: string;
  streamName: string;
  baseUrl: string;
}

export default function CameraLivePlayer({ cameraId, streamName, baseUrl }: CameraLivePlayerProps) {
  const videoRef = useRef<HTMLVideoElement>(null);
  const pcRef = useRef<RTCPeerConnection | null>(null);
  const retryTimeoutRef = useRef<number | null>(null);

  // GET /api/live/cameras/{cameraId}/state has a side effect on vms_rec's backend: it calls
  // EnsureWebRtcStreamingAsync on media_server for every webrtc-enabled stream, which is what
  // actually makes MediaMTX publish a track to negotiate over WHEP. vms_rec's own UI polls this
  // every 5s while a camera card is mounted (CameraUnifiedCard.tsx) — without it, MediaMTX has
  // nothing to send and the WHEP connection below just sits with no video.
  useEffect(() => {
    if (!cameraId) return;
    let stopped = false;
    const poll = () => {
      if (stopped) return;
      void vmsRecFetch(`/api/live/cameras/${cameraId}/state`).catch(() => {});
    };
    poll();
    const interval = window.setInterval(poll, 5000);
    return () => {
      stopped = true;
      window.clearInterval(interval);
    };
  }, [cameraId]);

  useEffect(() => {
    if (!streamName || !baseUrl) {
      return;
    }

    let stopped = false;
    let sessionId = 0;
    let reconnecting = false;

    const cleanupAndRetry = (pc?: RTCPeerConnection) => {
      if (stopped || reconnecting) {
        return;
      }
      reconnecting = true;

      if (pc) {
        pc.onconnectionstatechange = null;
        pc.ontrack = null;
        pc.close();
      }
      if (pcRef.current) {
        pcRef.current.onconnectionstatechange = null;
        pcRef.current.ontrack = null;
        pcRef.current.close();
        pcRef.current = null;
      }
      if (retryTimeoutRef.current) {
        clearTimeout(retryTimeoutRef.current);
      }

      retryTimeoutRef.current = window.setTimeout(() => {
        if (stopped) return;
        reconnecting = false;
        startConnection();
      }, 2000);
    };

    const isSessionExpired = (currentSession: number, pc: RTCPeerConnection) => {
      if (stopped || currentSession !== sessionId) {
        pc.onconnectionstatechange = null;
        pc.ontrack = null;
        pc.close();
        return true;
      }
      return false;
    };

    const startConnection = async () => {
      if (stopped) return;
      const currentSession = ++sessionId;

      if (pcRef.current) {
        pcRef.current.onconnectionstatechange = null;
        pcRef.current.ontrack = null;
        pcRef.current.close();
        pcRef.current = null;
      }

      const pc = new RTCPeerConnection({
        iceServers: [{ urls: 'stun:stun.l.google.com:19302' }],
      });
      pcRef.current = pc;

      pc.ontrack = (event) => {
        if (videoRef.current && event.streams?.[0]) {
          videoRef.current.srcObject = event.streams[0];
        }
      };

      pc.onconnectionstatechange = () => {
        if (pc.connectionState === 'failed' || pc.connectionState === 'disconnected') {
          cleanupAndRetry(pc);
        }
      };

      try {
        const offer = await pc.createOffer({ offerToReceiveAudio: true, offerToReceiveVideo: true });
        if (isSessionExpired(currentSession, pc)) return;

        await pc.setLocalDescription(offer);
        if (isSessionExpired(currentSession, pc)) return;

        const url = `${baseUrl.replace(/\/$/, '')}/${streamName}/whep`;
        console.log('WHEP url =', url);
        const resp = await fetch(url, {
          method: 'POST',
          headers: { 'Content-Type': 'application/sdp' },
          body: offer.sdp,
        });
        if (isSessionExpired(currentSession, pc)) return;
        if (!resp.ok) throw new Error(`WHEP failed: ${resp.status}`);

        const answerSDP = await resp.text();
        if (isSessionExpired(currentSession, pc)) return;

        await pc.setRemoteDescription({ type: 'answer', sdp: answerSDP });
        if (isSessionExpired(currentSession, pc)) return;
        reconnecting = false;
      } catch (err) {
        console.error('WebRTC connection error:', err);
        cleanupAndRetry(pc);
      }
    };

    startConnection();

    return () => {
      stopped = true;
      if (retryTimeoutRef.current) {
        clearTimeout(retryTimeoutRef.current);
        retryTimeoutRef.current = null;
      }
      if (pcRef.current) {
        pcRef.current.onconnectionstatechange = null;
        pcRef.current.ontrack = null;
        pcRef.current.close();
        pcRef.current = null;
      }
    };
  }, [streamName, baseUrl]);

  return (
    <Box
      title={streamName}
      sx={{
        width: '100%',
        height: '100%',
        overflow: 'hidden',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
      }}
    >
      <video
        ref={videoRef}
        autoPlay
        playsInline
        muted
        controls
        style={{ width: '100%', height: '100%', objectFit: 'contain' }}
      />
    </Box>
  );
}
