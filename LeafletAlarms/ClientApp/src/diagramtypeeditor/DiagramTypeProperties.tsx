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
import { IDiagramTypeDTO } from '../store/Marker';


export function DiagramTypeProperties() {

  const appDispatch = useAppDispatch();
  const diagramType: IDiagramTypeDTO = useSelector((state: ApplicationState) => state?.diagramtypeStates?.cur_diagramtype);

  const [typeId, setTypeId] = React.useState(diagramType?.id);

  useEffect(() => {

  }, []);

  const handleSave = useCallback(() => {

  }, []);

  function editMe() {
    //appDispatch(DiagramTypeStore.fetchDiagramTypeById(typeId));
  };

  const deleteMe = useCallback(
    () => {
    }, [])

  function handleChangeId(e: any) {
    const { target: { id, value } } = e;
    setTypeId(value);
  };

  function handleChangeName(e: any) {
    const { target: { id, value } } = e;
  };

  function handleChangeSrc(e: any) {
    const { target: { id, value } } = e;
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
            onChange={handleChangeId}
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
                  fullWidth
                label='id'
                size="small"
                value={region.id}
                onChange={handleChangeSrc}
                >
                  
                </TextField></ListItem>
            <ListItem id={"reg_geo" + index.toString()}>
              
              <TextField
                label='left'
                size="small"
                value={region.geometry.left}
                onChange={handleChangeSrc}
              >
              </TextField>
              <TextField
                label='top'
                size="small"
                value={region.geometry.top}
                onChange={handleChangeSrc}
              >
              </TextField>
              <TextField
                label='width'
                size="small"
                value={region.geometry.width}
                onChange={handleChangeSrc}
              >
              </TextField>
              <TextField
                label='height'
                size="small"
                value={region.geometry.height}
                onChange={handleChangeSrc}
              >
              </TextField>
              </ListItem>
            </div>
          )}
      </List>
    </Box>
  );
}