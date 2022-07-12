import * as React from "react";
import { Box, Button, IconButton } from "@mui/material";
import { useEffect, useState } from "react";
import CircleIcon from '@mui/icons-material/Circle';
import { useSelector } from "react-redux";
//"wss://localhost:44307/push"
var url = 'wss://' + window.location.hostname + ':' + window.location.port + '/state';
var socket = new WebSocket(url);


export function WebSockClient() {

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
    } catch (err) {
      console.log(err);
    }
  };

  useEffect(() => {
    if (box != null && isConnected) {

      var Message =
      {
        box: box,
        action: "set_box"
      };
      socket.send(JSON.stringify(Message));
    }
  }, [box]);

  const sendPing = () => {
    var Message =
    {
      ping: 'ping ' + new Date().toISOString(),
      action: "ping"
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