/* eslint-disable no-unused-vars */
import * as React from "react";
import { Box, Button, Tooltip } from "@mui/material";
import SignalCellular4BarIcon from '@mui/icons-material/SignalCellular4Bar';
import SignalCellularNodataIcon from '@mui/icons-material/SignalCellularNodata';
import { useWebSocketClient } from "./useWebSocketClient";

export function WebSockStatus() {
  const { isConnected, markers, box, selected_track, getWebSocketUrl, sendPing } = useWebSocketClient();

  return (
    <Box>
      <Tooltip
        title={`${selected_track?.id}\n${getWebSocketUrl()}\n${JSON.stringify(box)}\n${markers?.figs?.length}`}
      >
        <Button onClick={sendPing} style={{ textTransform: "none" }} size="small">
          {isConnected
            ? <SignalCellular4BarIcon sx={{ m: 1 }} color="success" />
            : <SignalCellularNodataIcon sx={{ m: 1 }} color="error" />}
          {markers?.figs && <div>objects: {markers.figs.length},</div>}
          <div> zoom: {box?.zoom}</div>
        </Button>
      </Tooltip>
    </Box>
  );
}
