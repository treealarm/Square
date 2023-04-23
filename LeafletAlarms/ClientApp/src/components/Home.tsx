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


const AccordionPanels = (props:{ components: Array<[IPanelsStatesDTO, JSX.Element]>}) => {

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
      <Accordion defaultExpanded sx={{ backgroundColor: color }}>
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


export function Home() {

  const panels = useSelector((state: ApplicationState) => state?.panelsStates?.panels);

  var showLeftPannel = panels.find(e => e.IsLeft) != null;
  var showRightPannel = panels.find(e => e.IsLeft == false) != null;

  return (
    <Grid container spacing={1} sx={{ height: "100%", p: "1px" }}>

      <Grid item xs={12} sx={{ height: "10%" }}>
        <Box sx={{ flexGrow: 1}}>
          <AppBar sx={{ backgroundColor: '#bbbbbb' }} >
            <Toolbar variant='dense' >
              <Box
                sx={{ flexGrow: 1 }}                
                display="flex"
                justifyContent="flex-start"                
              >
                <PanelSwitch IsLeftPanel={true} />
                <WebSockClient />
                
              </Box>
              <Box
                sx={{ flexGrow: 1 }}
                display="flex"
                justifyContent="flex-center"
              >
                <GlobalLayersOptions />
                
              </Box>
              <Box
                display="flex"
                justifyContent="flex-end"
                alignContent="center"
                sx={{ border: 1 }}
              >
                <Box sx={{m: 2, p: 2, border:1 }}><Login /></Box>
                
                <PanelSwitch IsLeftPanel={false} />
              </Box>
              
            </Toolbar>
          </AppBar>
        </Box>
      </Grid>

      <Grid item xs={2} sx={{ height: "90%", display: showLeftPannel ? '' : 'none' }}
        spacing={0}>
        <Paper sx={{ maxHeight: "100%", overflow: 'auto', width: "100%" }} >
          <LeftPanel/>
        </Paper>

      </Grid>

      <Grid item xs sx={{ minWidth: "100px", height: "90%", flexGrow: 1 }}
        spacing={0}>
        <Box sx={{ flexGrow: 1, height:'100%'}}>
          <MapComponent />
        </Box>
        
      </Grid>

      <Grid item xs={3} sx={{ height: "90%", display: showRightPannel ? '' : 'none' }}
        spacing={0}>
        <Paper sx={{ maxHeight: "100%", overflow: 'auto', width: "100%" }}>
          <RightPanel/>
        </Paper>

      </Grid>
    </Grid>
  );
}
