import * as React from 'react';
import { useCallback, useEffect, WheelEvent } from 'react';
import { ApplicationState } from '../store';
import { useSelector } from 'react-redux';
import { getExtraProp, IDiagramCoord, IDiagramDTO, IGetDiagramDTO } from '../store/Marker';

import { useAppDispatch } from '../store/configureStore';
import * as DiagramsStore from '../store/DiagramsStates';
import { useState } from 'react';
import { Box, ButtonGroup, IconButton } from '@mui/material';
import * as GuiStore from '../store/GUIStates';
import ZoomInIcon from '@mui/icons-material/ZoomIn';
import ZoomOutIcon from '@mui/icons-material/ZoomOut';
import RestartAltIcon from '@mui/icons-material/RestartAlt';
import DiagramElement from './DiagramElement';
import DiagramGray from './DiagramGray';
import * as MarkersVisualStore from '../store/MarkersVisualStates';
import Select, { SelectChangeEvent } from '@mui/material/Select';
import MenuItem from '@mui/material/MenuItem';

export default function DiagramViewer() {

  const appDispatch = useAppDispatch();
  const [zoom, setZoom] = useState(1.0);

  const diagram: IGetDiagramDTO = useSelector((state: ApplicationState) => state?.diagramsStates.cur_diagram);
  const depth = useSelector((state: ApplicationState) => state?.diagramsStates.depth);
  const visualStates = useSelector((state: ApplicationState) => state?.markersVisualStates?.visualStates);
  const alarmedObjects = useSelector((state: ApplicationState) => state?.markersVisualStates?.visualStates?.alarmed_objects);
  const selected_id = useSelector((state: ApplicationState) => state?.guiStates?.selected_id);


  const [currentPaperProps, setCurrentPaperProps] = useState({
    paper_width: 0,
    paper_height: 0,
    background_img: null
  });

  useEffect(
    () => {
      var paper_width =
        parseFloat(
          getExtraProp(diagram?.parent, "__paper_width", '1000'));
      var paper_height = parseFloat(
        getExtraProp(diagram?.parent, "__paper_height", '1000'));

      var background_img = getExtraProp(diagram?.parent, "__background_img", null);

      var curPaperProps =
      {
        paper_width: paper_width,
        paper_height: paper_height,
        background_img: background_img
      }
      setCurrentPaperProps(curPaperProps);

    }, [diagram?.parent]);

  useEffect(
    () => {
      if (diagram?.content == null) {
        return;
      }
      var objArray2: string[] = [];
      diagram?.content?.forEach(arr => objArray2.push(arr.id));
      appDispatch<any>(MarkersVisualStore.requestMarkersVisualStates(objArray2));
    }, [diagram?.content]);



  function hexToRgba(hex: string, alpha: number): string {

    if (hex == null) {
      return null;
    }
    const hexColor = hex.replace('#', '');
    const r = parseInt(hexColor.substring(0, 2), 16);
    const g = parseInt(hexColor.substring(2, 4), 16);
    const b = parseInt(hexColor.substring(4, 6), 16);

    return `rgba(${r}, ${g}, ${b}, ${alpha})`;
  }

  const getColor = useCallback(
    (marker: IDiagramDTO) => {
      var id = marker.id;

      var retColor: any = null;

      if (selected_id == id) {
        //retColor.dashArray = '5,10';
      }

      {
        var vState = visualStates.states.find(i => i.id == id);

        if (vState != null && vState.states.length > 0) {
          var vStateFirst = vState.states[0];
          var vStateDescr = visualStates.states_descr.find(s => s.state == vStateFirst);
          if (vStateDescr != null) {
            retColor = vStateDescr.state_color;
          }
        }
      }

      var vAlarmState = alarmedObjects.find(i => i.id == id);

      if (vAlarmState != null
        && (vAlarmState.alarm)) {

        retColor = '#ff0000';
      }
      else {
        var color = getExtraProp(marker, "__color");

        if (color != null) {
          retColor = color;
        }
      }

      return retColor;

    }, [visualStates, alarmedObjects, selected_id]);



  const handleChange = (event: SelectChangeEvent) => {
    appDispatch<any>(DiagramsStore.set_depth(event.target.value as any as number));
    appDispatch<any>(DiagramsStore.fetchDiagram(diagram.parent.id));
  };

  const handleClick = (e: React.MouseEvent<HTMLDivElement>) => {
    appDispatch<any>(GuiStore.actionCreators.selectTreeItem(diagram?.parent?.id));
  };

  const handleWheelEvent = (e: WheelEvent) => {
    //e.preventDefault();
    //e.stopPropagation();
    //var newZ = zoom + ((e.deltaY) / Math.abs(e.deltaY)) * -0.1
    //setZoom(newZ);
    //console.log("ZOOM", newZ)
  };

  function handleScrollCapture(event: any) {
    event.stopPropagation(); // Это предотвратит дальнейшее распространение события прокрутки
    event.preventDefault();
    console.log('Scroll event captured!');
    // Ваша логика обработки события прокрутки
  }

  if (diagram == null) {
    return null;
  }
  var content = diagram.content.filter(e => e.parent_id == diagram?.parent?.id);

  var coord: IDiagramCoord = 
  {
    left: 0,
    top: 0,
    width: currentPaperProps?.paper_width,
    height: currentPaperProps?.paper_height
  }

  if (diagram?.parent != null &&
    diagram.parent.geometry != null &&
    diagram.parent.geometry.width > 0 &&
    diagram.parent.geometry.height > 0) {
    coord = diagram.parent.geometry;
  }

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
          key={"box yellow"}
          onClick={handleClick}
          bgcolor="primary.light"
          sx={{// Paper
            position: 'absolute',
            border: 0,
            padding: 0,
            margin: 0,
            top: '0px', // Сдвиг от верхнего края
            left: '65px', // Сдвиг от левого края
            height: currentPaperProps?.paper_height * zoom + 'px',
            width: currentPaperProps?.paper_width * zoom + 'px',
            
          }}>

          {
            (currentPaperProps?.background_img != null && currentPaperProps?.background_img != '') &&
              <img
              key={"img_background" + diagram?.parent?.id}
              src={currentPaperProps?.background_img}
                style={{
                  border: 0,
                  padding: 0,
                  margin: 0,
                  width: '100%',
                  height: '100%',
                  objectFit: 'fill'
                }} />
          }
          

          <DiagramGray diagram={diagram?.parent} zoom={zoom} />

          {
            content.map((dgr, index) =>
              <DiagramElement
                diagram={dgr}
                parent={diagram?.parent}
                parent_coord={coord}
                zoom={zoom}
                z_index={2}
                getColor={getColor}
                key={index} />
            )
          }

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


        <Select

          labelId="select-depth"
          id="depth-select"
          value={depth.toString()}
          label="Depth"
          onChange={handleChange}
        >
          <MenuItem value={0}>0</MenuItem>
          <MenuItem value={1}>1</MenuItem>
          <MenuItem value={2}>2</MenuItem>
          <MenuItem value={3}>3</MenuItem>
        </Select>

      </ButtonGroup>

    </Box>
  );
}