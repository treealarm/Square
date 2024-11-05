import * as React from 'react';
import ScubaDivingIcon from '@mui/icons-material/ScubaDiving';
import PoolIcon from '@mui/icons-material/Pool';

import { ApplicationState } from '../store';
import { useSelector } from 'react-redux';
import { getExtraProp } from '../store/Marker';

import * as DiagramsStore from '../store/DiagramsStates';
import { useAppDispatch } from '../store/configureStore';
import { useCallback } from 'react';
import { Box, IconButton, Tooltip } from '@mui/material';
import DiagramParentsNavigator from './DiagramParentsNavigator';

export default function DiagramNavigation() {

  const objProps = useSelector((state: ApplicationState) => state?.objPropsStates?.objProps);
  const diagram = useSelector((state: ApplicationState) => state?.diagramsStates.cur_diagram_content);

  var __is_diagram = getExtraProp(objProps, "__is_diagram", "0");

  const appDispatch = useAppDispatch();

  const setDiagram = useCallback(
    (diagram_id: string) => {
      if (diagram_id == null) {
        appDispatch<any>(DiagramsStore.reset_diagram_contentreset_diagram_content());
        return;
      }
      appDispatch<any>(DiagramsStore.fetchGetDiagramContent(diagram_id));
    }, [ appDispatch]);

  const Resurface = useCallback(
    () => {
      appDispatch<any>(DiagramsStore.reset_diagram_contentreset_diagram_content());
    }, [ appDispatch]);

  if (__is_diagram != '1' && diagram?.parent == null) {
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
        diagram?.parent == null ?
          <React.Fragment /> :
          <Tooltip title={"Resurface from the diagram"}>
            <IconButton aria-label="search" size="medium" onClick={() => Resurface()}>
              <PoolIcon fontSize="inherit" />
            </IconButton>
          </Tooltip>
      }

      <DiagramParentsNavigator parent_list={diagram?.parents} parent_id={diagram?.parent?.id} />

      <Tooltip title={"Dive into the diagram"}>
        <IconButton aria-label="search" size="medium" onClick={() => setDiagram(objProps?.id)}>
          <ScubaDivingIcon fontSize="inherit" />
        </IconButton>
      </Tooltip>
    </Box>
  );
}