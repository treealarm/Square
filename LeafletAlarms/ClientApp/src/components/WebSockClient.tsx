import * as React from "react";
import * as MarkersVisualStore from '../store/MarkersVisualStates';
import * as MarkersStore from '../store/MarkersStates';
import { Box, IconButton, Paper, styled, Tooltip } from "@mui/material";
import { useCallback, useEffect, useState } from "react";
import CircleIcon from '@mui/icons-material/Circle';
import { useDispatch, useSelector } from "react-redux";
import { ApplicationState } from "../store";
import { useAppDispatch } from '../store/configureStore';
import * as TracksStore from '../store/TracksStates';

const  REACT_APP_AUTH_SERVER_URL  = process.env.REACT_APP_AUTH_SERVER_URL;

const Item = styled(Paper)(({ theme }) => ({
  ...theme.typography.body2,
  textAlign: 'center',
  color: theme.palette.text.secondary,
  height: 10,
  lineHeight: '10px',
}));

//"wss://localhost:44307/push"
var url = 'ws://';

if (window.location.protocol == "https:") {
  url = 'wss://';
}
url = url + window.location.hostname + ':' + window.location.port + '/state';

var socket: WebSocket;

export function WebSockClient() {

  //const dispatch = useDispatch();
  const appDispatch = useAppDispatch();

  const box = useSelector((state: ApplicationState) => state?.markersStates?.box);
  const markers = useSelector((state: ApplicationState) => state?.markersStates?.markers);
  const selected_tracks = useSelector((state: ApplicationState) => state?.tracksStates?.selected_tracks);

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
      console.log("selected tracks:", track_ids, " ", selected_tracks);

      if (track_ids != null && selected_tracks != null) {
        const filteredArray = selected_tracks.filter(value => track_ids.includes(value));

        console.log("filteredArray:", filteredArray);

        if (filteredArray.length > 0) {
          appDispatch<any>(TracksStore.actionCreators.GetRoutsByTracksIds(selected_tracks));
        }
      } 
    }, [selected_tracks]);

  function socket_onmessage(event: any) {
    try {
      console.log(event.data);
      var received = JSON.parse(event.data);

      if (received.action == "set_visual_states") {
        appDispatch<any>(MarkersVisualStore.actionCreators.updateMarkersVisualStates(received.data));
      }

      if (received.action == "set_ids2update") {
        appDispatch<any>(MarkersStore.actionCreators.requestMarkersByIds(received.data));
      }
      if (received.action == "set_ids2delete") {
        appDispatch<any>(MarkersStore.actionCreators.deleteMarkersLocally(received.data));
      }
      if (received.action == "set_alarm_states") {
        appDispatch<any>(MarkersVisualStore.actionCreators.updateMarkersAlarmStates(received.data));
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
      <Box sx={{ border: 1 }}>
        <Tooltip title={selected_tracks + '\n'+url + '\n' + JSON.stringify(box) + '\n' + markers?.figs?.length}>
        <IconButton
          onClick = {sendPing}
            style = {{ textTransform: 'none' }}
            size = "small"
          >
            <h1>{REACT_APP_AUTH_SERVER_URL}</h1>
          <CircleIcon color={isConnected ? "success" : "error"} />
            <Item key={'item1'} elevation={1}>
              objs:{markers?.figs?.length}, zoom:{box?.zoom}
            </Item>
          </IconButton> 
        </Tooltip>
        
      </Box>
      
      </React.Fragment>
  );
}

