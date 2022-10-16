import * as React from 'react';

import { useEffect, useCallback} from 'react';
import { useDispatch, useSelector} from "react-redux";

import { ApplicationState } from '../store';
import * as ObjPropsStore from '../store/ObjPropsStates';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import { Box, ButtonGroup, IconButton, TextField } from '@mui/material';
import DeleteIcon from '@mui/icons-material/Delete';
import EditIcon from '@mui/icons-material/Edit';
import SaveIcon from '@mui/icons-material/Save';
import SearchIcon from '@mui/icons-material/Search';
import { IObjProps, PointType, LineStringType, PolygonType, Marker, TreeMarker, ViewOption, IPointCoord, IPolygonCoord, LatLngPair, getExtraProp, setExtraProp } from '../store/Marker';
import * as EditStore from '../store/EditStates';
import * as MarkersStore from '../store/MarkersStates';
import * as GuiStore from '../store/GUIStates';
import * as TreeStore from '../store/TreeStates';
import NotInterestedSharpIcon from '@mui/icons-material/NotInterestedSharp';
import * as L from 'leaflet';

declare module 'react-redux' {
  interface DefaultRootState extends ApplicationState { }
}

export function ObjectProperties() {

  const dispatch = useDispatch();

  const selected_id = useSelector((state) => state?.guiStates?.selected_id);
  const objProps = useSelector((state) => state?.objPropsStates?.objProps);
  const propsUpdated = useSelector((state) => state?.objPropsStates?.updated);
  const selectedEditMode = useSelector((state) => state.editState);
  
  useEffect(() => {
    if (selected_id == null) {
      dispatch(EditStore.actionCreators.setFigureEditMode(false));
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

  useEffect(() => {
    if (propsUpdated) {
      // Update figure.
      dispatch(MarkersStore.actionCreators.requestMarkersByIds([objProps.id]));
    }

  }, [propsUpdated]);

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
    dispatch(EditStore.actionCreators.setFigureEditMode(false));

  }, [objProps]);

  function editMe(props: IObjProps, editMode: boolean){
    dispatch(EditStore.actionCreators.setFigureEditMode(editMode));

    if (!editMode) {
      dispatch(ObjPropsStore.actionCreators.getObjProps(selected_id));
    }
  };

  const deleteMe = useCallback(
    (props: IObjProps, e) => {

      var marker: Marker = {
        name: props.name,
        id: props.id,
        parent_id: props.parent_id
      }
      let idsToDelete: string[] = [marker.id];
      dispatch(MarkersStore.actionCreators.deleteMarker(idsToDelete));
      dispatch(GuiStore.actionCreators.selectTreeItem(null));
    }, [])

  const searchMeOnMap = useCallback(
    (props: IObjProps, e) => {

      var geo = JSON.parse(getExtraProp(props, "geometry"));
      var myFigure = null;
      var center: L.LatLng = null;

      switch (geo.type) {
        case PointType:
          var coord: LatLngPair = geo.coord;
          center = new L.LatLng(coord[0], coord[1]);
          break;
        case LineStringType:
          var coordArr: LatLngPair[] = geo.coord;
          myFigure = new L.Polyline(coordArr)
          break;
        case PolygonType:
          var coordArr: LatLngPair[] = geo.coord;
          myFigure = new L.Polygon(coordArr)
          break;
        default:
          break;
      }

      if (center == null) {
        let bounds: L.LatLngBounds = myFigure.getBounds();
        center = bounds.getCenter();
      }
      
      var viewOption: ViewOption = {
        map_center: [center.lat, center.lng],
        zoom: props.zoom_min
      };

      dispatch(GuiStore.actionCreators.setMapOption(viewOption));

    }, [objProps])

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
        <ListItem>
          <ButtonGroup variant="contained" aria-label="outlined primary button group">
            <IconButton aria-label="save" size="large" onClick={handleSave}>
              <SaveIcon fontSize="inherit" />
            </IconButton>
            <IconButton aria-label="edit" size="large" onClick={(e) => editMe(objProps, !selectedEditMode.edit_mode)} >
              {
                selectedEditMode.edit_mode ?
                  <NotInterestedSharpIcon fontSize="inherit" /> :
                  <EditIcon fontSize="inherit" />
              }

            </IconButton>
            <IconButton aria-label="delete" size="large" onClick={(e) => deleteMe(objProps, e)}>
              <DeleteIcon fontSize="inherit" />
            </IconButton>
          </ButtonGroup>

          <ButtonGroup variant="contained" aria-label="search button group">
            <IconButton aria-label="search" size="large" onClick={(e) => searchMeOnMap(objProps, e)}>
              <SearchIcon fontSize="inherit" />
            </IconButton>
          </ButtonGroup>

        </ListItem>
        <ListItem>
          <TextField
            fullWidth
          label='Id'
          size="small"
          value={objProps.id}
          inputProps={{ readOnly: true}}>
          </TextField>
        </ListItem>
        <ListItem>
          <TextField
            fullWidth
            label='ParentId'
            size="small"
            value={objProps.parent_id}
            inputProps={{ readOnly: true}}>
          </TextField>
          </ListItem>
        
        <ListItem>
          <TextField size="small"
          fullWidth
          id="outlined" label='Name'
            value={objProps?.name}
            onChange={handleChangeName} />
        </ListItem>
        
        {
          objProps?.extra_props?.map((item, index) =>
            <ListItem key={index}>

              <TextField size="small"
                fullWidth
                id={item.prop_name} label={item.prop_name}
                value={item.str_val}
                onChange={handleChangeProp} />

            </ListItem>
            )
        }

      </List>
    </Box>
  );
}