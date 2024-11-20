/* eslint-disable react-hooks/exhaustive-deps */
import * as React from "react";
import * as MarkersVisualStore from '../store/MarkersVisualStates';
import * as MarkersStore from '../store/MarkersStates';
import { Box, Button, Tooltip } from "@mui/material";
import { useCallback, useEffect, useState } from "react";
import { useSelector } from "react-redux";
import { ApplicationState } from "../store";
import { useAppDispatch } from '../store/configureStore';
import SignalCellular4BarIcon from '@mui/icons-material/SignalCellular4Bar';
import SignalCellularNodataIcon from '@mui/icons-material/SignalCellularNodata';

// Dynamic WebSocket URL
const getWebSocketUrl = () => {
  const protocol = window.location.protocol === "https:" ? 'wss://' : 'ws://';
  return `${protocol}${window.location.hostname}:${window.location.port}/state`;
};

export function WebSockClient() {
  const appDispatch = useAppDispatch();
  const box = useSelector((state: ApplicationState) => state?.markersStates?.box);
  const cur_diagram = useSelector((state: ApplicationState) => state?.diagramsStates?.cur_diagram_content);
  const markers = useSelector((state: ApplicationState) => state?.markersStates?.markers);
  const selected_track = useSelector((state: ApplicationState) => state?.tracksStates?.selected_track);

  const [isConnected, setIsConnected] = useState(false);
  const [updatedTracks, setUpdatedTracks] = useState<string[]>([]);
  const [socket, setSocket] = useState<WebSocket | null>(null);

  const onTracksUpdated = useCallback((track_ids: string[]) => {
    console.log("onTracksUpdated:", track_ids, " ", selected_track?.id);
  }, [selected_track]);

  useEffect(() => {
    onTracksUpdated(updatedTracks);
  }, [onTracksUpdated, updatedTracks]);

  const handleOpen = useCallback(() => setIsConnected(true), []);
  const handleClose = useCallback(() => {
    setIsConnected(false);
    setTimeout(() => connect(), 1000);
  }, []);
  const handleMessage = useCallback((event: MessageEvent) => {
    try {
      console.log(event.data);
      const received = JSON.parse(event.data);

      switch (received.action) {
        case "set_visual_states":
          appDispatch(MarkersVisualStore.requestAndUpdateMarkersVisualStates(received.data));
          break;
        case "set_ids2update":
          appDispatch(MarkersStore.fetchMarkersByIds(received.data));
          break;
        case "set_ids2delete":
          appDispatch(MarkersStore.deleteMarkersLocally(received.data));
          break;
        case "set_alarm_states":
          appDispatch(MarkersVisualStore.updateMarkersAlarmStates(received.data));
          break;
        case "update_viewbox":
          appDispatch(MarkersStore.initiateUpdateAll());
          break;
        case "update_routes_by_tracks":
          setUpdatedTracks(received.data as string[]);
          break;
        default:
          console.log("Unknown action:", received.action);
      }
    } catch (err) {
      console.log(err);
    }
  }, [appDispatch]);

  const connect = useCallback(() => {
    const ws = new WebSocket(getWebSocketUrl());
    ws.onopen = handleOpen;
    ws.onclose = handleClose;
    ws.onmessage = handleMessage;
    setSocket(ws);
  }, [handleOpen, handleClose, handleMessage]);

  const disconnect = useCallback(() => {
    if (socket) {
      socket.close();
      setSocket(null);
    }
  }, [socket]);

  useEffect(() => {
    connect();
    return () => disconnect();
  }, []);

  const safeSend = (data: any) => {
    try {
      socket?.send(JSON.stringify(data));
    } catch (err) {
      console.log(err);
    }
  };

  useEffect(() => {
    if (box && isConnected) {
      const message = { action: "set_box", data: box };
      safeSend(message);
    }
  }, [box, isConnected]);

  useEffect(() => {
    if (cur_diagram && isConnected) {
      const ids = cur_diagram.content?.map(arr => arr.id) || [];
      const message = { action: "set_ids", data: ids };
      safeSend(message);
    }
  }, [cur_diagram, isConnected]);

  const sendPing = () => {
    const message = { action: "ping", data: `ping ${new Date().toISOString()}` };
    safeSend(message);
  };

  return (
    <React.Fragment key={"WebSock1"}>
      <Box>
        <Tooltip title={`${selected_track?.id}\n${getWebSocketUrl()}\n${JSON.stringify(box)}\n${markers?.figs?.length}`}>
          <Button
            onClick={sendPing}
            style={{ textTransform: 'none' }}
            size="small"
          >
            {isConnected
              ? <SignalCellular4BarIcon sx={{ m: 4 }} color="success" />
              : <SignalCellularNodataIcon sx={{ m: 4 }} color="error" />
            }
            {markers?.figs && <div>objects: {markers.figs.length},</div>}
            <div> zoom: {box?.zoom}</div>
          </Button>
        </Tooltip>
      </Box>
    </React.Fragment>
  );
}
