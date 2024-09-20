/* eslint-disable react-hooks/exhaustive-deps */
import * as React from 'react';

import { useEffect, useCallback, useMemo} from 'react';
import { useSelector} from "react-redux";

import { ApplicationState } from '../store';
import * as ObjPropsStore from '../store/ObjPropsStates';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import { Box, ButtonGroup, Divider, IconButton, TextField, Tooltip } from '@mui/material';

import DeleteIcon from '@mui/icons-material/Delete';
import EditIcon from '@mui/icons-material/Edit';
import SaveIcon from '@mui/icons-material/Save';
import AddTaskIcon from '@mui/icons-material/AddTask';
import DeleteOutlineIcon from '@mui/icons-material/DeleteOutline';

import { IObjProps, Marker, TreeMarker, DeepCopy, IGeometryDTO, ObjExtraPropertyDTO } from '../store/Marker';
import * as EditStore from '../store/EditStates';
import * as MarkersStore from '../store/MarkersStates';
import * as GuiStore from '../store/GUIStates';
import * as TreeStore from '../store/TreeStates';
import NotInterestedSharpIcon from '@mui/icons-material/NotInterestedSharp';
import { SearchMeOnMap } from './SearchMeOnMap';
import { useAppDispatch } from '../store/configureStore';
import EditOptions from './EditOptions';
import { RequestRoute } from './RequestRoute';
import { DiagramProperties } from '../diagrams/DiagramProperties';
import * as DiagramsStore from '../store/DiagramsStates';
import { ControlSelector } from '../prop_controls/control_selector';
import { ObjectSelector } from '../components/ObjectSelector';
import SelectedObjectGeoEditor from './SelectedObjectGeoEditor';

export function ObjectProperties() {

  const appDispatch = useAppDispatch();

  const selected_id = useSelector((state: ApplicationState) => state?.guiStates?.selected_id);
  const objProps: IObjProps|null = useSelector((state: ApplicationState) => state?.objPropsStates?.objProps);
  const propsUpdated = useSelector((state: ApplicationState) => state?.objPropsStates?.updated);
  const selectedEditMode = useSelector((state: ApplicationState) => state.editState);
  const selected_marker = useSelector((state: ApplicationState) => state?.markersStates?.selected_marker);

  const [newPropName, setNewPropName] = React.useState('');


  function handleChangeName (e: any){
    const { target: { value } } = e;
    var copy = DeepCopy(objProps);
    if (copy == null) {
      return;
    }
    copy.name = value;
    appDispatch(ObjPropsStore.setObjPropsLocally(copy));
  }

  function handleChangeParentId(e: any) {
    const { target: { value } } = e;
    var copy = DeepCopy(objProps);
    if (copy == null) {
      return;
    }
    copy.parent_id = value;
    appDispatch(ObjPropsStore.setObjPropsLocally(copy));
  }

  function handleChangeProp(e: any) {
    const { target: { id, value } } = e;
    let copy = DeepCopy(objProps);

    if (copy == null) {
      return;
    }

    const first = copy.extra_props.find((obj) => {
      return obj.prop_name === id;
    });

    if (first != null) {
      first.str_val = value;
    }
    
    appDispatch(ObjPropsStore.setObjPropsLocally(copy));
  }

  function handleAddProp(id: any) {

    let copy = DeepCopy(objProps);

    if (copy == null) {
      return;
    }

    const first = copy.extra_props?.find((obj) => {
      return obj.prop_name == id;
    })

    if (first != null) {
      return;
    }
    var newProp: ObjExtraPropertyDTO =
    {
      str_val: "",
      prop_name: id
    };

    copy.extra_props?.push(newProp);
    appDispatch(ObjPropsStore.setObjPropsLocally(copy));
  }

  function handleRemoveProp(id: any) {

    let copy = DeepCopy(objProps);

    if (copy == null) {
      return;
    }

    copy.extra_props = copy.extra_props.filter((obj) => {
      return obj.prop_name != id;
    });

    appDispatch(ObjPropsStore.setObjPropsLocally(copy));
  }

  useEffect(() => {
    if (objProps?.id != null) {
      appDispatch(MarkersStore.fetchMarkersByIds([objProps.id]));
    }      
  }, [propsUpdated, appDispatch, objProps?.id]);

  const childDiagramPropEvents = useMemo(() => ({
    clickSave: () => { }
  }), []);

  const childGeoPropEvents = useMemo(() => ({
    clickSave: () => { }
  }), []);

  const handleSave = useCallback(() => {

    childDiagramPropEvents.clickSave();
    childGeoPropEvents.clickSave();

    var copy = DeepCopy(objProps);
    if (copy == null) {
      return;
    }

    appDispatch(ObjPropsStore.updateObjProps(copy));

    // Update name in tree control.
    var treeItem: TreeMarker = {
      id: copy.id,
      name: copy.name
    }
    appDispatch(TreeStore.setTreeItem(treeItem));

    // Stop edit mode.
    appDispatch(EditStore.setEditMode(false));

  }, [objProps, childDiagramPropEvents]);

  function editMe(props: IObjProps, editMode: boolean){
    appDispatch(EditStore.setEditMode(editMode));

    if (!editMode) {
      appDispatch(ObjPropsStore.fetchObjProps(selected_id??null));
    }
  }

  const deleteMe = useCallback(
    (obj_props: IObjProps) => {

      var marker: Marker = {
        name: obj_props.name,
        id: obj_props.id,
        parent_id: obj_props.parent_id
      }
      let idsToDelete: string[] = [marker.id];
      appDispatch(MarkersStore.deleteMarkers(idsToDelete));
      appDispatch(GuiStore.selectTreeItem(null));
      appDispatch(DiagramsStore.remove_ids_locally(idsToDelete));
    }, [appDispatch])


    if (objProps == null || objProps == undefined) {
      return null;
  }

  var geometry: IGeometryDTO = selected_marker?.geometry;


  const handleSelect = (id: string | null) => {
    const event = {
      target: {
        value: id
      }
    } as React.ChangeEvent<HTMLInputElement>;
    handleChangeParentId(event);
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

            <EditOptions />

            <Tooltip title={"Save object" }>
            <IconButton aria-label="save" size="medium" onClick={handleSave}>
              <SaveIcon fontSize="inherit" />
            </IconButton>
          </Tooltip>

            <Tooltip title={"Edit object"}>
            <IconButton aria-label="edit" size="medium"
              onClick={() => editMe(objProps, !selectedEditMode.edit_mode)} >
              {
                selectedEditMode.edit_mode ?
                  <NotInterestedSharpIcon fontSize="inherit" /> :
                  <EditIcon fontSize="inherit" />
              }

              </IconButton>
            </Tooltip>

            <Tooltip title={"Delete object"}>
            <IconButton aria-label="delete" size="medium" onClick={() => deleteMe(objProps)}>
              <DeleteIcon fontSize="inherit" />
              </IconButton>
            </Tooltip>

          </ButtonGroup>

        </ListItem>
        <Divider><br></br></Divider>
        <ListItem>
          <SearchMeOnMap
            geometry={geometry}
            text={objProps?.id}
            zoom_min={objProps?.zoom_min} />
          <RequestRoute geometry={geometry}></RequestRoute>
        </ListItem>

        <ListItem>
          <TextField
            fullWidth
            label='ParentId'
            size="small"
            value={objProps.parent_id ? objProps.parent_id:''}
            inputProps={{ readOnly: true}}>
          </TextField>
          <ObjectSelector
            selectedId={objProps.parent_id ?? null}
            excludeId={objProps.id}
            onSelect={handleSelect}
          />
          </ListItem>
        <ListItem>
          <TextField size="small"
          fullWidth
          id="name" label='Name'
            value={objProps?.name}
            onChange={handleChangeName} />
        </ListItem>
        <ListItem sx={{ width: '100%' }}>
          <SelectedObjectGeoEditor events={childGeoPropEvents} />
        </ListItem>
        <DiagramProperties events={childDiagramPropEvents} />
        <Divider><br></br></Divider>
        {
          objProps?.extra_props?.map((item, index) =>
            <ListItem key={index}
              sx={{  // Добавьте стили здесь
                display: 'flex',  // Включить flexbox
                alignItems: 'flex-start',  // Выровнять элементы по верхнему краю
              }}
            >

              <ControlSelector 
                prop_name={item.prop_name}
                str_val={item.str_val}
                visual_type={item.visual_type}
                handleChangeProp={handleChangeProp} />

              <Tooltip title={"remove property"}>
                <IconButton aria-label="delete" size="small" onClick={() => { handleRemoveProp(item.prop_name); }}>
                  <DeleteOutlineIcon fontSize="inherit" />
                </IconButton>
              </Tooltip>

            </ListItem>
            )
        }
        <Divider><br></br></Divider>
        <ListItem>
          <TextField size="small"
            fullWidth
            id="new_property_name" label='new property name'
            value={newPropName}
            onChange={(event: React.ChangeEvent<HTMLInputElement>) => { setNewPropName(event.target.value); }} />
          <Tooltip title={"add new property"}>
            <IconButton aria-label="save" size="medium" onClick={() => { handleAddProp(newPropName); }}>
              <AddTaskIcon fontSize="inherit" />
            </IconButton>
          </Tooltip>
        </ListItem>

      </List>
    </Box>
  );
}