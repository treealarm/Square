import * as React from 'react';
import ScubaDivingIcon from '@mui/icons-material/ScubaDiving';
import { ApplicationState } from '../store';
import { useSelector } from 'react-redux';
import { getExtraProp } from '../store/Marker';

import * as DiagramsStore from '../store/DiagramsStates';
import { useAppDispatch } from '../store/configureStore';
import { useCallback } from 'react';
import { Box, IconButton, Toolbar, Tooltip } from '@mui/material';


export default function DiagramViewer() {

  //const objProps = useSelector((state: ApplicationState) => state?.objPropsStates?.objProps);
  const diagram = useSelector((state: ApplicationState) => state?.diagramsStates.cur_diagram);

  //var __is_diagram = getExtraProp(objProps, "__is_diagram", "0");

  //const appDispatch = useAppDispatch();
  //const setDiagram = useCallback(
  //  (diagram_id: string) => {
  //    appDispatch<any>(DiagramsStore.fetchDiagram(diagram_id));
  //  }, [objProps]);

  if (diagram == null) {
    return null;
  }

 
  return (
    <Box
      sx={{
        width: '100%',
        height: '100%',
        display: 'flex',
        flexDirection: 'column',
        backgroundColor: '#ffaaaa',
        border: 1,
        position: 'relative',
        overflowY: "scroll" // added scroll
      }}
    >
      <Box
        sx={{
          position: 'absolute',
          border: 1,
          top: '10px', // Сдвиг от верхнего края
          left: '10px', // Сдвиг от левого края
          height: '600px',
          width: '400px',
          backgroundColor: 'transparent',
        }}
      >
        <img src="svg/rack.svg"
          style={{
            width: '100%',
            height: '100%',
            objectFit: 'fill', // Заполняет SVG без сохранения пропорций
          }} />

        <Box
          sx={{
            position: 'absolute',
            border: 1,
            top: '13%', // Сдвиг от верхнего края
            left: '15.5%', // Сдвиг от левого края
            right: '4.5%',
            height:'7.5%',
            backgroundColor: 'transparent'
          }}
        >
          <img src="svg/cisco.svg"
            alt="SVG Image"
            style={{ width: '100%', height: '100%', objectFit: 'fill' }}
          />
        </Box>
      </Box>
    </Box>
  );
}