import * as React from "react";
import { Box, Button } from "@mui/material";

import { useState } from "react";

//"wss://localhost:44307/push"
var url = 'wss://' + window.location.hostname + ':' + window.location.port + '/push';
var socket = new WebSocket(url);


export function WebSockClient() {

  const [isConnected, setIsConnected] = useState(socket.readyState == WebSocket.OPEN);

  socket.onopen = (event) => {
    setIsConnected(true);
  };

  socket.onclose = (event) => {
    setIsConnected(false);
  };

  socket.onmessage = function (event) {
    const json = JSON.parse(event.data);
    try {
      console.log(event.data);
    } catch (err) {
      console.log(err);
    }
  };

  const sendPing = () => {
    socket.send(JSON.stringify('ping ' + new Date().toISOString()));
  }

  const onButtonClick = (event: React.MouseEvent<HTMLElement>) => {

  };

  return (
    <div>

      <React.Fragment key={"WebSock1"}>
        <Box sx={{ border: 1 }}>

          <Button onClick={sendPing } style={{ textTransform: 'none' }}>
            Send ping
          </Button>
          <Button>Connected: {'' + isConnected}</Button>
       </Box>
      </React.Fragment>
    </div>
  );
}