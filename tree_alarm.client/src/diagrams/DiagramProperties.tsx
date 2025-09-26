/* eslint-disable react-hooks/exhaustive-deps */
import { useSelector } from "react-redux";

import { ApplicationState } from '../store';
import * as DiagramsStore from '../store/DiagramsStates';
import * as DiagramTypeStore from '../store/DiagramTypeStates'
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import { Box, Button, Divider, TextField } from '@mui/material';
import InputLabel from '@mui/material/InputLabel';
import MenuItem from '@mui/material/MenuItem';
import FormControl from '@mui/material/FormControl';
import Select from '@mui/material/Select';
import Link from '@mui/material/Link';

import { useAppDispatch } from '../store/configureStore';
import { DeepCopy, IDiagramContentDTO, IDiagramFullDTO } from '../store/Marker';
import { useCallback, useEffect } from 'react';

import { IDiagramDTO } from '../store/Marker';
import { useNavigate } from 'react-router-dom';
import FileUpload from '../components/FileUpload';
import AddIcon from '@mui/icons-material/Add';
import DeleteIcon from '@mui/icons-material/Delete';
import LinkIcon from '@mui/icons-material/Link';
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
  const cur_diagram_content: IDiagramContentDTO | null =
    useSelector((state: ApplicationState) => state?.diagramsStates?.cur_diagram_content ?? null);
  const cur_diagram_full: IDiagramFullDTO | null = useSelector((state: ApplicationState) => state?.diagramsStates?.cur_diagram ?? null);
  
  const result = useSelector((state: ApplicationState) => state?.diagramtypeStates?.result);
  const cur_diagramtype = useSelector((state: ApplicationState) => state?.diagramtypeStates?.cur_diagramtype);
  const content: IDiagramDTO[] = cur_diagram_content?.content??[];

  const cur_diagram: IDiagramDTO | null = cur_diagram_full?.diagram ?? null;
  var parentType = cur_diagram_full?.parent_type ?? null;
  

  useEffect(() => {
    if (result == 'OK') {
      
      appDispatch(DiagramTypeStore.set_result(null)); 
     
      var copy = DeepCopy(cur_diagram) ?? null;

      if (!copy) {
        return;
      }
      copy.dgr_type = cur_diagramtype?.name;

      appDispatch(DiagramsStore.updateDiagrams([copy]));

      var newType = cur_diagram_content?.dgr_types?.find(dt => dt.name == copy?.dgr_type);
      var cur_diagram_copy = DeepCopy(cur_diagram_content) ?? null;

      if (!cur_diagram_copy) {
        return;
      }
      var newContent = cur_diagram_copy.content?.filter(e => e.id != copy?.id);
      newContent.push(copy);
      cur_diagram_copy.content = newContent;
      if (newType == null) {
        // Add type if not exist  
        if (cur_diagram_copy.dgr_types == null) {
          cur_diagram_copy.dgr_types = [];
        }
        cur_diagram_copy.dgr_types.push(DeepCopy(cur_diagramtype));        
      }
      appDispatch(DiagramsStore.set_diagram_content_locally(cur_diagram_copy));
    }
  }, [result, cur_diagram, cur_diagramtype, cur_diagram_content]);


  function handleEditDiagramClick() {
    appDispatch(DiagramTypeStore.set_local_filter(cur_diagram?.dgr_type));
    appDispatch(DiagramTypeStore.set_result('EDIT_TYPE'));
    navigate("/editdiagram");
  }

  const handleSave = useCallback(() => {
    if (cur_diagram != null) {
      appDispatch(DiagramsStore.updateDiagrams([DeepCopy(cur_diagram)]));
    }
    //TODO Must refetch content since other properties updated

  }, [cur_diagram, content]);

  //subscribe to props.events changes
  useEffect(() => {
    if (props.events)
    {
      props.events.clickSave = handleSave;
    }
  }, [props.events, handleSave]);

  function handleChangeType(e: any) {
    const { target: { value } } = e;
    var copy = DeepCopy(cur_diagram_full);
    copy.cur_diagram.dgr_type = value;
    appDispatch(DiagramsStore.update_single_diagram_locally(copy));
  }

  function handleChangeBackgroundImg(e: any) {
    const { target: { value } } = e;
    var copy = DeepCopy(cur_diagram_full);
    copy.diagram.background_img = value;
    appDispatch(DiagramsStore.update_single_diagram_locally(copy));
  }
  function handleChangeRegion(e: any) {
    const { target: { id, value } } = e;

    var copy = DeepCopy(cur_diagram_full);
    var textId = id;

    if (!isNaN(value)) {
      if (textId == "editreg_left") {
        copy.diagram.geometry.left = value;
      }
      if (textId == "editreg_top") {
        copy.diagram.geometry.top = value;
      }
      if (textId == "editreg_width") {
        copy.diagram.geometry.width = value;
      }
      if (textId == "editreg_height") {
        copy.diagram.geometry.height = value;
      }
    }

    appDispatch(DiagramsStore.update_single_diagram_locally(copy));
  }

  function handleChangeRegionId(e: any) {
    const { target: { value } } = e;

    var copy = DeepCopy(cur_diagram_full);
    if (copy?.diagram) {
      copy.diagram.region_id = value;
      appDispatch(DiagramsStore.update_single_diagram_locally(copy));
    }    
  }

  const onClickAddNewDiagram = useCallback(() => {

    if (!selected_id || cur_diagram != null)
      return;
    const copy: IDiagramDTO = {
      id: selected_id,
      name: 'New Diagram',
      parent_id: null,
      dgr_type: null,
      background_img: null,
      geometry:
      {
        left: 0,
        top: 0,
        width: 100,
        height: 100
      },
      region_id: null
    };

    appDispatch(DiagramsStore.updateDiagrams([copy]));  
  }, [selected_id, cur_diagram]);
  const onClickDeleteDiagram = useCallback(() => {

    if (cur_diagram == null)
      return;

    appDispatch(DiagramsStore.deleteDiagrams([cur_diagram.id]));
  }, [selected_id, cur_diagram]);
  
  function onUploadSuccess(data: any) {

    if (data == null) {
      return;
    }

    var copy = DeepCopy(cur_diagram);
    copy.background_img = data;
    appDispatch(DiagramsStore.update_single_diagram_locally(copy));
  }

  let enable_add_diagram = selected_id != null && cur_diagram == null;

  if (enable_add_diagram) {
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
          <ListItem id="btn_add">
        <Button
          variant="contained"
          color="primary"
          startIcon={<AddIcon />}
          onClick={onClickAddNewDiagram}
        >
          Add Diagram for Object
            </Button>
            </ListItem>
        </List>
        </Box>
    );
  }


  if (cur_diagram == null
    ||
    (cur_diagram.geometry == null && cur_diagram.dgr_type == null))
  {

    // lets consider it as a parent - regular object, not diagram
    return null;
  }
  let cur_diagram_type = cur_diagram?.dgr_type != null ? cur_diagram?.dgr_type : "";

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
            label='diagram type'
            size="small"
            value={cur_diagram_type}
            onChange={handleChangeType}
          >
          </TextField>
          <Divider><br></br></Divider>

          <Button
            component={Link} onClick={() => { handleEditDiagramClick(); }}
            variant="contained"
            color="primary"
            startIcon={<LinkIcon/>}
          >
            {'select'}
          </Button >
           
        </ListItem>

        <ListItem id={"reg_id"}
          sx={{ display: parentType?.regions?.length??0 > 0 ? "block" : "none" }}>

          <FormControl fullWidth>
            <InputLabel id="editreg_id_label">region id</InputLabel>
            <Select
              labelId="editreg_id_label"
              name={"editreg_id"}
              value={cur_diagram?.region_id != null ? cur_diagram.region_id : ""}
              label="Region id"
              onChange={handleChangeRegionId}
              size="small"
            >
              {
                parentType?.regions.map((region, index) =>
                  <MenuItem key={"menu" + index } value={region?.region_key}>{ region?.region_key}</MenuItem>
                )}

            </Select>
          </FormControl>

        </ListItem>

        <ListItem id="curDiagram_background_img">
          <TextField
            fullWidth
            label='background image'
            size="small"
            value={cur_diagram?.background_img == null ? "" : cur_diagram?.background_img}
            onChange={handleChangeBackgroundImg}
          >
          </TextField>
          <Divider orientation='vertical'><br /></Divider>
          <FileUpload key="file_upload" path="diagram_background" onUploadSuccess={onUploadSuccess} />
        </ListItem>

        <ListItem id={"reg_geo"}>

          <TextField
            id={"editreg_left"}
            label='left'
            size="small"
            value={cur_diagram?.geometry?.left}
            onChange={handleChangeRegion}
          >
          </TextField>
          <TextField
            id={"editreg_top"}
            label='top'
            size="small"
            value={cur_diagram?.geometry?.top}
            onChange={handleChangeRegion}
          >
          </TextField>
          <TextField
            id={"editreg_width"}
            label='width'
            size="small"
            value={cur_diagram?.geometry?.width}
            onChange={handleChangeRegion}
          >
          </TextField>
          <TextField
            id={"editreg_height"}
            label='height'
            size="small"
            value={cur_diagram?.geometry?.height}
            onChange={handleChangeRegion}
          >
          </TextField>
        </ListItem>
      <ListItem id="btn_delete">
        <Button
          variant="contained"
          color="primary"
          startIcon={<DeleteIcon />}
          onClick={onClickDeleteDiagram}
        >
          Delete Diagram for Object
        </Button>
        </ListItem>
      </List>
    </Box>
  );
}