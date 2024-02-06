import * as React from 'react';
import { useCallback, useEffect, WheelEvent } from 'react';
import { ApplicationState } from '../store';
import { useSelector } from 'react-redux';
import { getExtraProp, IDiagramCoord, IDiagramTypeDTO } from '../store/Marker';

import { useAppDispatch } from '../store/configureStore';
import * as DiagramTypeStore from '../store/DiagramTypeStates';
import { useState } from 'react';
import { Box, ButtonGroup, IconButton, FormControl } from '@mui/material';
import * as GuiStore from '../store/GUIStates';
import ZoomInIcon from '@mui/icons-material/ZoomIn';
import ZoomOutIcon from '@mui/icons-material/ZoomOut';
import RestartAltIcon from '@mui/icons-material/RestartAlt';
import * as MarkersVisualStore from '../store/MarkersVisualStates';
import Select, { SelectChangeEvent } from '@mui/material/Select';
import MenuItem from '@mui/material/MenuItem';

export default function DiagramTypeViewer() {

  const appDispatch = useAppDispatch();
  const [zoom, setZoom] = useState(1.0);

  const diagramType = useSelector((state: ApplicationState) => state?.diagramtypeStates?.cur_diagramtype);
 

  var paper_width = 1000;
  var paper_height = 1000;

  useEffect(
    () => {
      appDispatch<any>(DiagramTypeStore.fetchDiagramTypeByName(""));
    }, []);


  const handleChange = (event: SelectChangeEvent) => {

  };

  const handleClick = (e: React.MouseEvent<HTMLDivElement>) => {

  };



  if (diagramType == null) {
    return null;
  }


  return (
    <Box
      key={"box top"}
      sx={{
        width: '100%',
        height: '100%',
        display: 'flex',
        flexDirection: 'column',
        border: 0,
        padding: 0,
        margin: 0,
        position: 'relative',
      }}
    >
      <Box
        key={"box transparent"}
        sx={{
          width: '100%',
          height: '100%',
          display: 'flex',
          flexDirection: 'column',
          backgroundColor: 'gray',
          border: 1,
          padding: 0,
          margin: 0,
          position: 'relative',
          overflow: 'auto'
        }}
      >
        <Box
          key={"box yellow"}
          onClick={handleClick}
          sx={{// Paper
            position: 'absolute',
            border: 1,
            padding: 0,
            margin: 0,
            top: '0px', // Сдвиг от верхнего края
            left: '65px', // Сдвиг от левого края
            height: paper_height * zoom + 'px',
            width: paper_width * zoom + 'px',
            backgroundColor: 'yellow',
          }}>


        </Box>
      </Box>

      <ButtonGroup variant="contained" orientation="vertical"
        sx={{ position: 'absolute', left: '5px', backgroundColor: 'lightgray' }}>
        <IconButton
          onClick={(e: any) => setZoom(zoom + 0.1)}>
          <ZoomInIcon fontSize="inherit"></ZoomInIcon>
        </IconButton>

        <IconButton
          onClick={(e: any) => setZoom(zoom - 0.1)}>
          <ZoomOutIcon fontSize="inherit" />
        </IconButton>
        <IconButton
          onClick={(e: any) => setZoom(1)}>
          <RestartAltIcon fontSize="inherit" />
        </IconButton>

      </ButtonGroup>

    </Box>
  );
}