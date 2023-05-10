import * as React from "react";
import {
  Accordion, AccordionDetails, AccordionSummary,
  Box,
  Card,
  IconButton, Tooltip, Typography
} from "@mui/material";
import { TogglePanelButton } from "./TogglePanelButton";

import CloseIcon from "@mui/icons-material/Close";
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';

import { useAppDispatch } from "../store/configureStore";
import { EPanelType, IPanelsStatesDTO, IPanelTypes } from "../store/Marker";
import { useSelector } from "react-redux";
import { ApplicationState } from "../store";
import * as PanelsStore from '../store/PanelsStates';
import EditOptions from "../Tree/EditOptions";

export const AccordionPanels = (props: { components: Array<[IPanelsStatesDTO, JSX.Element]> }) => {

  const appDispatch = useAppDispatch();

  var components = props.components.filter(e => e != null);
  const panels = useSelector((state: ApplicationState) => state?.panelsStates?.panels);

  if (components.length == 0) {
    return null;
  }

  var togglePanels = IPanelTypes.panels.filter(p =>
    p.panelId == IPanelTypes.search ||
    p.panelId == IPanelTypes.properties ||
    p.panelId == IPanelTypes.track_props
  );


  var showToggleButtons = false;

  if (components[0][0].panelType == EPanelType.Right) {
    showToggleButtons = panels
      .filter(p => togglePanels.find(p1 => p1.panelId == p.panelId) != null)
      .length > 0;
  }


  const handleClose = (panelId: string) => {

    var removed = panels.filter(e => e.panelId != panelId);
    appDispatch(PanelsStore.set_panels(removed));
  };

  var accordions = components.map((component) => {

    var color = '#eee';

    return (
      <Box sx={{
        width: '100%',
        height: '100%',
        display: 'flex',
        flexDirection: 'column'
      }}>
          <Box
            sx={{ flexGrow: 1, backgroundColor: color, display:'flex' }}>

          <Box
            display="flex"
            justifyContent="flex-start"
            alignContent="center"
            sx={{ flexGrow: 1, backgroundColor: color }}>
              <Typography sx={{ p: 2, fontWeight: 'bold' }}>{component[0].panelValue}</Typography>            
          </Box>
          {
            component[0].panelId == IPanelTypes.properties ?
              <EditOptions />
              :
              <div/>
          }
          
          {
            showToggleButtons ?
              <Box
                sx={{ flexGrow: 1 }}
                display="flex"
                justifyContent="flex-end"
                alignContent="center">

                {
                  togglePanels.map((datum) =>
                    <TogglePanelButton panel={datum} key={"TogglePanelButton:" + datum.panelId} />
                  )
                }
              </Box>
              :
              <div />

              
          }
          
          <Box
            display="flex"
            justifyContent="flex-end"
            alignContent="center">
            <Tooltip title="Remove panel">
              <IconButton
                aria-label="close"
                size="small"
                onClick={(e: any) => handleClose(component[0].panelId)}

              >
                <CloseIcon />
              </IconButton>
            </Tooltip>
          </Box>

          </Box>


        <Box sx={{
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