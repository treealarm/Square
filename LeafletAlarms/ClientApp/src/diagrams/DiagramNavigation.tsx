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

export default function FixedBottomNavigation() {

  const objProps = useSelector((state: ApplicationState) => state?.objPropsStates?.objProps);
  const diagram = useSelector((state: ApplicationState) => state?.diagramsStates.cur_diagram);

  var __is_diagram = getExtraProp(objProps, "__is_diagram", "0");

  const appDispatch = useAppDispatch();

  const setDiagram = useCallback(
    (diagram_id: string) => {
      if (diagram_id == null) {
        appDispatch<any>(DiagramsStore.reset_diagram());
        return;
      }
      appDispatch<any>(DiagramsStore.fetchDiagram(diagram_id));
    }, [objProps, diagram]);

  const Resurface = useCallback(
    () => {
      if (objProps?.parent_id == null) {
        appDispatch<any>(DiagramsStore.reset_diagram());
        return;
      }
      appDispatch<any>(DiagramsStore.fetchDiagram(objProps?.parent_id));
    }, [objProps, diagram]);

  if (__is_diagram != '1' && diagram?.parent_id == null) {
    return null;
  }

  return (
    <Box
      sx={{ backgroundColor: '#bbbbbb' }}
    >
      <Tooltip title={"Dive into the diagram"}>
        <IconButton aria-label="search" size="medium" onClick={(e: any) => setDiagram(objProps?.id)}>
          <ScubaDivingIcon fontSize="inherit" />
        </IconButton>
      </Tooltip>

      {
        diagram?.parent_id == null ?
          <React.Fragment /> :
      <Tooltip title={"Resurface from the diagram"}>
            <IconButton aria-label="search" size="medium" onClick={(e: any) => Resurface()}>
          <PoolIcon fontSize="inherit" />
        </IconButton>
      </Tooltip>
      }
      

    </Box>
  );
}