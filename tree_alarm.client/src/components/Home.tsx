import * as React from 'react';
import { useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';

import { TreeControl } from "../tree/TreeControl";
import { MapComponent } from "../map/MapComponent";
import { ObjectProperties } from "../tree/ObjectProperties";
import {
  Box,
  Grid,
} from "@mui/material";


import { RetroSearch } from "../tree/RetroSearch";
import { SearchResult } from "../tree/SearchResult";
import { ObjectRights } from "../rights/ObjectRights";
import { useSelector } from "react-redux";
import { ApplicationState } from "../store";
import { TrackProps } from "../tree/TrackProps";
import { DeepCopy, EPanelType, IPanelsStatesDTO, IPanelTypes } from "../store/Marker";
import { MainToolbar } from "./MainToolbar";
import { AccordionPanels } from "./AccordionPanels";
import DiagramViewer from "../diagrams/DiagramViewer";
import { useAppDispatch } from "../store/configureStore";
import * as GuiStore from "../store/GUIStates";
import * as PanelsStore from "../store/PanelsStates";

import { ActionsControl } from '../actions/ActionsControl';

const LeftPanel = () => {

  const panels = useSelector((state: ApplicationState) => state?.panelsStates?.panels);

  var components: Array<[IPanelsStatesDTO, React.ReactElement]> = panels?.map((datum) => {

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
  if (!panels) {
    return null;
  }

  const panelComponents: Record<string, React.ReactElement> = {
    [IPanelTypes.properties.toString()]: <ObjectProperties />,
    [IPanelTypes.search.toString()]: <RetroSearch />,
    [IPanelTypes.rights.toString()]: <ObjectRights />,
    [IPanelTypes.track_props.toString()]: <TrackProps />,
    [IPanelTypes.actions.toString()]: <ActionsControl />,
  };

  const components: Array<[IPanelsStatesDTO, React.ReactElement]> = panels
    .filter(datum => panelComponents[datum.panelId.toString()]) // ��������� ������������� ����������
    .map(datum => [
      datum,
      React.cloneElement(panelComponents[datum.panelId.toString()], { key: datum.panelId }),
    ]);

  return <AccordionPanels components={components} />;
};



export function Home() {

  const appDispatch = useAppDispatch();
  const [searchParams] = useSearchParams();
  const panels = useSelector((state: ApplicationState) => state?.panelsStates?.panels) ?? null;

  var showLeftPannel = panels?.find(e => e.panelType == EPanelType.Left) != null;
  var showRightPannel = panels?.find(e => e.panelType == EPanelType.Right) != null;
  const diagramDiving = useSelector((state: ApplicationState) => state?.guiStates?.diagramDiving);
  //const diagram = useSelector((state: ApplicationState) => state?.diagramsStates?.cur_diagram);
  //const selected_geo_object = useSelector((state: ApplicationState) => state?.markersStates?.selected_geo_object);

  // Reverse deep-link from vms_rec's "Open in Square" button (CameraUnifiedCard.tsx) —
  // ?selectedId=<id> selects that object and ensures the Properties panel is open to show it, even
  // on a fresh browser with no saved panel layout in localStorage (see PanelsStates.ts).
  useEffect(() => {
    const selectedId = searchParams.get('selectedId');
    if (!selectedId) return;
    appDispatch(GuiStore.selectTreeItem(selectedId));
    const hasPropertiesPanel = panels?.some(
      (p) => p.panelId === IPanelTypes.properties && p.panelType === EPanelType.Right
    );
    if (!hasPropertiesPanel) {
      const newPanels = (DeepCopy(panels) ?? []).filter((p: IPanelsStatesDTO) => p.panelType !== EPanelType.Right);
      newPanels.push({ panelId: IPanelTypes.properties, panelValue: 'Properties', panelType: EPanelType.Right });
      appDispatch(PanelsStore.set_panels(newPanels));
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [searchParams]);

  return (
    <Box sx={{ height: '98vh', display: 'flex', flexDirection: 'column' }}>
      <Box sx={{
        width: '100%'
      }}
      >
        <MainToolbar />
      </Box>

      <Grid container sx={{
        height: '100%',
        width: '100%',
        overflow: 'auto',
        flex: 1
      }}>

        <Grid size={3} sx={{ height: "100%", display: showLeftPannel ? '' : 'none' }}>
          <LeftPanel />
        </Grid>

        <Grid size="grow" sx={{ minWidth: '100px', minHeight: '100px', height: '100%' }}>
          {diagramDiving ? <DiagramViewer /> : <MapComponent />}
        </Grid>

        <Grid size={3} sx={{ height: "100%", display: showRightPannel ? '' : 'none' }}>
          <RightPanel />
        </Grid>
      </Grid>

    </Box>
  );
}
