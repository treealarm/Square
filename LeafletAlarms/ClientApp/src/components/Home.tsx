import * as React from "react";
import TabControl from "../Tree/TabControl";
import { TreeControl } from "../Tree/TreeControl";
import { MapComponent } from "../map/MapComponent";
import EditOptions from "../Tree/EditOptions";
import { ObjectProperties } from "../Tree/ObjectProperties";
import {
  Accordion, AccordionDetails, AccordionSummary,
  Box,
  Grid, Paper, Stack, styled, Toolbar, Typography
} from "@mui/material";

import { RetroSearch } from "../Tree/RetroSearch";
import { SearchResult } from "../Tree/SearchResult";
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import { ObjectLogic } from "../Logic/ObjectLogic";
import { ObjectRights } from "../Rights/ObjectRights";
import { useSelector } from "react-redux";
import { ApplicationState } from "../store";
import { TrackProps } from "../Tree/TrackProps";
import { IPanelsStatesDTO, IPanelTypes } from "../store/Marker";
import { MainToolbar } from "./MainToolbar";


const AccordionPanels = (props: { components: Array<[IPanelsStatesDTO, JSX.Element]> }) => {

  var components = props.components.filter(e => e != null);

  if (components.length == 0) {
    return null;
  }

  var counter = 0;

  var accordions = components.map((component) => {

    var color = '#dddddd';

    if (counter % 2 == 0) {
      color = '#f0f0f0';
    }
    counter++;

    return (
      <Accordion key={ counter } defaultExpanded sx={{ backgroundColor: color }}>
        <AccordionSummary expandIcon={<ExpandMoreIcon />} sx={{ backgroundColor: color }}>
          <Typography sx={{ fontWeight: 'bold' }}>{component[0].panelValue}</Typography>
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

  var showLeftPannel = panels.find(e => e.IsLeft) != null;
  var showRightPannel = panels.find(e => e.IsLeft == false) != null;

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
