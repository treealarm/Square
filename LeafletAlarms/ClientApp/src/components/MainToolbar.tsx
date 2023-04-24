import * as React from "react";

import {
  AppBar,
  Box,
  Toolbar
} from "@mui/material";

import { WebSockClient } from "./WebSockClient";
import GlobalLayersOptions from "../Tree/GlobalLayersOptions";
import { Login } from "../auth/Login";
import PanelSwitch from "./PanelSwitch";

export function MainToolbar() {
return (
<Box sx={{ flexGrow: 1 }}>
  <AppBar sx={{ backgroundColor: '#bbbbbb' }} >
    <Toolbar variant='dense' >
      <Box
        sx={{ flexGrow: 1 }}
        display="flex"
        justifyContent="flex-start"
      >
        <PanelSwitch IsLeftPanel={true} />
        <WebSockClient />

      </Box>
      <Box
        sx={{ flexGrow: 1 }}
        display="flex"
        justifyContent="flex-center"
      >
        <GlobalLayersOptions />

      </Box>
      <Box
        display="flex"
        justifyContent="flex-end"
        alignContent="center"
      >
        <Box sx={{ m: 2, p: 2}}><Login /></Box>

        <PanelSwitch IsLeftPanel={false} />
      </Box>

    </Toolbar>
  </AppBar>
</Box>
);
}