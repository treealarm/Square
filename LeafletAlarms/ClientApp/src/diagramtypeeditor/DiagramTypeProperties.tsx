import * as React from 'react';

import { useEffect, useCallback } from 'react';
import { useSelector } from "react-redux";
import * as DiagramTypeStore from '../store/DiagramTypeStates'

import { ApplicationState } from '../store';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import { Box, ButtonGroup, Divider, IconButton, TextField, Tooltip } from '@mui/material';

import DeleteIcon from '@mui/icons-material/Delete';
import EditIcon from '@mui/icons-material/Edit';
import SaveIcon from '@mui/icons-material/Save';
import { useAppDispatch } from '../store/configureStore';
import { DeepCopy, IDiagramTypeDTO } from '../store/Marker';


export function DiagramTypeProperties() {

  const appDispatch = useAppDispatch();
  const diagramType: IDiagramTypeDTO = useSelector((state: ApplicationState) => state?.diagramtypeStates?.cur_diagramtype);

  const [typeId, setTypeId] = React.useState(diagramType?.id);

  useEffect(() => {

  }, []);

  const handleSave = useCallback(() => {
    var copy = DeepCopy(diagramType);
    appDispatch(DiagramTypeStore.updateDiagramTypes([copy]));    
  }, [diagramType]);

  function editMe() {
    //appDispatch(DiagramTypeStore.fetchDiagramTypeById(typeId));
  };

  const deleteMe = useCallback(
    () => {
    }, [])


  function handleChangeName(e: any) {
    const { target: { id, value } } = e;
    var copy = DeepCopy(diagramType);
    copy.name = value;
    appDispatch(DiagramTypeStore.set_local_diagram(copy));
  };

  function handleChangeSrc(e: any) {
    const { target: { id, value } } = e;
    var copy = DeepCopy(diagramType);
    copy.src = value;
    appDispatch(DiagramTypeStore.set_local_diagram(copy));
  };

  function handleChangeRegion(e: any) {
    const { target: { id, value } } = e;

    var copy = DeepCopy(diagramType);

    var val = id.split(':', 2);
    var textId = val[0];
    var index = val[1];

    if (textId == "editreg_id") {
      copy.regions[index].id = value;
    }
    else if (!isNaN(value)) {
      if (textId == "editreg_left") {
        copy.regions[index].geometry.left = value;
      }
      if (textId == "editreg_top") {
        copy.regions[index].geometry.top = value;
      }
      if (textId == "editreg_width") {
        copy.regions[index].geometry.width = value;
      }
      if (textId == "editreg_height") {
        copy.regions[index].geometry.height = value;
      }
    }

    appDispatch(DiagramTypeStore.set_local_diagram(copy));
  };

  return (
    <Box sx={{
      width: '100%',

      bgcolor: 'background.paper',
      overflow: 'auto',
      height: '100%',
      border: 0
    }}>

      <List dense>
        <ListItem>
          <ButtonGroup variant="contained" aria-label="properties pannel">
            <Tooltip title={"Save object"}>
              <IconButton aria-label="save" size="medium" onClick={handleSave}>
                <SaveIcon fontSize="inherit" />
              </IconButton>
            </Tooltip>

            <Tooltip title={"Edit object"}>
              <IconButton aria-label="edit" size="medium"
                onClick={(e: any) => editMe()} >

                <EditIcon fontSize="inherit" />

              </IconButton>
            </Tooltip>

            <Tooltip title={"Delete object"}>
              <IconButton aria-label="delete" size="medium" onClick={(e: any) => deleteMe()}>
                <DeleteIcon fontSize="inherit" />
              </IconButton>
            </Tooltip>

          </ButtonGroup>

        </ListItem>
        <Divider><br></br></Divider>
        <ListItem id="diagram_type_name_id">
          <TextField
            fullWidth

            label='Id'
            size="small"
            value={diagramType ? diagramType?.id : ""}
            inputProps={{ readOnly: true }}>
          </TextField>
        </ListItem>

        <ListItem id="diagram_type_name_name">
          <TextField
            fullWidth
            label='Name'
            size="small"
            value={diagramType ? diagramType?.name : ""}
            onChange={handleChangeName}
          >
          </TextField>
        </ListItem>

        <ListItem id="diagram_type_name_src">
          <TextField
            fullWidth
            label='Src'
            size="small"
            value={diagramType ? diagramType?.src : ""}
            onChange={handleChangeSrc}
          >
          </TextField>
        </ListItem>
        {
          diagramType?.regions?.map((region, index) =>
            <div><Divider><br /></Divider>
              <ListItem id={"reg_id" + index.toString()}>
                <TextField
                  id={"editreg_id:" + index.toString()}
                  fullWidth
                  label='id'
                  size="small"
                  value={region.id}
                  onChange={handleChangeRegion}
                >

                </TextField></ListItem>
              <ListItem id={"reg_geo" + index.toString()}>

                <TextField
                  id={"editreg_left:" + index.toString()}
                  label='left'
                  size="small"
                  value={region.geometry.left}
                  onChange={handleChangeRegion}
                >
                </TextField>
                <TextField
                  id={"editreg_top:" + index.toString()}
                  label='top'
                  size="small"
                  value={region.geometry.top}
                  onChange={handleChangeRegion}
                >
                </TextField>
                <TextField
                  id={"editreg_width:" + index.toString()}
                  label='width'
                  size="small"
                  value={region.geometry.width}
                  onChange={handleChangeRegion}
                >
                </TextField>
                <TextField
                  id={"editreg_height:" + index.toString()}
                  label='height'
                  size="small"
                  value={region.geometry.height}
                  onChange={handleChangeRegion}
                >
                </TextField>
              </ListItem>
            </div>
          )}
      </List>
    </Box>
  );
}