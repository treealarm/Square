import * as React from "react";
import TabControl from "../Tree/TabControl";
import { TreeControl } from "../Tree/TreeControl";
import { MapComponent } from "../map/MapComponent";
import EditOptions from "../Tree/EditOptions";
import { ObjectProperties } from "../Tree/ObjectProperties";
import {
  Box,
  Grid, Paper, Stack, styled, Toolbar,
} from "@mui/material";


import { RetroSearch } from "../Tree/RetroSearch";
import { SearchResult } from "../Tree/SearchResult";
import { ObjectLogic } from "../Logic/ObjectLogic";
import { ObjectRights } from "../Rights/ObjectRights";
import { useSelector } from "react-redux";
import { ApplicationState } from "../store";
import { TrackProps } from "../Tree/TrackProps";
import { EPanelType, IPanelsStatesDTO, IPanelTypes } from "../store/Marker";
import { MainToolbar } from "./MainToolbar";
import { AccordionPanels } from "./AccordionPanels";



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
