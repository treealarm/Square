import * as React from 'react';

import { useEffect, useCallback} from 'react';
import { useSelector} from "react-redux";

import { ApplicationState } from '../store';
import * as ObjPropsStore from '../store/ObjPropsStates';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import { Box, Button, ButtonGroup, Divider, IconButton, TextField, Tooltip } from '@mui/material';

import DeleteIcon from '@mui/icons-material/Delete';
import EditIcon from '@mui/icons-material/Edit';
import SaveIcon from '@mui/icons-material/Save';
import AddTaskIcon from '@mui/icons-material/AddTask';
import DeleteOutlineIcon from '@mui/icons-material/DeleteOutline';

import { IObjProps, Marker, TreeMarker, getExtraProp, DeepCopy, IGeometryDTO, ObjExtraPropertyDTO } from '../store/Marker';
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

declare module 'react-redux' {
  interface DefaultRootState extends ApplicationState { }
}

export function ObjectProperties() {

  const dispatch = useAppDispatch();

  const selected_id = useSelector((state: ApplicationState) => state?.guiStates?.selected_id);
  const objProps: IObjProps = useSelector((state: ApplicationState) => state?.objPropsStates?.objProps);
  const propsUpdated = useSelector((state: ApplicationState) => state?.objPropsStates?.updated);
  const selectedEditMode = useSelector((state: ApplicationState) => state.editState);

  const [newPropName, setNewPropName] = React.useState('');

  //useEffect(() => {
  //  if (selected_id == null) {
  //    dispatch<any>(EditStore.actionCreators.setFigureEditMode(false));
  //  }
  //  else if (selected_id?.startsWith('00000000', 0))
  //  {
  //    var selectedMarker = markers.figs.find(m => m.id == selected_id);

  //    if (selectedMarker != null) {
  //      dispatch<any>(ObjPropsStore.actionCreators.setObjPropsLocally(selectedMarker));
  //      return;
  //    }
  //  }

  //  dispatch<any>(ObjPropsStore.actionCreators.getObjProps(selected_id));
  //}, [selected_id]);


  function handleChangeName (e: any){
    const { target: { id, value } } = e;
    var copy = DeepCopy(objProps);
    if (copy == null) {
      return;
    }
    copy.name = value;
    dispatch<any>(ObjPropsStore.actionCreators.setObjPropsLocally(copy));
  };

  function handleChangeParentId(e: any) {
    const { target: { id, value } } = e;
    var copy = DeepCopy(objProps);
    if (copy == null) {
      return;
    }
    copy.parent_id = value;
    dispatch<any>(ObjPropsStore.actionCreators.setObjPropsLocally(copy));
  };

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
    
    dispatch<any>(ObjPropsStore.actionCreators.setObjPropsLocally(copy));
  };

  function handleAddProp(id: any) {

    let copy = DeepCopy(objProps);

    if (copy == null) {
      return;
    }

    const first = copy.extra_props.find((obj) => {
      return obj.prop_name == id;
    });

    if (first != null) {
      return;
    }
    var newProp: ObjExtraPropertyDTO =
    {
      str_val: "",
      prop_name: id
    };
    copy.extra_props.push(newProp);
    dispatch<any>(ObjPropsStore.actionCreators.setObjPropsLocally(copy));
  };

  function handleRemoveProp(id: any) {

    let copy = DeepCopy(objProps);

    if (copy == null) {
      return;
    }

    copy.extra_props = copy.extra_props.filter((obj) => {
      return obj.prop_name != id;
    });

    dispatch<any>(ObjPropsStore.actionCreators.setObjPropsLocally(copy));
  };

  useEffect(() => {
    if (objProps?.id != null) {
      dispatch<any>(MarkersStore.actionCreators.requestMarkersByIds([objProps.id]));
    }      
  }, [propsUpdated]);

  const childDiagramPropEvents = { clickSave: () => { } };

  const handleSave = useCallback(() => {

    childDiagramPropEvents.clickSave();
    var copy = DeepCopy(objProps);
    if (copy == null) {
      return;
    }

    dispatch<any>(ObjPropsStore.actionCreators.updateObjProps(copy));

    // Update name in tree control.
    var treeItem: TreeMarker = {
      id: copy.id,
      name: copy.name
    }
    dispatch<any>(TreeStore.setTreeItem(treeItem));

    // Stop edit mode.
    dispatch<any>(EditStore.actionCreators.setFigureEditMode(false));

  }, [objProps, childDiagramPropEvents.clickSave]);

  function editMe(props: IObjProps, editMode: boolean){
    dispatch<any>(EditStore.actionCreators.setFigureEditMode(editMode));

    if (!editMode) {
      dispatch<any>(ObjPropsStore.actionCreators.getObjProps(selected_id));
    }
  };

  const deleteMe = useCallback(
    (props: IObjProps, e: any) => {

      var marker: Marker = {
        name: props.name,
        id: props.id,
        parent_id: props.parent_id
      }
      let idsToDelete: string[] = [marker.id];
      dispatch<any>(MarkersStore.actionCreators.deleteMarker(idsToDelete));
      dispatch<any>(GuiStore.actionCreators.selectTreeItem(null));
      dispatch(DiagramsStore.remove_ids_locally(idsToDelete));
    }, [])


    if (objProps == null || objProps == undefined) {
      return null;
  }

  var geometry: IGeometryDTO;

  try {
    geometry = JSON.parse(getExtraProp(objProps, "geometry")) as IGeometryDTO;
  }
  catch (e: any) {
    console.error(e);
  }
  


  const handleSelect = (id: string | null) => {
    const event = {
      target: {
        value: id
      }
    } as React.ChangeEvent<HTMLInputElement>;
    handleChangeParentId(event);
    setIsSelectorOpen(false);
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
              onClick={(e: any) => editMe(objProps, !selectedEditMode.edit_mode)} >
              {
                selectedEditMode.edit_mode ?
                  <NotInterestedSharpIcon fontSize="inherit" /> :
                  <EditIcon fontSize="inherit" />
              }

              </IconButton>
            </Tooltip>

            <Tooltip title={"Delete object"}>
            <IconButton aria-label="delete" size="medium" onClick={(e: any) => deleteMe(objProps, e)}>
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
                <IconButton aria-label="delete" size="small" onClick={(e: any) => { handleRemoveProp(item.prop_name); }}>
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
            <IconButton aria-label="save" size="medium" onClick={(e: any) => { handleAddProp(newPropName); }}>
              <AddTaskIcon fontSize="inherit" />
            </IconButton>
          </Tooltip>
        </ListItem>

      </List>
    </Box>
  );
}