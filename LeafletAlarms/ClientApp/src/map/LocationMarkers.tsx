import * as React from 'react';
import * as L from 'leaflet';
import { useDispatch, useSelector} from "react-redux";
import * as MarkersStore from '../store/MarkersStates';
import * as GuiStore from '../store/GUIStates';
import * as MarkersVisualStore from '../store/MarkersVisualStates';
import { ApplicationState } from '../store';
import { BoundBox } from '../store/Marker';


import { useCallback, useMemo, useEffect } from 'react'
import {
  useMap,
  useMapEvents,
  Circle,
  Polygon,
  Polyline
} from 'react-leaflet'


import { LeafletEvent, LeafletMouseEvent } from 'leaflet';

declare module 'react-redux' {
  interface DefaultRootState extends ApplicationState { }
}

function MyPolygon(props: any) {
  if (props.hidden == true) {
    return null;
  }

  const dispatch = useDispatch();

  const eventHandlers = useMemo(
    () => ({
      click(event: LeafletMouseEvent) {
        var selected_id = props.marker.id;
        dispatch(GuiStore.actionCreators.selectTreeItem(selected_id));
      }
    }),
    [],
  )

  return (
    <React.Fragment>
      <Polygon
        pathOptions={props.pathOptions}
        positions={props.positions}
        eventHandlers={eventHandlers}
      >
      </Polygon>
    </React.Fragment>
  );
}

function MyPolyline(props: any) {
  if (props.hidden == true) {
    return null;
  }

  const dispatch = useDispatch();
  const eventHandlers = useMemo(
    () => ({
      click(event: LeafletMouseEvent) {
        var selected_id = props.marker.id;
        dispatch(GuiStore.actionCreators.selectTreeItem(selected_id));
      }
    }),
    [],
  )

  return (
    <React.Fragment>
      <Polyline
        pathOptions={props.pathOptions}
        positions={props.positions}
        eventHandlers={eventHandlers}
      >
      </Polyline>
    </React.Fragment>
  );
}

function MyCircle(props: any) {
  if (props.hidden == true) {
    return null;
  }

  const dispatch = useDispatch();
  const eventHandlers = useMemo(
    () => ({
      click(event: LeafletMouseEvent) {
        dispatch(GuiStore.actionCreators.selectTreeItem(props.marker.id));
      }
    }),
    [],
  )

  return (
    <React.Fragment>
      <Circle
        pathOptions={props.pathOptions}
        center={props.center}
        radius={props.radius}
        eventHandlers={eventHandlers}
      >
      </Circle>
    </React.Fragment>
  );
}

export function LocationMarkers() {

  const dispatch = useDispatch();
  const parentMap = useMap();
  
  useEffect(() => {
    console.log('ComponentDidMount LocationMarkers');
    var bounds: L.LatLngBounds;
    bounds = parentMap.getBounds();
    var boundBox: BoundBox = {
      wn: [bounds.getWest(), bounds.getNorth()],
      es: [bounds.getEast(), bounds.getSouth()],
      zoom: parentMap.getZoom()
    };
    dispatch(MarkersStore.actionCreators.requestMarkers(boundBox));
  }, []);

  const selected_id = useSelector((state) => state?.guiStates?.selected_id);
  const checked_ids = useSelector((state) => state?.guiStates?.checked);

  const selectedEditMode = useSelector((state) => state.editState);

  const markers = useSelector((state) => state?.markersStates?.markers);
  const isChanging = useSelector((state) => state?.markersStates?.isChanging);
  const visualStates = useSelector((state) => state?.markersVisualStates?.visualStates);
  const alarmedObjects = useSelector((state) => state?.markersVisualStates?.alarmed_objects);
  const objProps = useSelector((state) => state?.objPropsStates?.objProps);

  useEffect(
    () => {
      if (markers == null) {
        return;
      }
      var objArray2: string[] = [];
      markers.circles?.forEach(arr => objArray2.push(arr.id));
      markers.polygons?.forEach(arr => objArray2.push(arr.id));
      markers.polylines?.forEach(arr => objArray2.push(arr.id));
      dispatch(MarkersVisualStore.actionCreators.requestMarkersVisualStates(objArray2));
    }, [markers]);

   const mapEvents = useMapEvents({
      click(e) {
       var ll: L.LatLng = e.latlng as L.LatLng;
       //dispatch(GuiStore.actionCreators.selectTreeItem(null));
      },

       moveend(e: LeafletEvent) {
         var bounds: L.LatLngBounds;
         bounds = e.target.getBounds();
         var boundBox: BoundBox = {
           wn: [bounds.getWest(), bounds.getNorth()],
           es: [bounds.getEast(), bounds.getSouth()],
           zoom: e.target.getZoom()
         };

         dispatch(MarkersStore.actionCreators.requestMarkers(boundBox));

         console.log('Locat  ionMarkers Chaged:', e.target.getBounds(), "->", e.target.getZoom());
       },
        mousemove(e: L.LeafletMouseEvent) {

     }
   });

  
  const selectMe = useCallback(
    (marker, e) => {
      parentMap.closePopup();
      var selected_id = marker.id;
      dispatch(GuiStore.actionCreators.selectTreeItem(selected_id));
    }, [])

  
  useEffect(
    () => {
      dispatch(GuiStore.actionCreators.requestTreeUpdate());
    }, [isChanging]);

  const colorOptionsUnselected = { fillColor: "green" };
  const colorOptionsSelected = { fillColor: "yellow" };
  const colorOptionsChecked = { fillColor: "blue" };
  

  const getColor = useCallback(
    (id: string) => {

      var retColor: L.PathOptions    = {    };
      retColor.fillColor = colorOptionsUnselected.fillColor;
      retColor.dashArray = '';
      retColor.color = 'green';
      
      if (checked_ids.indexOf(id) !== -1) {
        //retColor.fillColor = colorOptionsChecked.fillColor;
        retColor.dashArray = '5,10';
      }

      if (selected_id == id) {
        //retColor.fillColor = colorOptionsSelected.fillColor;
        retColor.dashArray = '5,10';
      }

      {
        var vState = visualStates.states.find(i => i.id == id);

        if (vState != null && vState.states.length > 0) {
          var vStateFirst = vState.states[0];
          var vStateDescr = visualStates.states_descr.find(s => s.state == vStateFirst);
          if (vStateDescr != null) {
            retColor.fillColor = vStateDescr.state_color
            retColor.color = vStateDescr.state_color
          }
        }
      }

      var vAlarmState = alarmedObjects.find(i => i.id == id);

      if (vAlarmState != null
        && (vAlarmState.alarm || vAlarmState.children_alarms > 0)) {
        //const colorOptions = {
        //  fillColor: 'yellow',
        //  fillOpacity: 0.5,
        //  color: 'yellow',
        //  opacity: 1,
        //  dashArray: '5,10'
        //}
        retColor.color = 'red';
      }
      else {

      }

      return retColor;

    }, [visualStates, alarmedObjects, selected_id, checked_ids])

  var hidden_id: string = null;

  if (selectedEditMode.edit_mode) {
    hidden_id = objProps?.id;
  }

  return (
    <React.Fragment>
      {
        markers?.circles?.map((marker, index) =>
          <MyCircle
            key={marker.id}
            center={marker.geometry.coord}
            pathOptions={getColor(marker.id)}
            radius={marker.radius > 0 ? marker.radius : 10}
            hidden={marker.id == hidden_id}

            marker={marker}
          >
          </MyCircle>
        )}
      {
        markers?.polygons?.map((marker, index) =>
          <MyPolygon
            pathOptions={getColor(marker.id)}
            positions={marker.geometry.coord}
            key={marker.id}
            hidden={marker.id == hidden_id}

            marker={marker}
            selectMe={selectMe}
          >
          </MyPolygon>
        )}

      {
        markers?.polylines?.map((marker, index) =>
          <MyPolyline
            pathOptions={getColor(marker.id)}
            positions={marker.geometry.coord}
            key={marker.id}
            hidden={marker.id == hidden_id}

            marker={marker}
          >
          </MyPolyline>
        )}      
    </React.Fragment>
  );
}
