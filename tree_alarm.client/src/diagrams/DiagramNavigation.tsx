/* eslint-disable react-hooks/exhaustive-deps */
import * as React from 'react';
import ScubaDivingIcon from '@mui/icons-material/ScubaDiving';
import PoolIcon from '@mui/icons-material/Pool';

import { ApplicationState } from '../store';
import { useSelector } from 'react-redux';
import { IDiagramDTO } from '../store/Marker';

import * as DiagramsStore from '../store/DiagramsStates';
import { useAppDispatch } from '../store/configureStore';
import { useCallback } from 'react';
import { Box, IconButton, Tooltip } from '@mui/material';
import DiagramParentsNavigator from './DiagramParentsNavigator';

export default function DiagramNavigation() {

  const objProps = useSelector((state: ApplicationState) => state?.objPropsStates?.objProps);
  const diagram_content = useSelector((state: ApplicationState) => state?.diagramsStates?.cur_diagram_content);
  const cur_diagram: IDiagramDTO | null = useSelector((state: ApplicationState) => state?.diagramsStates?.cur_diagram ?? null);

  var parent_props = diagram_content?.content_props?.find(i => i.id == cur_diagram?.id);
  var parent_diagram = diagram_content?.content?.find(e => e.id == parent_props?.parent_id) ?? null;

  const appDispatch = useAppDispatch();

  const setDiagram = useCallback(
    (diagram_id: string|null) => {
      if (diagram_id == null) {
        appDispatch<any>(DiagramsStore.update_single_diagram_locally(null));
        return;
      }
      appDispatch<any>(DiagramsStore.fetchGetDiagramContent(diagram_id));
    }, [ ]);

  const Resurface = useCallback(
    () => {
      appDispatch<any>(DiagramsStore.update_single_diagram_locally(null));
    }, [ ]);

  if (!cur_diagram) {
    return null;
  }

  return (
    <Box
      sx={{
        backgroundColor: '#bbbbbb',
        flexDirection: 'row',
        justifyContent: 'flex-end',
        display: 'flex'
      }}
    >
      <React.Fragment />
      {
        parent_diagram == null ?
          <React.Fragment /> :
          <Tooltip title={"Resurface from the diagram"}>
            <IconButton aria-label="search" size="medium" onClick={() => Resurface()}>
              <PoolIcon fontSize="inherit" />
            </IconButton>
          </Tooltip>
      }

      <DiagramParentsNavigator parent_list={diagram_content?.parents ?? null} parent_id={parent_diagram?.id ?? null} />

      <Tooltip title={"Dive into the diagram"}>
        <IconButton aria-label="search" size="medium" onClick={() => setDiagram(objProps?.id ??null)}>
          <ScubaDivingIcon fontSize="inherit" />
        </IconButton>
      </Tooltip>
    </Box>
  );
}