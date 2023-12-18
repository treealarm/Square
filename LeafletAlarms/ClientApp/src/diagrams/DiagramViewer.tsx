import * as React from 'react';
import ScubaDivingIcon from '@mui/icons-material/ScubaDiving';
import { ApplicationState } from '../store';
import { useSelector } from 'react-redux';
import { getExtraProp } from '../store/Marker';

import * as DiagramsStore from '../store/DiagramsStates';
import { useAppDispatch } from '../store/configureStore';
import { useCallback } from 'react';
import { Box, IconButton, Toolbar, Tooltip } from '@mui/material';
import * as GuiStore from '../store/GUIStates';

export default function DiagramViewer() {

  const objProps = useSelector((state: ApplicationState) => state?.objPropsStates?.objProps);
  const diagram = useSelector((state: ApplicationState) => state?.diagramsStates.cur_diagram);

  var paper_width = getExtraProp(diagram?.container_diagram, "__paper_width", '1000'); 
  var paper_height = getExtraProp(diagram?.container_diagram, "__paper_height", '1000'); 
  //var __is_diagram = getExtraProp(objProps, "__is_diagram", "0");

  const appDispatch = useAppDispatch();
  //const setDiagram = useCallback(
  //  (diagram_id: string) => {
  //    appDispatch<any>(DiagramsStore.fetchDiagram(diagram_id));
  //  }, [objProps]);


  const selectItem = useCallback(
    (diagram_id: string) => {
      //appDispatch<any>(DiagramsStore.reset_diagram(null));
      appDispatch<any>(GuiStore.actionCreators.selectTreeItem(diagram_id));
    }, [objProps]);

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
        overflowX: "scroll", // added scroll
        overflowY: "scroll" // added scroll
      }}
    >
      <Box
        sx={{// Paper
          position: 'absolute',
          border: 1,
          top: '0px', // Сдвиг от верхнего края
          left: '0px', // Сдвиг от левого края
          height: paper_width + 'px',
          width: paper_width + 'px',
          backgroundColor: 'yellow',
        }}>

        {
          diagram?.content.map((diagram, index) =>

            <Box onClick={() => { selectItem(diagram?.id) }}
              sx={{// Main object
                position: 'absolute',
                border: 1,
                top: diagram.geometry.top + '%', // Сдвиг от верхнего края
                left: diagram.geometry.left + '%', // Сдвиг от левого края
                height: diagram.geometry.height + '%',
                width: diagram.geometry.width + '%',
                backgroundColor: 'transparent',
                '&:hover': {
                  cursor: 'pointer'
                }
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
                  height: '7.5%',
                  backgroundColor: 'transparent'
                }}
              >
                <img src="svg/cisco.svg"
                  alt="SVG Image"
                  style={{ width: '100%', height: '100%', objectFit: 'fill' }}
                />
              </Box>

              <Box
                sx={{
                  position: 'absolute',
                  border: 1,
                  top: '21%', // Сдвиг от верхнего края
                  left: '15.5%', // Сдвиг от левого края
                  right: '4.5%',
                  height: '7.5%',
                  backgroundColor: 'transparent'
                }}
              >
                <img src="svg/cisco.svg"
                  alt="SVG Image"
                  style={{ width: '100%', height: '100%', objectFit: 'fill' }}
                />
              </Box>

            </Box>
       ) }
        
      </Box>
    </Box>
  );
}