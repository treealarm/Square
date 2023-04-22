import * as React from "react";
import TabControl from "../Tree/TabControl";
import { TreeControl } from "../Tree/TreeControl";
import { MapComponent } from "../map/MapComponent";
import EditOptions from "../Tree/EditOptions";
import { ObjectProperties } from "../Tree/ObjectProperties";
import {
  Accordion, AccordionDetails, AccordionSummary,
  AppBar,
  Box,
  Checkbox,
  FormControlLabel,
  FormGroup,
  Grid, Paper, Stack, Toolbar, Typography
} from "@mui/material";
import { WebSockClient } from "./WebSockClient";
import { RetroSearch } from "../Tree/RetroSearch";
import { SearchResult } from "../Tree/SearchResult";
import GlobalLayersOptions from "../Tree/GlobalLayersOptions";
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import { ObjectLogic } from "../Logic/ObjectLogic";
import { Login } from "../auth/Login";
import { ObjectRights } from "../Rights/ObjectRights";
import PanelSwitch from "./PanelSwitch";
import { useSelector } from "react-redux";
import { ApplicationState } from "../store";
import { TrackProps } from "../Tree/TrackProps";
import { IPanelsStatesDTO, IPanelTypes } from "../store/Marker";

const LeftPanel = () => {

  const panels = useSelector((state: ApplicationState) => state?.panelsStates?.panels);

  var components = panels.map((datum) => {

    if (datum.panelId == IPanelTypes.tree) {
      return (
        <Accordion defaultExpanded>
          <AccordionSummary
            expandIcon={<ExpandMoreIcon/>}
            aria-controls="panel-tree"
            id="panel-tree">
            <Typography>Tree</Typography>
          </AccordionSummary>
          <AccordionDetails sx={{ maxHeight: "100%", padding: 1, margin: 0 }} >
            <TabControl />
            <TreeControl />
          </AccordionDetails>
        </Accordion>
      );
    }

    if (datum.panelId == IPanelTypes.search_result) {
      return (
        <Accordion defaultExpanded>
          <AccordionSummary
            expandIcon={<ExpandMoreIcon/>}
            aria-controls="panel-search-result"
            id="panel-search-result">
            <Typography>Search result</Typography>
          </AccordionSummary>
          <AccordionDetails>
            <SearchResult></SearchResult>
          </AccordionDetails>
        </Accordion>
      );
    }
    return null;
  });

  components = components.filter(e => e != null);

  if (components.length == 0) {
    return null;
  }
  return <div>{components}</div>;
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

  components = components.filter(e => e != null);

  if (components.length == 0) {
    return null;
  }

  var accordions = components.map((component) => {
    return (
      <Accordion defaultExpanded>
        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
          <Typography>{component[0].panelValue}</Typography>
        </AccordionSummary>
        <AccordionDetails>
          {component[1]}
        </AccordionDetails>
      </Accordion>
    )
  });

  return <div>{accordions}</div>;
};


export function Home() {

  const panels = useSelector((state: ApplicationState) => state?.panelsStates?.panels);

  var search_result = panels.find(e => e.panelId == IPanelTypes.search_result);
  var tree = panels.find(e => e.panelId == IPanelTypes.tree);

  var showLeftPannel = tree != null || search_result != null;

  var properties = panels.find(e => e.panelId == IPanelTypes.properties);
  var search = panels.find(e => e.panelId == IPanelTypes.search);
  var logic = panels.find(e => e.panelId == IPanelTypes.logic);
  var rights = panels.find(e => e.panelId == IPanelTypes.rights);
  var track_props = panels.find(e => e.panelId == IPanelTypes.track_props);

  var showRightPannel =
    properties != null ||
    logic != null ||
    rights != null ||
    search != null ||
    track_props != null;

  return (
    <Grid container spacing={1} sx={{ height: "100%", p: "1px" }}>
      <Grid item xs={12} sx={{ height: "auto" }}>
        <Box sx={{ flexGrow: 1 }}>
          <AppBar position="static" sx={{ backgroundColor: '#aaaaaa' }} >
            <Toolbar>  
              <PanelSwitch />
          <WebSockClient />
          <GlobalLayersOptions />
          <Login />
            </Toolbar>
          </AppBar>
        </Box>
      </Grid>

      <Grid item xs={2} sx={{ height: "90%", display: showLeftPannel ? '' : 'none' }}
        container spacing={0}>
        <Paper sx={{ maxHeight: "100%", overflow: 'auto', width: "100%" }} >
          <LeftPanel/>
        </Paper>

      </Grid>

      <Grid item xs sx={{ minWidth: "100px", height: "90%", flexGrow: 1 }} container spacing={0}>
        <Box sx={{ flexGrow: 1}}>
          <MapComponent />
        </Box>
        
      </Grid>

      <Grid item xs={3} sx={{ height: "90%", display: showRightPannel ? '' : 'none' }}
        container spacing={0}>
        <Paper sx={{ maxHeight: "100%", overflow: 'auto', width: "100%" }}>
          <RightPanel/>
        </Paper>

      </Grid>
    </Grid>
  );
}
