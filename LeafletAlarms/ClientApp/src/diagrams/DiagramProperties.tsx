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

export interface ChildEvents {
  clickSave: () => void;
}

// Интерфейс для props дочернего компонента
export interface IDiagramProperties {
  events: ChildEvents;
}
export function DiagramProperties(props: IDiagramProperties) {

  const appDispatch = useAppDispatch();
  const selected_id = useSelector((state: ApplicationState) => state?.guiStates?.selected_id);
  const cur_diagram: IGetDiagramDTO = useSelector((state: ApplicationState) => state?.diagramsStates?.cur_diagram);
  const result = useSelector((state: ApplicationState) => state?.diagramtypeStates?.result);
  const cur_diagramtype = useSelector((state: ApplicationState) => state?.diagramtypeStates?.cur_diagramtype);

  const content: IDiagramDTO[] = cur_diagram?.content;
  var curDiagram = content?.find(e => e.id == selected_id);

  var parent = content?.find(e => e.id == curDiagram?.parent_id);
  var parentType = cur_diagram?.dgr_types?.find(t => t.name == parent?.dgr_type);


  const [selectedDiagram, setSelectedDiagram] = React.useState(curDiagram);
  let navigate = useNavigate();

  useEffect(() => {
    setSelectedDiagram(curDiagram);
  }, [curDiagram]);

  useEffect(() => {
    if (result == 'OK') {
      appDispatch(DiagramTypeStore.set_result(null)); 
      var copy = DeepCopy(selectedDiagram);
      copy.dgr_type = cur_diagramtype?.name;
      setSelectedDiagram(copy);
      var newType = cur_diagram.dgr_types.find(dt => dt.name == copy.dgr_type);

      if (newType == null) {
        // Add type if not exist
        var cur_diagram_copy = DeepCopy(cur_diagram);
        cur_diagram_copy.dgr_types.push(DeepCopy(cur_diagramtype));
        appDispatch(DiagramsStore.set_local_diagram(cur_diagram_copy));
      }
    }
  }, [result, selectedDiagram, cur_diagramtype]);

  function handleEditDiagramClick() {
    appDispatch(DiagramTypeStore.set_local_filter(selectedDiagram?.dgr_type));
    appDispatch(DiagramTypeStore.set_result('EDIT_TYPE'));
    navigate("/editdiagram");
  }

  const handleSave = useCallback(() => {
    if (selectedDiagram == null) {
      return;
    }
    //var copy = DeepCopy(content);
    //copy = copy.filter(d => d.id != selectedDiagram.id);
    //copy.push(DeepCopy(selectedDiagram));
    appDispatch(DiagramsStore.updateDiagrams([DeepCopy(selectedDiagram)]));
  }, [selectedDiagram, content]);

  // Подписываемся на изменение props.events
  useEffect(() => {
    if (props.events)
    {
      props.events.clickSave = handleSave;
    }
  }, [props.events, handleSave]);

  function handleChangeType(e: any) {
    const { target: { id, value } } = e;
    var copy = DeepCopy(selectedDiagram);
    copy.dgr_type = value;
    setSelectedDiagram(copy);
  };

  function handleChangeRegion(e: any) {
    const { target: { id, value } } = e;

    var copy = DeepCopy(selectedDiagram);
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

    setSelectedDiagram(copy);
  };

  function handleChangeRegionId(e: any) {
    const { target: { name, value } } = e;

    var copy = DeepCopy(selectedDiagram);
    copy.region_id = value;
    setSelectedDiagram(copy);
  };

  if (selectedDiagram == null) {
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
            value={selectedDiagram?.dgr_type != null ? selectedDiagram?.dgr_type : ""}
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
              value={selectedDiagram?.region_id != null ? selectedDiagram.region_id : ""}
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

        <ListItem id={"reg_geo"}>

          <TextField
            id={"editreg_left"}
            label='left'
            size="small"
            value={selectedDiagram.geometry.left}
            onChange={handleChangeRegion}
          >
          </TextField>
          <TextField
            id={"editreg_top"}
            label='top'
            size="small"
            value={selectedDiagram.geometry.top}
            onChange={handleChangeRegion}
          >
          </TextField>
          <TextField
            id={"editreg_width"}
            label='width'
            size="small"
            value={selectedDiagram.geometry.width}
            onChange={handleChangeRegion}
          >
          </TextField>
          <TextField
            id={"editreg_height"}
            label='height'
            size="small"
            value={selectedDiagram.geometry.height}
            onChange={handleChangeRegion}
          >
          </TextField>
        </ListItem>
      </List>
    </Box>
  );
}