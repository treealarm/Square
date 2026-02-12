/* eslint-disable react-hooks/exhaustive-deps */
import * as React from 'react';
import { useCallback, useEffect, WheelEvent } from 'react';
import { ApplicationState } from '../store';
import { useSelector } from 'react-redux';
import { IDiagramCoord, IDiagramDTO, IDiagramContentDTO } from '../store/Marker';

import { useAppDispatch } from '../store/configureStore';
import * as DiagramsStore from '../store/DiagramsStates';
import * as ValuesStore from '../store/ValuesStates';

import { useState } from 'react';
import { Box, ButtonGroup, IconButton } from '@mui/material';
import * as GuiStore from '../store/GUIStates';
import ZoomInIcon from '@mui/icons-material/ZoomIn';
import ZoomOutIcon from '@mui/icons-material/ZoomOut';
import RestartAltIcon from '@mui/icons-material/RestartAlt';
import DiagramElement from './DiagramElement';

import * as MarkersVisualStore from '../store/MarkersVisualStates';
import Select, { SelectChangeEvent } from '@mui/material/Select';
import MenuItem from '@mui/material/MenuItem';

export default function DiagramViewer() {

  const appDispatch = useAppDispatch();
  const [zoom, setZoom] = useState(1.0);

  const cur_diagram_content: IDiagramContentDTO | null =
    useSelector((state: ApplicationState) => state?.diagramsStates?.cur_diagram_content ?? null);
  const depth = useSelector((state: ApplicationState) => state?.diagramsStates?.depth);
  const visualStates = useSelector((state: ApplicationState) => state?.markersVisualStates?.visualStates);
  const alarmedObjects = useSelector((state: ApplicationState) => state?.markersVisualStates?.visualStates?.alarmed_objects);
  const selected_id = useSelector((state: ApplicationState) => state?.guiStates?.selected_id);

  const cur_diagram = cur_diagram_content?.content.find(e => e.id == cur_diagram_content?.diagram_id);

  const object_ids = cur_diagram_content?.content?.map(arr => arr.id) || [];
  const update_values_periodically = useSelector((state: ApplicationState) => state?.valuesStates?.update_values_periodically);

  const [currentPaperProps, setCurrentPaperProps] = useState({
    paper_width: 1000,
    paper_height: 800,
    background_img: null
  });

  useEffect(
    () => {
      if (object_ids.length == 0) {
        return;
      }
      appDispatch(ValuesStore.fetchValuesByOwners(object_ids));
    }, [object_ids, update_values_periodically]);

  useEffect(
    () => {
      if (cur_diagram_content?.content == null) {
        return;
      }
      var objArray2: string[] = [];
      cur_diagram_content?.content?.forEach(arr => objArray2.push(arr.id));
      appDispatch(MarkersVisualStore.requestMarkersVisualStates(objArray2));
    }, [cur_diagram_content?.content]);


  useEffect(
    () => {

      if (cur_diagram && cur_diagram.geometry &&
        cur_diagram.geometry.width > 100 &&
        cur_diagram.geometry.height > 100) {
        setCurrentPaperProps(
          {
            paper_width: cur_diagram.geometry.width * 2,
            paper_height: cur_diagram.geometry.height * 2,
            background_img: currentPaperProps.background_img
          }
        );
      }
    }, [cur_diagram, currentPaperProps.background_img]);

  const getColor = useCallback(
    (marker: IDiagramDTO) => {
      var id = marker.id;

      var retColor: any = null;

      if (selected_id == id) {
        //retColor.dashArray = '5,10';
      }

      {
        var vState = visualStates?.states.find(i => i.id == id);

        if (vState != null && vState.states.length > 0) {
          var vStateFirst = vState.states[0];
          var vStateDescr = visualStates?.states_descr.find(s => s.state == vStateFirst);
          if (vStateDescr != null) {
            retColor = vStateDescr.state_color;
          }
        }
      }

      var vAlarmState = alarmedObjects?.find(i => i.id == id);

      if (vAlarmState != null
        && (vAlarmState.alarm || vAlarmState.children_alarms > 0)) {

        retColor = '#ff0000';
      }
      //else {
      //  var color = getExtraProp(marker, "__color");

      //  if (color != null) {
      //    retColor = color;
      //  }
      //}

      return retColor;

    }, [visualStates, alarmedObjects, selected_id]);



  const handleChange = (event: SelectChangeEvent) => {
    appDispatch<any>(DiagramsStore.set_depth(event.target.value as any as number));
    if (cur_diagram?.id)
      appDispatch<any>(DiagramsStore.fetchGetDiagramContent(cur_diagram?.id ?? null));
  };

  const handleClick = (e: React.MouseEvent<HTMLDivElement>) => {
    console.log(e);
    if (cur_diagram?.id)
      appDispatch(GuiStore.selectTreeItem(cur_diagram?.id));
  };

  const handleWheelEvent = (e: WheelEvent) => {
    //e.preventDefault();
    //e.stopPropagation();
    //var newZ = zoom + ((e.deltaY) / Math.abs(e.deltaY)) * -0.1
    //setZoom(newZ);
    console.log(e)
  };


  if (cur_diagram_content == null || cur_diagram == null) {
    return null;
  }
 
  var coord: IDiagramCoord = 
  {
    left: 0,
    top: 0,
    width: currentPaperProps?.paper_width,
    height: currentPaperProps?.paper_height
  }

  if (cur_diagram != null &&
    cur_diagram.geometry != null &&
    cur_diagram.geometry.width > 0 &&
    cur_diagram.geometry.height > 0) {
    coord = cur_diagram.geometry;
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
              key={"img_background" + cur_diagram?.id}
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
          

          
            <DiagramElement
              diagram={cur_diagram}
              parent={null}
              parent_coord={coord}
              zoom={zoom}
              z_index={1}
              getColor={getColor}
              key={'base diagram'} />


        </Box>
      </Box>

      <ButtonGroup variant="contained" orientation="vertical"
        sx={{ position: 'absolute', left: '5px', backgroundColor: 'lightgray' }}>
        <IconButton
          onClick={() => setZoom(zoom + 0.1)}>
          <ZoomInIcon fontSize="inherit"></ZoomInIcon>
        </IconButton>

        <IconButton
          onClick={() => setZoom(zoom - 0.1)}>
          <ZoomOutIcon fontSize="inherit" />
        </IconButton>
        <IconButton
          onClick={() => setZoom(1)}>
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