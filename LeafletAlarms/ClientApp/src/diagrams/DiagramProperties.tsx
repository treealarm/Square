import * as React from 'react';

import { useSelector } from "react-redux";

import { ApplicationState } from '../store';
import * as DiagramsStore from '../store/DiagramsStates';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import { Box, Divider, TextField } from '@mui/material';


import { useAppDispatch } from '../store/configureStore';
import { DeepCopy, IDiagramDTO } from '../store/Marker';
import { useCallback, useEffect } from 'react';

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
  const content: IDiagramDTO[] = useSelector((state: ApplicationState) => state?.diagramsStates?.cur_diagram?.content);
  var curDiagram = content?.find(e => e.id == selected_id);

  const [selectedDiagram, setSelectedDiagram] = React.useState(curDiagram);

  useEffect(() => {
    setSelectedDiagram(curDiagram);
  }, [curDiagram]);


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

    var val = id.split(':', 2);
    var textId = val[0];

    if (textId == "editreg_id") {
      copy.region_id = value;
    }
    else if (!isNaN(value)) {
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

        </ListItem>

        <ListItem id={"reg_id"}>
          <TextField
            id={"editreg_id:"}
            fullWidth
            label='Region id'
            size="small"
            value={selectedDiagram?.region_id != null ? selectedDiagram.region_id : ""}
            onChange={handleChangeRegion}
          >

          </TextField>

        </ListItem>

        <ListItem id={"reg_geo"}>

          <TextField
            id={"editreg_left:"}
            label='left'
            size="small"
            value={selectedDiagram.geometry.left}
            onChange={handleChangeRegion}
          >
          </TextField>
          <TextField
            id={"editreg_top:"}
            label='top'
            size="small"
            value={selectedDiagram.geometry.top}
            onChange={handleChangeRegion}
          >
          </TextField>
          <TextField
            id={"editreg_width:"}
            label='width'
            size="small"
            value={selectedDiagram.geometry.width}
            onChange={handleChangeRegion}
          >
          </TextField>
          <TextField
            id={"editreg_height:"}
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