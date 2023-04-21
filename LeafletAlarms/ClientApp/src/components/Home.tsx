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
        <Accordion>
          <AccordionSummary
            expandIcon={<ExpandMoreIcon color="primary" />}
            aria-controls="panel-tree"
            id="panel-tree">
            <Typography color="primary">Tree</Typography>
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
        <Accordion>
          <AccordionSummary
            expandIcon={<ExpandMoreIcon color="secondary" />}
            aria-controls="panel-search-result"
            id="panel-search-result">
            <Typography color="secondary">Search result</Typography>
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


export function Home() {

  const [showPannel, setShowPannel] = React.useState(true);

  const handleShowPannel = (event: React.ChangeEvent<HTMLInputElement>) => {
    setShowPannel(event.target.checked);
  };
  const panels = useSelector((state: ApplicationState) => state?.panelsStates?.panels);

  var search_result = panels.find(e => e.panelName == "search_result");
  var tree = panels.find(e => e.panelName == "tree");

  var showLeftPannel = tree != null || search_result != null;

  return (
    <Grid container spacing={1} sx={{ height: "100%", p: "1px" }}>
      <Grid item xs={12} sx={{ height: "auto" }}>
        <Box sx={{ flexGrow: 1 }}>
          <AppBar position="static" sx={{ backgroundColor: '#aaaaaa' }} >
        <Toolbar>
          
          <FormGroup row>
                <PanelSwitch/>
            <FormControlLabel control={
              <Checkbox
                checked={showPannel}
                id="CheckBoxShowPannel"
                onChange={handleShowPannel}
                size="small"
                tabIndex={-1}
                disableRipple
              />}
                  label="Show panels"
                />
              
          </FormGroup>

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

      <Grid item xs={3} sx={{ height: "90%", display: showPannel ? '' : 'none' }}
        container spacing={0}>
        <Paper sx={{ maxHeight: "100%", overflow: 'auto', width: "100%" }}>
          <Accordion>
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

          <Accordion>
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

          <Accordion>
            <AccordionSummary
              expandIcon={<ExpandMoreIcon color="action" />}
              aria-controls="panel-logic"
              id="panel-logic"
            >
              <Typography color="action">Logic</Typography>
            </AccordionSummary>
            <AccordionDetails>
              <ObjectLogic/>
            </AccordionDetails>
          </Accordion>

          <Accordion>
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

        </Paper>

      </Grid>
    </Grid>
  );
}
