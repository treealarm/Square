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

const LeftPanel = () => {

  const panels = useSelector((state: ApplicationState) => state?.panelsStates?.panels);

  var components = panels.map((datum) => {

    if (datum.panelName == "tree") {
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

    if (datum.panelName == "search_result") {
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

  var components = panels.map((datum) => {

    if (datum.panelName == "properties") {
      return (
        <Accordion defaultExpanded>
          <AccordionSummary
            expandIcon={<ExpandMoreIcon color="primary" />}
            aria-controls="panel1a-content"
            id="panel1a-header">
            <Typography color="primary">Properties</Typography>
          </AccordionSummary>
          <AccordionDetails>
            <Stack sx={{ height: "100%" }}>
              <EditOptions />
              <ObjectProperties />
            </Stack>
          </AccordionDetails>
        </Accordion>
      );
    }

    if (datum.panelName == "search") {
      return (
        <Accordion defaultExpanded>
          <AccordionSummary
            expandIcon={<ExpandMoreIcon color="secondary" />}
            aria-controls="panel-properties"
            id="panel-properties"
          >
            <Typography color="secondary">Search</Typography>
          </AccordionSummary>
          <AccordionDetails>
            <RetroSearch></RetroSearch>
          </AccordionDetails>
        </Accordion>
      );
    }

    if (datum.panelName == "logic") {
      return (
        <Accordion defaultExpanded>
          <AccordionSummary
            expandIcon={<ExpandMoreIcon color="action" />}
            aria-controls="panel-logic"
            id="panel-logic"
          >
            <Typography color="action">Logic</Typography>
          </AccordionSummary>
          <AccordionDetails>
            <ObjectLogic />
          </AccordionDetails>
        </Accordion>
      );
    }

    if (datum.panelName == "rights") {
      return (
        <Accordion defaultExpanded>
          <AccordionSummary
            expandIcon={<ExpandMoreIcon color="action" />}
            aria-controls="panel-logic"
            id="panel-logic"
          >
            <Typography color="warning">Rights</Typography>
          </AccordionSummary>
          <AccordionDetails>
            <ObjectRights />
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


export function Home() {

  const panels = useSelector((state: ApplicationState) => state?.panelsStates?.panels);

  var search_result = panels.find(e => e.panelName == "search_result");
  var tree = panels.find(e => e.panelName == "tree");

  var showLeftPannel = tree != null || search_result != null;

  var properties = panels.find(e => e.panelName == "properties");
  var search = panels.find(e => e.panelName == "search");
  var logic = panels.find(e => e.panelName == "logic");
  var rights = panels.find(e => e.panelName == "rights");

  var showRightPannel =
    properties != null ||
    logic != null ||
    rights != null ||
    search != null;

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
