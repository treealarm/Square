import * as React from 'react';

import { useEffect, useCallback, useState, useRef } from 'react';
import { useDispatch, useSelector, useStore } from "react-redux";

import { ApplicationState } from '../store';
import * as ObjPropsStore from '../store/ObjPropsStates';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import { Box, Button, ButtonGroup, IconButton, ListItemButton, ListItemText, TextField, ToggleButton } from '@mui/material';
import DeleteIcon from '@mui/icons-material/Delete';
import EditIcon from '@mui/icons-material/Edit';
import SaveIcon from '@mui/icons-material/Save';
import { ObjExtraPropertyDTO, IObjProps, PointType, LineStringType, PolygonType, Marker, TreeMarker } from '../store/Marker';
import * as EditStore from '../store/EditStates';
import * as MarkersStore from '../store/MarkersStates';
import * as GuiStore from '../store/GUIStates';
import * as TreeStore from '../store/TreeStates';
import NotInterestedSharpIcon from '@mui/icons-material/NotInterestedSharp';


declare module 'react-redux' {
  interface DefaultRootState extends ApplicationState { }
}

export function ObjectProperties() {

  const dispatch = useDispatch();

  const selected_id = useSelector((state) => state?.guiStates?.selected_id);
  const objProps = useSelector((state) => state?.objPropsStates?.objProps);
  const selectedEditMode = useSelector((state) => state.editState);
  
  useEffect(() => {
    if (selected_id == null) {
      dispatch(EditStore.actionCreators.setFigureEditMode(EditStore.NothingTool, false));
    }
    dispatch(ObjPropsStore.actionCreators.getObjProps(selected_id));
  }, [selected_id]);


  function handleChangeName (e: any){
    const { target: { id, value } } = e;
    var copy = Object.assign({}, objProps);
    if (copy == null) {
      return;
    }
    copy.name = value;
    dispatch(ObjPropsStore.actionCreators.setObjPropsLocally(copy));
  };

  function handleChangeGeo(e: any) {
    const { target: { id, value } } = e;
    var copy = Object.assign({}, objProps);
    if (copy == null) {
      return;
    }
    copy.geometry = value;
    dispatch(ObjPropsStore.actionCreators.setObjPropsLocally(copy));
  };

  function handleChangeProp(e: any) {
    const { target: { id, value } } = e;
    let copy = Object.assign({}, objProps);
    if (copy == null) {
      return;
    }

    const first = copy.extra_props.find((obj) => {
      return obj.prop_name === id;
    });

    if (first != null) {
      first.str_val = value;
    }
    
    dispatch(ObjPropsStore.actionCreators.setObjPropsLocally(copy));
  };


  const handleSave = useCallback(() => {
    var copy = Object.assign({}, objProps);
    if (copy == null) {
      return;
    }

    dispatch(ObjPropsStore.actionCreators.updateObjProps(copy));

    // Update name in tree control.
    var treeItem: TreeMarker = {
      id: copy.id,
      name: copy.name
    }
    dispatch(TreeStore.actionCreators.setTreeItem(copy));

    // Stop edit mode.
    dispatch(EditStore.actionCreators.setFigureEditMode(EditStore.NothingTool, false));

    // Update figure.
    dispatch(MarkersStore.actionCreators.requestMarkersByIds([treeItem.id]));
    

  }, [objProps]);

  function editMe(props: IObjProps, editMode: boolean){

      if (!editMode) {
        dispatch(EditStore.actionCreators.setFigureEditMode(EditStore.NothingTool, false));
        return;
      }

      var marker: Marker = {
        name: props.name,
        type: props.type,
        id: props.id,
        parent_id: props.parent_id        
      }

      var value = EditStore.NothingTool;

      if (marker.type == PointType) {
        value = EditStore.CircleTool
      }

      if (marker.type == LineStringType) {
        value = EditStore.PolylineTool
      }

      if (marker.type == PolygonType) {
        value = EditStore.PolygonTool
      }

      dispatch(EditStore.actionCreators.setFigureEditMode(value, true));
  };

  const deleteMe = useCallback(
    (props: IObjProps, e) => {

      var marker: Marker = {
        name: props.name,
        type: props.type,
        id: props.id,
        parent_id: props.parent_id
      }
      let idsToDelete: string[] = [marker.id];
      dispatch(MarkersStore.actionCreators.deleteMarker(idsToDelete));
      dispatch(GuiStore.actionCreators.selectTreeItem(null));
    }, [])

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
            value={objProps?.name}
            onChange={handleChangeName} />
        </ListItem>
        <ListItem>
          <TextField size="small"
            fullWidth sx={{ width: '25ch' }}
            id="outlined" label='Geo'
            value={objProps?.geometry}
            onChange={handleChangeGeo} />
        </ListItem>
        <ListItem>{objProps.parent_id}</ListItem>
        <ListItem>{objProps.type}</ListItem>
        {
          objProps?.extra_props.map((item, index) =>
            <ListItem key={index}>

              <TextField size="small"
                fullWidth sx={{ width: '25ch' }}
                id={item.prop_name} label={item.prop_name}
                value={item.str_val}
                onChange={handleChangeProp} />

            </ListItem>
            )
        }
        <ListItem>
          <ButtonGroup variant="contained" aria-label="outlined primary button group">
            <IconButton aria-label="save" size="large">
              <SaveIcon fontSize="inherit" onClick={handleSave} />
            </IconButton>
            <IconButton aria-label="edit" size="large">
              {
                selectedEditMode.edit_mode ? 
                  <NotInterestedSharpIcon fontSize="inherit" onClick={(e) => editMe(objProps, false)} /> :
                  <EditIcon fontSize="inherit" onClick={(e) => editMe(objProps, true)} />
              }            
              
            </IconButton>
            <IconButton aria-label="delete" size="large">
              <DeleteIcon fontSize="inherit" onClick={(e) => deleteMe(objProps, e)}/>
            </IconButton>          
          </ButtonGroup>
        </ListItem>
      </List>
    </Box>
  );
}