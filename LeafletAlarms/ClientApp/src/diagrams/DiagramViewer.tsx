import * as React from 'react';
import { WheelEvent, WheelEventHandler } from 'react';
import ScubaDivingIcon from '@mui/icons-material/ScubaDiving';
import { ApplicationState } from '../store';
import { useSelector } from 'react-redux';
import { getExtraProp } from '../store/Marker';

import * as DiagramsStore from '../store/DiagramsStates';
import { useAppDispatch } from '../store/configureStore';
import { useCallback, useState } from 'react';
import { Box, ButtonGroup, IconButton, SpeedDial, SpeedDialAction, SpeedDialIcon, Toolbar, Tooltip } from '@mui/material';
import * as GuiStore from '../store/GUIStates';
import ZoomInIcon from '@mui/icons-material/ZoomIn';
import ZoomOutIcon from '@mui/icons-material/ZoomOut';
import RestartAltIcon from '@mui/icons-material/RestartAlt';
export default function DiagramViewer() {

  const [zoom, setZoom] = useState(1.0);

  const objProps = useSelector((state: ApplicationState) => state?.objPropsStates?.objProps);
  const diagram = useSelector((state: ApplicationState) => state?.diagramsStates.cur_diagram);

  var paper_width =
    parseFloat(
      getExtraProp(diagram?.container_diagram, "__paper_width", '1000'));
  var paper_height = parseFloat(
    getExtraProp(diagram?.container_diagram, "__paper_height", '1000'));
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

  const handleWheelEvent = (e: WheelEvent) => {
    e.preventDefault();
    var newZ = zoom + ((e.deltaY) / Math.abs(e.deltaY)) * -0.1
    setZoom(newZ);
    console.log("ZOOM", newZ)
  };

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
        border: 0,
        padding: 0,
        margin: 0,
        position: 'relative',
      }}
    >
      <Box
        sx={{
          width: '100%',
          height: '100%',
          display: 'flex',
          flexDirection: 'column',
          backgroundColor: 'gray',
          border: 0,
          padding: 0,
          margin: 0,
          position: 'relative',
          overflow: 'auto'
        }}
      >
        <Box
          onWheel={handleWheelEvent}
          sx={{// Paper
            position: 'absolute',
            border: 0,
            padding: 0,
            margin: 0,
            top: '0px', // Сдвиг от верхнего края
            left: '0px', // Сдвиг от левого края
            height: paper_height * zoom + 'px',
            width: paper_width * zoom + 'px',
            backgroundColor: 'yellow',
          }}>

          {
            diagram?.content.map((diagram, index) =>
              <React.Fragment>


                <Box onClick={() => { selectItem(diagram?.id) }}
                  sx={{// Main object
                    border: 0,
                    padding: 0,
                    margin: 0,
                    position: 'absolute',
                    
                    top: diagram.geometry.top * zoom + 'px', // Сдвиг от верхнего края
                    left: diagram.geometry.left * zoom + 'px', // Сдвиг от левого края
                    height: diagram.geometry.height * zoom + 'px',
                    width: diagram.geometry.width * zoom + 'px',
                    backgroundColor: 'transparent',
                    '&:hover': {
                      cursor: 'pointer'
                    },
                    display: 'flex', // Добавляем свойство display и flex-direction
                    flexDirection: 'column',
                  }}
                >
                  <Box
                    sx={{
                      border: 1,
                      padding: 0,
                      margin: 0,
                      position: 'absolute',

                      top: 22 * zoom + 'px', // Сдвиг от верхнего края
                      left: 32 * zoom + 'px', // Сдвиг от левого края
                      height: 40 * zoom + 'px',
                      width: 158 * zoom + 'px',
                      backgroundColor: 'transparent',
                      display: 'flex', // Добавляем свойство display и flex-direction
                      flexDirection: 'column',
                    }}
                  >
                    {
                      <img src="svg/cisco.svg"
                        style={{
                          border: 0,
                          padding: 0,
                          margin: 0,
                          width: '100%',
                          height: '100%',
                          objectFit: 'fill', // Заполняет SVG без сохранения пропорций
                        }} />
                    }

                  </Box>

                  <img src="svg/rack.svg"
                    style={{
                      border: 0, 
                      padding: 0,
                      margin: 0,
                      width: '100%',
                      height: '100%',
                      objectFit: 'fill', // Заполняет SVG без сохранения пропорций
                    }} />

                </Box>
                
              </React.Fragment>
            )}

        </Box>
      </Box>

      <ButtonGroup variant="contained" orientation="vertical"
        sx={{ position: 'absolute', left: '10px', backgroundColor: 'lightgray' }}>
        <IconButton
          size="small"
          onClick={(e: any) => setZoom(zoom + 0.1)}>
          <ZoomInIcon fontSize="inherit"></ZoomInIcon>
        </IconButton>

        <IconButton
          size="small"
          onClick={(e: any) => setZoom(zoom - 0.1)}>
          <ZoomOutIcon fontSize="inherit" />
        </IconButton>
        <IconButton
          size="small"
          onClick={(e: any) => setZoom(1)}>
          <RestartAltIcon fontSize="inherit" />
        </IconButton>

      </ButtonGroup>

    </Box>
  );
}