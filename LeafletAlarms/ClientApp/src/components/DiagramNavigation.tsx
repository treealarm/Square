import * as React from 'react';
import ScubaDivingIcon from '@mui/icons-material/ScubaDiving';
import { ApplicationState } from '../store';
import { useSelector } from 'react-redux';
import { getExtraProp } from '../store/Marker';

import * as DiagramsStore from '../store/DiagramsStates';
import { useAppDispatch } from '../store/configureStore';
import { useCallback } from 'react';
import { Box, IconButton, Toolbar, Tooltip } from '@mui/material';

export default function FixedBottomNavigation() {

  const objProps = useSelector((state: ApplicationState) => state?.objPropsStates?.objProps);

  var __is_diagram = getExtraProp(objProps, "__is_diagram", "0");

  const appDispatch = useAppDispatch();
  const setDiagram = useCallback(
    (diagram_id:string) => {
      appDispatch<any>(DiagramsStore.fetchDiagram(diagram_id));
    }, [objProps]);

  if (__is_diagram != '1') {
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

    </Box>
  );
}