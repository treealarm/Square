﻿import * as React from 'react';
import {
  Box,
  IconButton, Tooltip, Typography
} from "@mui/material";
import { TogglePanelButton } from "./TogglePanelButton";

import CloseIcon from "@mui/icons-material/Close";

import { useAppDispatch } from "../store/configureStore";
import { EPanelType, IPanelsStatesDTO, IPanelTypes } from "../store/Marker";
import { useSelector } from "react-redux";
import { ApplicationState } from "../store";
import * as PanelsStore from '../store/PanelsStates';

export const AccordionPanels = (props: { components: Array<[IPanelsStatesDTO, React.ReactElement]> }) => {

  const appDispatch = useAppDispatch();

  var components = props.components.filter(e => e != null);
  const panels = useSelector((state: ApplicationState) => state?.panelsStates?.panels)??[];

  if (components.length == 0) {
    return null;
  }

  const handleClose = (panelId: string) => {

    var removed = panels.filter(e => e.panelId != panelId);
    appDispatch(PanelsStore.set_panels(removed));
  };

  var accordions = components.map((component) => {

    var color = '#eee';

    var togglePanels: IPanelsStatesDTO[] = [];

    if (component[0].panelType == EPanelType.Right) {
      togglePanels = IPanelTypes.panels.filter(p =>
        p.panelId == IPanelTypes.search ||
        p.panelId == IPanelTypes.properties ||
        p.panelId == IPanelTypes.track_props ||
        p.panelId == IPanelTypes.actions
      );

      if (component[0].panelId == IPanelTypes.search) {
        const searchResultPanel = IPanelTypes.panels.find(
          p => p.panelId == IPanelTypes.search_result
        );

        if (searchResultPanel) {
          togglePanels.unshift(searchResultPanel);
        }
      }

    }

    if (component[0].panelId == IPanelTypes.tree) {
      togglePanels = IPanelTypes.panels.filter(p =>
        p.panelId == IPanelTypes.properties
      );
    }

      if (component[0].panelId == IPanelTypes.search_result) {
      togglePanels = IPanelTypes.panels.filter(p =>
        p.panelId == IPanelTypes.track_props
      );
    }

    // if we want to hide buttons which are active
    //togglePanels = togglePanels.filter(p => panels.findIndex(p1 => p1.panelId == p.panelId) < 0);

    return (
      <Box
        key={"Box1"+component[0].panelId}
        sx={{
        width: '100%',
        height: '100%',
        display: 'flex',
        flexDirection: 'column'
        
      }}>
        <Box
          key={"Box2" +component[0].panelId}
            sx={{ flexGrow: 1, backgroundColor: color, display:'flex' }}>

          <Box
            display="flex"
            justifyContent="flex-start"
            alignContent="center"
            sx={{ flexGrow: 1, backgroundColor: color }}>
              <Typography sx={{ p: 2, fontWeight: 'bold' }}>{component[0].panelValue}</Typography>            
          </Box>
          
          <Box
            sx={{ flexGrow: 1 }}
            display="flex"
            justifyContent="center"
            alignContent="center">

            {
              togglePanels.map((datum) =>
                <TogglePanelButton panel={datum} key={"TogglePanelButton:" + datum.panelId} />
              )
            }
          </Box>
          
          <Box
            display="flex"
            justifyContent="flex-end"
            alignContent="center">
            <Tooltip title="Remove panel">
              <IconButton
                aria-label="close"
                size="small"
                onClick={() => handleClose(component[0].panelId)}

              >
                <CloseIcon />
              </IconButton>
            </Tooltip>
          </Box>

          </Box>


        <Box
          key={"Box3" +component[0].panelId}
          sx={{
          width: '100%',
          height: '100%',
          overflow: 'auto'
          
        }}>
          {component[1]}
        </Box>
      </Box>
    )
  });

  return <Box sx={{
    width: '100%',
    height:'100%',
    display: 'flex',
    flexDirection: 'column'
  }}>{accordions}</Box>;
};