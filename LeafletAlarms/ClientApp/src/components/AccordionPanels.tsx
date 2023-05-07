import * as React from "react";
import {
  Accordion, AccordionDetails, AccordionSummary,
  Box,
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

export const AccordionPanels = (props: { components: Array<[IPanelsStatesDTO, JSX.Element]> }) => {

  const appDispatch = useAppDispatch();
  const [expand, setExpand] = React.useState(true);
  const toggleAccordion = () => {
    setExpand((prev) => !prev);
  };

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

  var counter = 0;

  var accordions = components.map((component) => {

    var color = '#dddddd';

    if (counter % 2 == 0) {
      color = '#f0f0f0';
    }
    counter++;

    return (
      <Accordion key={counter} expanded={expand} sx={{ backgroundColor: color }}>
        <AccordionSummary expandIcon={<ExpandMoreIcon
          onClick={toggleAccordion}
        />} sx={{ backgroundColor: color }}>

          <Box
            sx={{ flexGrow: 1 }}
            display="flex"
            justifyContent="flex-start"
          >
            <Typography sx={{ fontWeight: 'bold' }}>{component[0].panelValue}</Typography>
          </Box>

          {
            showToggleButtons ?
              <Box
                sx={{ flexGrow: 1 }}
                display="flex"
                justifyContent="flex-end"
                alignContent="center">

                {
                  togglePanels.map((datum) =>
                    <TogglePanelButton panel={datum} />
                  )
                }
              </Box>
              :
              <div />
          }


          <Box
            display="flex"
            justifyContent="flex-end"
            alignContent="center"
          >
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

        </AccordionSummary>
        <AccordionDetails sx={{ backgroundColor: color }}>
          {component[1]}
        </AccordionDetails>
      </Accordion>
    )
  });

  return <div>{accordions}</div>;
};