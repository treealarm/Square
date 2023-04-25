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
import { DeepCopy, IPanelsStatesDTO, IPanelTypes } from "../store/Marker";
import { MainToolbar } from "./MainToolbar";
import CloseIcon from "@mui/icons-material/Close";
import { useAppDispatch } from "../store/configureStore";

import DataObjectIcon from '@mui/icons-material/DataObject';
import SearchIcon from '@mui/icons-material/Search';
import SummarizeIcon from '@mui/icons-material/Summarize';

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

  var firstComponent = components[0];

  if (firstComponent[0].IsLeft) {

  }

  var properties: IPanelsStatesDTO = null;
  var search: IPanelsStatesDTO = null;
  var track_props: IPanelsStatesDTO = null;

  components.map((component) => {

    if (component[0].panelId == IPanelTypes.properties) {
      properties = component[0];
    }

    if (component[0].panelId == IPanelTypes.search) {
      search = component[0];
    }

    if (component[0].panelId == IPanelTypes.track_props) {
      track_props = component[0];
    }
  });

  const showToggleButtons =
    properties != null || search != null || track_props != null;

  const handleClose = (panelId: string) => {

      var removed = panels.filter(e => e.panelId != panelId);
      appDispatch(PanelsStore.set_panels(removed));
  };

  const handleSelectRight = (panelId: string, text: string) => {

    var exist = panels.find(e => e.panelId == panelId);

    if (exist) {
      var removed = panels.filter(e => e.panelId != panelId);
      appDispatch(PanelsStore.set_panels(removed));
    }
    else {
      var newPanels = DeepCopy(panels);

      newPanels = newPanels.filter(e => e.IsLeft != false);
      newPanels.push(
        {
          panelId: panelId,
          panelValue: text,
          IsLeft: false
        });

      appDispatch(PanelsStore.set_panels(newPanels));
    }
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

                <ToggleButton
                  value="check"
                  aria-label="properties"
                  selected={properties != null}
                  size="small"
                  onChange={() =>
                    handleSelectRight(IPanelTypes.properties, IPanelTypes.propertiesName)
                  }
                  >
                  <DataObjectIcon />
                </ToggleButton>

                <ToggleButton
                  value="check"
                  aria-label="search"
                  selected={search != null}
                  size="small"
                  onChange={() => 
                    handleSelectRight(IPanelTypes.search, IPanelTypes.searchName)
                  }
                  >
                  <SearchIcon />
                </ToggleButton>

                <ToggleButton
                  value="check"
                  aria-label="track_props"
                  selected={track_props != null}
                  size="small"
                  onChange={() => handleSelectRight(IPanelTypes.track_props, IPanelTypes.track_propsName)
                  }>
                  <SummarizeIcon />
                </ToggleButton>
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
