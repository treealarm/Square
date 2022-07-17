import * as React from "react";
import * as MarkersVisualStore from '../store/MarkersVisualStates';
import * as MarkersStore from '../store/MarkersStates';
import { Box, Button, IconButton } from "@mui/material";
import { useEffect, useState } from "react";
import CircleIcon from '@mui/icons-material/Circle';
import { useDispatch, useSelector } from "react-redux";
//"wss://localhost:44307/push"
var url = 'wss://' + window.location.hostname + ':' + window.location.port + '/state';
var socket = new WebSocket(url);


export function WebSockClient() {

  const dispatch = useDispatch();

  const box = useSelector((state) => state?.markersStates?.box);

  const [isConnected, setIsConnected] = useState(socket.readyState == WebSocket.OPEN);

  socket.onopen = (event) => {
    setIsConnected(true);
  };

  socket.onclose = (event) => {
    setIsConnected(false);
  };

  socket.onmessage = function (event) {
    try {
      console.log(event.data);
      var received = JSON.parse(event.data);

      if (received.action == "set_visual_states") {
        dispatch(MarkersVisualStore.actionCreators.setMarkersVisualStates(received.data));
      }

      if (received.action == "set_ids2update") {
        dispatch(MarkersStore.actionCreators.requestMarkersByIds(received.data));
      }
      
    } catch (err) {
      console.log(err);
    }
  };

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
          <IconButton onClick={sendPing} style={{ textTransform: 'none' }}>
            <CircleIcon color={isConnected ? "success" : "error"} />
          </IconButton>
       </Box>
      </React.Fragment>
  );
}