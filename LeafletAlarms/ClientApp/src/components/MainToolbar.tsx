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
import { EPanelType } from "../store/Marker";
import { SearchApplyButton } from "../Tree/SearchApplyButton";
import EditOptions from "../Tree/EditOptions";

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
            <PanelSwitch panelType={EPanelType.Left} />
            <WebSockClient />

          </Box>

          <Box
            sx={{ flexGrow: 1 }}
            display="flex"
            justifyContent="flex-end"
          >
            <SearchApplyButton hideIfNotPushed={true} />            
          </Box>

          <Box
            sx={{ flexGrow: 1 }}
            display="flex"
            justifyContent="flex-end"
          >            
            <GlobalLayersOptions />
          </Box>          

          <Box
            display="flex"
            justifyContent="flex-end"
            alignContent="center"
          >
            <Box sx={{ m: 2, p: 2 }}><Login /></Box>

            <PanelSwitch panelType={EPanelType.Right} />
          </Box>

        </Toolbar>
      </AppBar>
    </Box>
  );
}