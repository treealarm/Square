import * as React from "react";
import TabControl from "../Tree/TabControl";
import { TreeControl } from "../Tree/TreeControl";
import { MapComponent } from "../map/MapComponent";
import EditOptions from "../Tree/EditOptions";
import { ObjectProperties } from "../Tree/ObjectProperties";
import {
  Accordion, AccordionDetails, AccordionSummary,
  Box,
  Grid, IconButton, Paper, Stack, styled, ToggleButton, Toolbar, Tooltip, Typography
} from "@mui/material";

import * as PanelsStore from '../store/PanelsStates';
import { RetroSearch } from "../Tree/RetroSearch";
import { SearchResult } from "../Tree/SearchResult";
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import { ObjectLogic } from "../Logic/ObjectLogic";
import { ObjectRights } from "../Rights/ObjectRights";
import { useSelector } from "react-redux";
import { ApplicationState } from "../store";
import { TrackProps } from "../Tree/TrackProps";
import { DeepCopy, EPanelType, IPanelsStatesDTO, IPanelTypes } from "../store/Marker";
import { MainToolbar } from "./MainToolbar";
import CloseIcon from "@mui/icons-material/Close";
import { useAppDispatch } from "../store/configureStore";

import { PanelIcon } from "./PanelIcon";

const TogglePanelButton = (props: { panel: IPanelsStatesDTO }) =>
{
  const appDispatch = useAppDispatch();

  const panels = useSelector((state: ApplicationState) => state?.panelsStates?.panels);

  var thisPanel = panels.find(p => p.panelId == props.panel.panelId);

  const handleSelectRight = (panelId: string, text: string) => {

    var exist = panels.find(e => e.panelId == panelId);

    if (exist) {
      var removed = panels.filter(e => e.panelId != panelId);
      appDispatch(PanelsStore.set_panels(removed));
    }
    else {
      var newPanels = DeepCopy(panels);

      newPanels = newPanels.filter(e => e.panelType != EPanelType.Right);
      newPanels.push(
        {
          panelId: panelId,
          panelValue: text,
          panelType: EPanelType.Right
        });

      appDispatch(PanelsStore.set_panels(newPanels));
    }
  };

  return (
    <ToggleButton
      value="check"
      aria-label="properties"
      selected={thisPanel != null}
      size="small"
      onChange={() =>
        handleSelectRight(props.panel.panelId,
          props.panel.panelValue)
      }
    >
      <PanelIcon panelId={props.panel.panelId} />
    </ToggleButton>
  );
}

const AccordionPanels = (props: { components: Array<[IPanelsStatesDTO, JSX.Element]> }) => {

  const appDispatch = useAppDispatch();
  const [expand, setExpand] = React.useState(false);
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
                    <TogglePanelButton panel={datum}/>
                  )
                }
              </Box>
            :
            <div/>
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

const LeftPanel = () => {

  const panels = useSelector((state: ApplicationState) => state?.panelsStates?.panels);

  var components: Array<[IPanelsStatesDTO, JSX.Element]> = panels.map((datum) => {

    if (datum.panelId == IPanelTypes.tree) {
      return [datum, (
        <div>
          <TabControl />
          <TreeControl />
        </div>
      )];
    }

    if (datum.panelId == IPanelTypes.search_result) {
      return [datum, (
        <SearchResult></SearchResult>
      )];
    }
    return null;
  });

  return (<AccordionPanels components={components} />);
};

const RightPanel = () => {

  const panels = useSelector((state: ApplicationState) => state?.panelsStates?.panels);


  var components: Array<[IPanelsStatesDTO, JSX.Element]> = panels.map((datum) => {

    if (datum.panelId == IPanelTypes.properties) {
      return [datum, (
        <Stack sx={{ height: "100%" }}>
          <EditOptions />
          <ObjectProperties />
        </Stack>
      )];
    }

    if (datum.panelId == IPanelTypes.search) {
      return [datum, (
        <RetroSearch></RetroSearch>
      )];
    }

    if (datum.panelId == IPanelTypes.logic) {
      return [datum, (
        <ObjectLogic />
      )];
    }

    if (datum.panelId == IPanelTypes.rights) {
      return [datum, (
        <ObjectRights />
      )];
    }

    if (datum.panelId == IPanelTypes.track_props) {
      return [datum, (
        <TrackProps />
      )];
    }
    return null;
  });

  return (<AccordionPanels components={components} />);
};

const Offset = styled('div')(({ theme }) => theme.mixins.toolbar);

export function Home() {

  const panels = useSelector((state: ApplicationState) => state?.panelsStates?.panels);

  var showLeftPannel = panels.find(e => e.panelType == EPanelType.Left) != null;
  var showRightPannel = panels.find(e => e.panelType == EPanelType.Right) != null;

  return (
    <Box sx={{ height: '98vh' }}>
      <MainToolbar  />
      <Toolbar />
      <Grid container sx={{ height: 'calc(100% - 60px)' }}>
      
        <Grid item xs={3} sx={{ height: "100%", display: showLeftPannel ? '' : 'none' }}>
            <Paper sx={{ height: "100%", overflow: 'auto', width: "100%" }} >
              <LeftPanel />
            </Paper>

          </Grid>

          <Grid item xs sx={{ minWidth: "100px" }}>

            <Box sx={{ height: '100%' }}>
              <MapComponent />
            </Box>

          </Grid>

        <Grid item xs={3} sx={{ height: "100%", display: showRightPannel ? '' : 'none' }}>
          <Paper sx={{ height: "100%", overflow: 'auto', width: "100%" }}>
              <RightPanel />
            </Paper>

          </Grid>
        </Grid>
    </Box>
  );
}
