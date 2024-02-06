import * as React from 'react';

import { useEffect, useCallback} from 'react';
import { useSelector} from "react-redux";
import * as DiagramTypeStore from '../store/DiagramTypeStates'

import { ApplicationState } from '../store';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import { Box, ButtonGroup, Divider, IconButton, TextField, Tooltip } from '@mui/material';

import DeleteIcon from '@mui/icons-material/Delete';
import EditIcon from '@mui/icons-material/Edit';
import SaveIcon from '@mui/icons-material/Save';
import { useAppDispatch } from '../store/configureStore';


export function DiagramTypeProperties() {

  const appDispatch = useAppDispatch();
  const diagramType = useSelector((state: ApplicationState) => state?.diagramtypeStates?.cur_diagramtype);

  const [typeId, setTypeId] = React.useState(diagramType?.id);

  useEffect(() => {

  }, []);

  const handleSave = useCallback(() => {
   
  }, []);

  function editMe(){
    appDispatch(DiagramTypeStore.fetchDiagramTypeById(typeId));
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

  return (
    <Box sx={{
      width: '100%',
      
      bgcolor: 'background.paper',
      overflow: 'auto',
      height: '100%',
      border: 1
    }}>

      <List dense>
        <ListItem>
          <ButtonGroup variant="contained" aria-label="properties pannel">
            <Tooltip title={"Save object" }>
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
        <ListItem>
          <TextField
            fullWidth
            label='Id'
            size="small"
            value={typeId ? typeId : ''}
            onChange={handleChangeId }>
          </TextField>
        </ListItem>

        <ListItem>
          <TextField size="small"
            fullWidth
            id="name" label='Name'
            value={diagramType?.name}
            onChange={handleChangeName} />
        </ListItem>

      </List>
    </Box>
  );
}