/* eslint-disable react-hooks/exhaustive-deps */
import { useCallback, useEffect, useState } from "react";
import { useAppDispatch } from "../store/configureStore";
import * as MarkersVisualStore from '../store/MarkersVisualStates';
import * as MarkersStore from '../store/MarkersStates';
import * as ValuesStore from '../store/ValuesStates';
import { ApplicationState } from "../store";
import { useSelector } from "react-redux";

const getWebSocketUrl = () => {
  const protocol = window.location.protocol === "https:" ? 'wss://' : 'ws://';
  return `${protocol}${window.location.hostname}:${window.location.port}/state`;
};

export function useWebSocketClient() {
  const appDispatch = useAppDispatch();

  const box = useSelector((state: ApplicationState) => state?.markersStates?.box);
  const cur_diagram = useSelector((state: ApplicationState) => state?.diagramsStates?.cur_diagram_content);
  const markers = useSelector((state: ApplicationState) => state?.markersStates?.markers);
  const selected_track = useSelector((state: ApplicationState) => state?.tracksStates?.selected_track);
  const update_values_periodically = useSelector((state: ApplicationState) => state?.valuesStates?.update_values_periodically);
  const cur_interface = useSelector((state: ApplicationState) => state?.guiStates?.cur_interface);
  const checked_ids = useSelector((state: ApplicationState) => state?.guiStates?.checked) ?? [];

  const [isConnected, setIsConnected] = useState(false);
  const [updatedTracks, setUpdatedTracks] = useState<string[]>([]);
  const [socket, setSocket] = useState<WebSocket | null>(null);

  const handleOpen = useCallback(() => setIsConnected(true), []);
  const handleClose = useCallback(() => {
    setIsConnected(false);
    setTimeout(() => connect(), 1000);
  }, []);

  const handleMessage = useCallback((event: MessageEvent) => {
    try {
      const received = JSON.parse(event.data);

      switch (received.action) {
        case "set_visual_states":
        case "set_alarm_states":
          appDispatch(MarkersVisualStore.requestAndUpdateMarkersVisualStates(received.data));
          break;
        case "set_ids2update":
          appDispatch(MarkersStore.fetchMarkersByIds(received.data));
          break;
        case "set_ids2delete":
          appDispatch(MarkersStore.deleteMarkersLocally(received.data));
          break;
        case "update_viewbox":
          appDispatch(MarkersStore.initiateUpdateAll());
          break;
        case "update_routes_by_tracks":
          setUpdatedTracks(received.data as string[]);
          break;
        case "on_values_changed":
          appDispatch(ValuesStore.fetchValuesByOwners(received.data));
          break;
        default:
          console.log("Unknown action:", received.action);
      }
    } catch (err) {
      console.log(err);
    }
  }, []);

  const connect = useCallback(() => {
    if (socket && (socket.readyState === WebSocket.OPEN || socket.readyState === WebSocket.CONNECTING)) {
      return;
    }

    const ws = new WebSocket(getWebSocketUrl());
    ws.onopen = handleOpen;
    ws.onclose = handleClose;
    ws.onmessage = handleMessage;

    if (socket) {
      socket.onopen = null;
      socket.onclose = null;
      socket.onmessage = null;
      socket.close();
    }

    setSocket(ws);
  }, [socket, handleOpen, handleClose, handleMessage]);

  const disconnect = useCallback(() => {
    if (socket) {
      socket.onopen = null;
      socket.onclose = null;
      socket.onmessage = null;
      socket.close();
      setSocket(null);
    }
  }, [socket]);

  useEffect(() => {
    connect();
    return () => disconnect();
  }, [connect, disconnect]);

  const safeSend = useCallback((data: any) => {
    try {
      socket?.send(JSON.stringify(data));
    } catch (err) {
      console.log(err);
    }
  }, [socket]);

  // Box update
  useEffect(() => {
    if (box && isConnected) {
      safeSend({ action: "set_box", data: box });
    }
  }, [box, isConnected]);

  // IDs update
  useEffect(() => {
    if (isConnected) {
      let ids: string[] = [];

      if (cur_interface === "_states") {
        ids = checked_ids;
      } else if (cur_diagram) {
        ids = cur_diagram.content?.map(arr => arr.id) || [];
      }

      if (ids.length > 0) {
        safeSend({ action: "set_ids", data: ids });
      }
    }
  }, [cur_interface, cur_diagram, checked_ids, isConnected]);

  // Values periodic update
  useEffect(() => {
    safeSend({ action: "update_values_periodically", data: JSON.stringify(update_values_periodically) });
  }, [update_values_periodically]);

  const sendPing = () => {
    safeSend({ action: "ping", data: `ping ${new Date().toISOString()}` });
  };

  return {
    isConnected,
    markers,
    box,
    selected_track,
    getWebSocketUrl,
    sendPing,
  };
}
