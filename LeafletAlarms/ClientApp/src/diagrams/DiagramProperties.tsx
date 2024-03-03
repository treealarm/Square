import * as React from 'react';

import { useSelector } from "react-redux";

import { ApplicationState } from '../store';
import * as DiagramsStore from '../store/DiagramsStates';
import * as DiagramTypeStore from '../store/DiagramTypeStates'
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import { Box, Button, Divider, TextField, Typography } from '@mui/material';
import InputLabel from '@mui/material/InputLabel';
import MenuItem from '@mui/material/MenuItem';
import FormControl from '@mui/material/FormControl';
import Select from '@mui/material/Select';
import Link from '@mui/material/Link';

import { useAppDispatch } from '../store/configureStore';
import { DeepCopy, IGetDiagramDTO, IGetDiagramTypesByFilterDTO } from '../store/Marker';
import { useCallback, useEffect } from 'react';

import { IDiagramDTO } from '../store/Marker';
import { useNavigate } from 'react-router-dom';
import FileUpload from '../components/FileUpload';

export interface ChildEvents {
  clickSave: () => void;
}

// Интерфейс для props дочернего компонента
export interface IDiagramProperties {
  events: ChildEvents;
}
export function DiagramProperties(props: IDiagramProperties) {

  const appDispatch = useAppDispatch();
  let navigate = useNavigate();
  const selected_id = useSelector((state: ApplicationState) => state?.guiStates?.selected_id);
  const getDiagramDto: IGetDiagramDTO = useSelector((state: ApplicationState) => state?.diagramsStates?.cur_diagram);
  const result = useSelector((state: ApplicationState) => state?.diagramtypeStates?.result);
  const cur_diagramtype = useSelector((state: ApplicationState) => state?.diagramtypeStates?.cur_diagramtype);

  const content: IDiagramDTO[] = getDiagramDto?.content;
  var curDiagram = content?.find(e => e.id == selected_id);

  var parent = content?.find(e => e.id == curDiagram?.parent_id);
  var parentType = getDiagramDto?.dgr_types?.find(t => t.name == parent?.dgr_type);

  //let timeoutId: any = null;

  //React.useEffect(() => {
  //  if (timeoutId != null) {
  //    clearTimeout(timeoutId);
  //  }

  //  timeoutId = setTimeout(() => {
  //    appDispatch(DiagramsStore.update_single_diagram(copy));
  //  }, 1500);
  //  return () => clearTimeout(timeoutId);
  //}, [curDiagram]);

  useEffect(() => {
    if (result == 'OK') {
      
      appDispatch(DiagramTypeStore.set_result(null)); 
      var copy = DeepCopy(curDiagram);
      copy.dgr_type = cur_diagramtype?.name;
      
      var newType = getDiagramDto.dgr_types?.find(dt => dt.name == copy.dgr_type);
      var cur_diagram_copy = DeepCopy(getDiagramDto);
      var newContent = cur_diagram_copy.content?.filter(e => e.id != copy.id);
      newContent.push(copy);
      cur_diagram_copy.content = newContent;
      if (newType == null) {
        // Add type if not exist  
        if (cur_diagram_copy.dgr_types == null) {
          cur_diagram_copy.dgr_types = [];
        }
        cur_diagram_copy.dgr_types.push(DeepCopy(cur_diagramtype));        
      }
      appDispatch(DiagramsStore.set_local_diagram(cur_diagram_copy));
    }
  }, [result, curDiagram, cur_diagramtype, getDiagramDto]);

  function handleEditDiagramClick() {
    appDispatch(DiagramTypeStore.set_local_filter(curDiagram?.dgr_type));
    appDispatch(DiagramTypeStore.set_result('EDIT_TYPE'));
    navigate("/editdiagram");
  }

  const handleSave = useCallback(() => {
    if (curDiagram == null) {
      return;
    }
    //TODO Must refetch content since other properties updated
    appDispatch(DiagramsStore.updateDiagrams([DeepCopy(curDiagram)]));
  }, [curDiagram, content]);

  //subscribe to props.events changes
  useEffect(() => {
    if (props.events)
    {
      props.events.clickSave = handleSave;
    }
  }, [props.events, handleSave]);

  function handleChangeType(e: any) {
    const { target: { id, value } } = e;
    var copy = DeepCopy(curDiagram);
    copy.dgr_type = value;
    appDispatch(DiagramsStore.update_single_diagram(copy));
  };

  function handleChangeBackgroundImg(e: any) {
    const { target: { id, value } } = e;
    var copy = DeepCopy(curDiagram);
    copy.background_img = value;
    appDispatch(DiagramsStore.update_single_diagram(copy));
  }
  function handleChangeRegion(e: any) {
    const { target: { id, value } } = e;

    var copy = DeepCopy(curDiagram);
    var textId = id;

    if (!isNaN(value)) {
      if (textId == "editreg_left") {
        copy.geometry.left = value;
      }
      if (textId == "editreg_top") {
        copy.geometry.top = value;
      }
      if (textId == "editreg_width") {
        copy.geometry.width = value;
      }
      if (textId == "editreg_height") {
        copy.geometry.height = value;
      }
    }

    appDispatch(DiagramsStore.update_single_diagram(copy));
  };

  function handleChangeRegionId(e: any) {
    const { target: { name, value } } = e;

    var copy = DeepCopy(curDiagram);
    copy.region_id = value;
    appDispatch(DiagramsStore.update_single_diagram(copy));
  };

  function onUploadSuccess(data: any) {

    if (data == null) {
      return;
    }

    var copy = DeepCopy(curDiagram);
    copy.background_img = data;
    appDispatch(DiagramsStore.update_single_diagram(copy));
  }

  if (curDiagram == null
    ||
    (curDiagram.geometry == null && curDiagram.dgr_type == null))
  {
    // lets consider it as a parent - regular object, not diagram
    return null;
  }
  return (
    <Box sx={{
      width: '100%',

      bgcolor: 'background.paper',
      overflow: 'auto',
      height: '100%',
      border: 0
    }}>

      <List dense>
        <Divider><br></br></Divider>

        <ListItem id="diagram_type_name_src">

          <TextField
            fullWidth
            label='Diagram type'
            size="small"
            value={curDiagram?.dgr_type != null ? curDiagram?.dgr_type : ""}
            onChange={handleChangeType}
          >
          </TextField>
          <Divider><br></br></Divider>

          <Link component="button" onClick={() => { handleEditDiagramClick(); }}>
            {'select'}
          </Link >
           
        </ListItem>

        <ListItem id={"reg_id"}>

          <FormControl fullWidth>
            <InputLabel id="editreg_id_label">Region id</InputLabel>
            <Select
              labelId="editreg_id_label"
              name={"editreg_id"}
              value={curDiagram?.region_id != null ? curDiagram.region_id : ""}
              label="Region id"
              onChange={handleChangeRegionId}
              size="small"
            >
              {
                parentType?.regions.map((region, index) =>
                  <MenuItem value={region?.id}>{ region?.id}</MenuItem>
                )}

            </Select>
          </FormControl>

        </ListItem>

        <ListItem id="curDiagram_background_img">
          <TextField
            fullWidth
            label='background image'
            size="small"
            value={curDiagram?.background_img == null ? "" : curDiagram?.background_img}
            onChange={handleChangeBackgroundImg}
          >
          </TextField>
          <Divider orientation='vertical'><br /></Divider>
          <FileUpload key="file_upload" path="diagram_background" onUploadSuccess={onUploadSuccess} />
        </ListItem>
      </List>

        <ListItem id={"reg_geo"}>

          <TextField
            id={"editreg_left"}
            label='left'
            size="small"
            value={curDiagram.geometry.left}
            onChange={handleChangeRegion}
          >
          </TextField>
          <TextField
            id={"editreg_top"}
            label='top'
            size="small"
            value={curDiagram.geometry.top}
            onChange={handleChangeRegion}
          >
          </TextField>
          <TextField
            id={"editreg_width"}
            label='width'
            size="small"
            value={curDiagram.geometry.width}
            onChange={handleChangeRegion}
          >
          </TextField>
          <TextField
            id={"editreg_height"}
            label='height'
            size="small"
            value={curDiagram.geometry.height}
            onChange={handleChangeRegion}
          >
          </TextField>
        </ListItem>

    </Box>
  );
}