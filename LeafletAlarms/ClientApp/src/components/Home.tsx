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
          <TreeControl />
      )];
    }

    if (datum.panelId == IPanelTypes.search_result) {
      return [datum, (
        <SearchResult/>
      )];
    }
    return null;
  });

  return (
    <AccordionPanels components={components} />      
  );
};

const RightPanel = () => {

  const panels = useSelector((state: ApplicationState) => state?.panelsStates?.panels);


  var components: Array<[IPanelsStatesDTO, JSX.Element]> = panels.map((datum) => {

    if (datum.panelId == IPanelTypes.properties) {
      return [datum, (         
          <ObjectProperties />
      )];
    }

    if (datum.panelId == IPanelTypes.search) {
      return [datum, (
        <RetroSearch/>
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
    <Box sx={{ height: '98vh', display: 'flex', flexDirection: 'column' }}>
      <Box sx={{
        width: '100%'
      }}
      >
      <MainToolbar  />
        <Toolbar />
      </Box>

      <Grid container sx={{
        height: '100%',
        width: '100%',
        overflow: 'auto',
        flex: 1
      }}>
      
        <Grid item xs={3} sx={{ height: "100%", display: showLeftPannel ? '' : 'none' }}>
          <LeftPanel />
          </Grid>

        <Grid item xs sx={{ minWidth: '100px', minHeight: '100px', height: '100%' }}>
              <MapComponent />
          </Grid>

        <Grid item xs={3} sx={{ height: "100%", display: showRightPannel ? '' : 'none' }}>
              <RightPanel />
          </Grid>
        </Grid>
    </Box>
  );
}
