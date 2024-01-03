import * as React from 'react';
import { WheelEvent } from 'react';
import { ApplicationState } from '../store';
import { useSelector } from 'react-redux';
import { getExtraProp } from '../store/Marker';

import { useAppDispatch } from '../store/configureStore';
import {  useState } from 'react';
import { Box, ButtonGroup, IconButton} from '@mui/material';
import * as GuiStore from '../store/GUIStates';
import ZoomInIcon from '@mui/icons-material/ZoomIn';
import ZoomOutIcon from '@mui/icons-material/ZoomOut';
import RestartAltIcon from '@mui/icons-material/RestartAlt';
import DiagramElement from './DiagramElement';
import DiagramGray from './DiagramGray';
export default function DiagramViewer() {

  const [zoom, setZoom] = useState(1.0);

  const diagram = useSelector((state: ApplicationState) => state?.diagramsStates.cur_diagram);
  var parent = diagram.content.find(e => e.id == diagram.parent_id);

  var paper_width =
    parseFloat(
      getExtraProp(parent, "__paper_width", '1000'));
  var paper_height = parseFloat(
    getExtraProp(parent, "__paper_height", '1000'));
  //var __is_diagram = getExtraProp(objProps, "__is_diagram", "0");

  const appDispatch = useAppDispatch();


  const handleClick = (e: React.MouseEvent<HTMLDivElement>) => {
    appDispatch<any>(GuiStore.actionCreators.selectTreeItem(null));
  };

  const handleWheelEvent = (e: WheelEvent) => {
    e.preventDefault();
    e.stopPropagation();
    var newZ = zoom + ((e.deltaY) / Math.abs(e.deltaY)) * -0.1
    setZoom(newZ);
    console.log("ZOOM", newZ)
  };

  function handleScrollCapture(event:any) {
    event.stopPropagation(); // Это предотвратит дальнейшее распространение события прокрутки
    event.preventDefault();
    console.log('Scroll event captured!');
    // Ваша логика обработки события прокрутки
  }

  if (diagram == null) {
    return null;
  }
  var content = diagram.content.filter(e => e.parent_id == diagram.parent_id);
  

  return (
    <Box
      onWheel={handleWheelEvent}
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
          border: 0,
          padding: 0,
          margin: 0,
          position: 'relative',
          overflow: 'auto'
        }}
      >
        <Box
          key={ "box yellow" }
          onClick={handleClick}
          sx={{// Paper
            position: 'absolute',
            border: 0,
            padding: 0,
            margin: 0,
            top: '0px', // Сдвиг от верхнего края
            left: '35px', // Сдвиг от левого края
            height: paper_height * zoom + 'px',
            width: paper_width * zoom + 'px',
            backgroundColor: 'yellow',
          }}>
          <DiagramGray diagram={parent} zoom={zoom} />

          {
            content.map((dgr, index) =>
              <DiagramElement diagram={dgr} parent={parent} zoom={zoom} z_index={2} key={ index } />
            )}

          
        </Box>
      </Box>

      <ButtonGroup variant="contained" orientation="vertical"
        sx={{ position: 'absolute', left: '5px', backgroundColor: 'lightgray' }}>
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