import * as React from 'react';

import { useEffect, useCallback, useState, useRef } from 'react';
import { useDispatch, useSelector, useStore } from "react-redux";

import { ApplicationState } from '../store';
import * as ObjPropsStore from '../store/ObjPropsStates';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import { Box, ListItemButton, ListItemText, TextField } from '@mui/material';



declare module 'react-redux' {
  interface DefaultRootState extends ApplicationState { }
}

export function ObjectProperties() {

  const dispatch = useDispatch();
  const selected_id = useSelector((state) => state?.guiStates?.selected_id);
  const objProps = useSelector((state) => state?.objPropsStates?.objProps);


  useEffect(() => {
    dispatch(ObjPropsStore.actionCreators.getObjProps(selected_id));
  }, [selected_id]);

  const [name, setName] = React.useState(objProps?.name);

  const handleChangeName = useCallback(
    (e: any) => {
      const { target: { name, value } } = e;
      setName(value);
    }, [name]
  );


  const handleSave = useCallback(() => {
    var copy = Object.assign({}, objProps);
    copy.name = name;
    dispatch(ObjPropsStore.actionCreators.updateObjProps(copy));
  }, [name]);

    if (objProps == null || objProps == undefined) {
    return null;
  }

  return (
    <Box sx={{
      width: '100%',
      maxWidth: 460,
      bgcolor: 'background.paper',
      overflow: 'auto',
      height: '100%',
      border: 1
    }}>

      <List>
        <ListItem>{selected_id}</ListItem>
        <ListItem>
          <TextField size="small"
          fullWidth sx={{ width: '25ch' }}
          id="outlined" label='Name'
            defaultValue={objProps.name}
            onChange={handleChangeName} />
        </ListItem>
        <ListItem>{objProps.parent_id}</ListItem>
        <ListItem>{objProps.type}</ListItem>
        <ListItemButton role={undefined} onClick={handleSave}>
          <ListItemText primary="Save" />
        </ListItemButton>
      </List>
    </Box>
  );
}