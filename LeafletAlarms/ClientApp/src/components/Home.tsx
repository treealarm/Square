import * as React from "react";
import { TreeControl } from "../tree/TreeControl";
import { MapComponent } from "../map/MapComponent";
import { ObjectProperties } from "../tree/ObjectProperties";
import {
  Box,
  Grid, Toolbar,
} from "@mui/material";


import { RetroSearch } from "../tree/RetroSearch";
import { SearchResult } from "../tree/SearchResult";
import { ObjectRights } from "../rights/ObjectRights";
import { useSelector } from "react-redux";
import { ApplicationState } from "../store";
import { TrackProps } from "../tree/TrackProps";
import { EPanelType, IPanelsStatesDTO, IPanelTypes } from "../store/Marker";
import { MainToolbar } from "./MainToolbar";
import { AccordionPanels } from "./AccordionPanels";
import DiagramViewer from "../diagrams/DiagramViewer";
import { ObjectPropertiesUpdater } from "./ObjectPropertiesUpdater";


const LeftPanel = () => {

  const panels = useSelector((state: ApplicationState) => state?.panelsStates?.panels);

  var components: Array<[IPanelsStatesDTO, JSX.Element]> = panels.map((datum) => {

    if (datum.panelId == IPanelTypes.tree) {
      return [datum, (
        <TreeControl key={"TreeControl" + datum.panelId} />
      )];
    }

    if (datum.panelId == IPanelTypes.search_result) {
      return [datum, (
        <SearchResult key={"SearchResult" + datum.panelId} />
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
        <RetroSearch />
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

  var showLeftPannel = panels.find(e => e.panelType == EPanelType.Left) != null;
  var showRightPannel = panels.find(e => e.panelType == EPanelType.Right) != null;
  const diagram = useSelector((state: ApplicationState) => state?.diagramsStates.cur_diagram);

  return (
    <Box sx={{ height: '98vh', display: 'flex', flexDirection: 'column' }}>
      <Box sx={{
        width: '100%'
      }}
      >
        <ObjectPropertiesUpdater />
        <MainToolbar />
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
          {diagram == null ? <MapComponent /> : <DiagramViewer />}
        </Grid>

        <Grid item xs={3} sx={{ height: "100%", display: showRightPannel ? '' : 'none' }}>
          <RightPanel />
        </Grid>
      </Grid>

    </Box>
  );
}
