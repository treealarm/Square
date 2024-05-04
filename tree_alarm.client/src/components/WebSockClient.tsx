import * as React from "react";
import * as MarkersVisualStore from '../store/MarkersVisualStates';
import * as MarkersStore from '../store/MarkersStates';
import { Box, Button, Paper, styled, Tooltip } from "@mui/material";
import { useCallback, useEffect, useState } from "react";
import { useSelector } from "react-redux";
import { ApplicationState } from "../store";
import { useAppDispatch } from '../store/configureStore';
import SignalCellular4BarIcon from '@mui/icons-material/SignalCellular4Bar';
import SignalCellularNodataIcon from '@mui/icons-material/SignalCellularNodata';


//"wss://localhost:44307/push"
var url = 'ws://';

if (window.location.protocol == "https:") {
  url = 'wss://';
}
url = url + window.location.hostname + ':' + window.location.port + '/state';

var socket: WebSocket;

export function WebSockClient() {

  const appDispatch = useAppDispatch();

  const box = useSelector((state: ApplicationState) => state?.markersStates?.box);
  const cur_diagram = useSelector((state: ApplicationState) => state?.diagramsStates?.cur_diagram);
  const markers = useSelector((state: ApplicationState) => state?.markersStates?.markers);
  const selected_track = useSelector((state: ApplicationState) => state?.tracksStates?.selected_track);

  const [isConnected, setIsConnected] = useState(false);

  const [updatedTracks, setUpdatedTracks] = useState([]);


  useEffect(() => {
    onTracksUpdated(updatedTracks);
  }, [updatedTracks])

  function socket_onopen(event: any){
    setIsConnected(true);
  };

  function socket_onclose(event: any){
    setIsConnected(false);
    setTimeout(function () {
      connect();
    }, 1000);
  };

  const onTracksUpdated = useCallback(
    (track_ids: string[]) => {
      // TODO update routes newly created.
      console.log("onTracksUpdated:", track_ids, " ", selected_track?.id);
    }, [selected_track]);

  function socket_onmessage(event: any) {
    try {
      console.log(event.data);
      var received = JSON.parse(event.data);

      if (received.action == "set_visual_states") {
        appDispatch<any>(MarkersVisualStore.updateMarkersVisualStates(received.data));
      }

      if (received.action == "set_ids2update") {
        appDispatch<any>(MarkersStore.actionCreators.requestMarkersByIds(received.data));
      }
      if (received.action == "set_ids2delete") {
        appDispatch<any>(MarkersStore.actionCreators.deleteMarkersLocally(received.data));
      }
      if (received.action == "set_alarm_states") {
        appDispatch<any>(MarkersVisualStore.updateMarkersAlarmStates(received.data));
      }
      if (received.action == "update_viewbox") {
        appDispatch<any>(MarkersStore.actionCreators.initiateUpdateAll());
      }
      if (received.action == "update_routes_by_tracks") {        
        var track_ids = received.data as Array<string>;

        setUpdatedTracks(track_ids)               
      }      
      
    } catch (err) {
      console.log(err);
    }
  };

  function connect() {
    socket = new WebSocket(url);
    socket.onopen = socket_onopen;
    socket.onclose = socket_onclose;
    socket.onmessage = socket_onmessage;
  }

  function disconnect() {

    if (socket == null) {
      return;
    }
    socket.close();
    socket = null;
  }

  useEffect(
    () => {
      connect();

      return function cleanup() {
        disconnect();
      };
    }, []);

  const SafeSend = (data: any) => {
    try {
      socket.send(JSON.stringify(data));
    }
    catch (err) {
      console.log(err);
    }
    
  }

  useEffect(() => {
    if (box != null && isConnected) {

      var Message =
      {
        action: "set_box",
        data: box        
      };
      SafeSend(Message);
    }
  }, [box, isConnected]);

  useEffect(() => {
    if (cur_diagram != null && isConnected) {
      var objArray2: string[] = [];
      cur_diagram?.content?.forEach(arr => objArray2.push(arr.id));

      var Message =
      {
        action: "set_ids",
        data: objArray2
      };
      SafeSend(Message);
    }
  }, [cur_diagram, isConnected]);

  const sendPing = () => {
    var Message =
    {
      action: "ping",
      data: 'ping ' + new Date().toISOString(),      
    };
    SafeSend(Message);
  }

  const onButtonClick = (event: React.MouseEvent<HTMLElement>) => {

  };

  return (
      <React.Fragment key={"WebSock1"}>
      <Box>
        <Tooltip title={selected_track?.id + '\n' + url + '\n' + JSON.stringify(box) + '\n' + markers?.figs?.length}>
        <Button
          onClick = {sendPing}
            style = {{ textTransform: 'none' }}
            size = "small"
          >
            {
              isConnected ?
                <SignalCellular4BarIcon sx = {{m: 4}} color="success" />
                :
                <SignalCellularNodataIcon sx={{ m: 4 }} color="error" />
            }
            
            {
              markers?.figs != null ? <div>objects:{markers?.figs?.length.toString() + ","}</div> : <div />
            }
            <div> zoom:{box?.zoom}</div>
          </Button> 
        </Tooltip>
        
      </Box>
      
      </React.Fragment>
  );
}

