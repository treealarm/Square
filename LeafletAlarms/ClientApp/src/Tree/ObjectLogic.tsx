import * as React from 'react';

import { useCallback } from 'react';
import { useDispatch, useSelector } from "react-redux";
import * as ObjLogicStore from '../store/ObjLogicStates';
import { ApplicationState } from '../store';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import { Box, ButtonGroup, IconButton, TextField } from '@mui/material';
import SaveIcon from '@mui/icons-material/Save';

declare module 'react-redux' {
  interface DefaultRootState extends ApplicationState { }
}

export function ObjectLogic() {

  const dispatch = useDispatch();

  const selected_id = useSelector((state) => state?.guiStates?.selected_id);
  const logic = useSelector((state) => state?.objLogicStates?.logic);

  React.useEffect(() => {
    if (selected_id == null) {
      dispatch(ObjLogicStore.actionCreators.setObjLogicLocally([]));
    }
    else {
      dispatch(ObjLogicStore.actionCreators.getObjLogic(selected_id));
    }    
  }, [selected_id]);

  const handleSave = useCallback(() => {

  }, []);



  return (
    <Box sx={{
      width: '100%',

      bgcolor: 'background.paper',
      overflow: 'auto',
      height: '100%',
      border: 1
    }}>

      <List>
        <ListItem>
          <ButtonGroup variant="contained" aria-label="properties pannel">
            <IconButton aria-label="save" size="medium" onClick={handleSave}>
              <SaveIcon fontSize="inherit" />
            </IconButton>
           
          </ButtonGroup>

        </ListItem> 
        {
          logic?.map((item, index) =>
            <ListItem key={index}>

              <TextField size="small"
                fullWidth
                id={item.logic} label={"logic_name"}
                value={item.logic}
                />

            </ListItem>
          )
        }
      </List>
    </Box>
  );
}