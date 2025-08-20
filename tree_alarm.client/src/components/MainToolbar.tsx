/* eslint-disable no-unused-vars */

import {
  AppBar,
  Box,
  Button,
  Toolbar
} from "@mui/material";

import { WebSockStatus } from "./WebSockStatus";
import GlobalLayersOptions from "../tree/GlobalLayersOptions";
import { Login } from "../auth/Login";
import PanelSwitch from "./PanelSwitch";
import { EPanelType } from "../store/Marker";
import { SearchApplyButton } from "../tree/SearchApplyButton";
import { FlyToMyLocationButton } from "../tree/FlyToMyLocationButton";
import DiagramNavigation from "../diagrams/DiagramNavigation";
import { useNavigate } from "react-router-dom";

export function MainToolbar() {

  const navigate = useNavigate();

  const goToStatesViewer = () => {
    navigate("/_states"); // путь к твоему StateViewer
  };

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
            <WebSockStatus/>

          </Box>

          <Box
            sx={{ flexGrow: 1 }}
            display="flex"
            justifyContent="flex-end"
          >
            <SearchApplyButton hideIfNotPushed={true} />  
            <FlyToMyLocationButton />
            <DiagramNavigation />
          </Box>

          {/* кнопка перехода в StateViewer */}
          <Button onClick={goToStatesViewer} variant="contained" size="small" sx={{ ml: 2 }}>
            States Viewer
          </Button>

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