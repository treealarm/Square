import * as React from "react";
import * as MarkersVisualStore from '../store/MarkersVisualStates';
import * as MarkersStore from '../store/MarkersStates';
import { Box, Button, IconButton, Paper, styled, Tooltip } from "@mui/material";
import { useEffect, useState } from "react";
import CircleIcon from '@mui/icons-material/Circle';
import { useDispatch, useSelector } from "react-redux";

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

  const dispatch = useDispatch();

  const box = useSelector((state) => state?.markersStates?.box);

  const [isConnected, setIsConnected] = useState(false);

  function socket_onopen(event: any){
    setIsConnected(true);
  };

  function socket_onclose(event: any){
    setIsConnected(false);
    setTimeout(function () {
      connect();
    }, 1000);
  };

  const markers = useSelector((state) => state?.markersStates?.markers);

  function socket_onmessage(event: any) {
    try {
      console.log(event.data);
      var received = JSON.parse(event.data);

      if (received.action == "set_visual_states") {
        dispatch(MarkersVisualStore.actionCreators.updateMarkersVisualStates(received.data));
      }

      if (received.action == "set_ids2update") {
        dispatch(MarkersStore.actionCreators.requestMarkersByIds(received.data));
      }
      if (received.action == "set_ids2delete") {
        dispatch(MarkersStore.actionCreators.deleteMarkersLocally(received.data));
      }
      if (received.action == "set_alarm_states") {
        dispatch(MarkersVisualStore.actionCreators.updateMarkersAlarmStates(received.data));
      }
      if (received.action == "update_viewbox") {
        dispatch(MarkersStore.actionCreators.initiateUpdateAll());
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
  useEffect(
    () => {
      connect();
    }, []);

  useEffect(() => {
    if (box != null && isConnected) {

      var Message =
      {
        action: "set_box",
        data: box        
      };
      socket.send(JSON.stringify(Message));
    }
  }, [box, isConnected]);

  const sendPing = () => {
    var Message =
    {
      action: "ping",
      data: 'ping ' + new Date().toISOString(),      
    };
    socket.send(JSON.stringify(Message));
  }

  const onButtonClick = (event: React.MouseEvent<HTMLElement>) => {

  };

  return (
      <React.Fragment key={"WebSock1"}>
      <Box sx={{ border: 1 }}>
        <Tooltip title={url + '\n' + JSON.stringify(box) + '\n' + markers?.figs?.length}>
        <IconButton
          onClick={sendPing}
          style={{ textTransform: 'none' }}
          >
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