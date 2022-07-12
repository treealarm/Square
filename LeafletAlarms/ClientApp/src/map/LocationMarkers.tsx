import * as React from 'react';
import * as L from 'leaflet';
import { useDispatch, useSelector} from "react-redux";
import * as MarkersStore from '../store/MarkersStates';
import * as GuiStore from '../store/GUIStates';
import { ApplicationState } from '../store';
import { BoundBox, Marker } from '../store/Marker';


import { useCallback, useMemo, useEffect } from 'react'
import {
  useMap,
  useMapEvents,
  Circle,
  Polygon,
  Polyline
} from 'react-leaflet'


import { LeafletEvent, LeafletMouseEvent } from 'leaflet';
import { EditableFigure } from './EditableFigure';

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

   const mapEvents = useMapEvents({
      click(e) {
         var ll: L.LatLng = e.latlng as L.LatLng;
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

  
  const updateBaseMarker = useCallback(
    (marker, e) => {
      dispatch(MarkersStore.actionCreators.updateBaseInfo(marker));
    }, [])

  
  useEffect(
    () => {
      dispatch(GuiStore.actionCreators.requestTreeUpdate());
    }, [isChanging]);

  const colorOptionsUnselected = { color: "green" };
  const colorOptionsSelected = { color: "yellow" };
  const colorOptionsChecked = { color: "blue" };
  

  const getColor = useCallback(
    (id: string) => {

      if (checked_ids.indexOf(id) !== -1) {
        return colorOptionsChecked;
      }

      if (selected_id == id) {
        return colorOptionsSelected;
      }

      var vState = visualStates.find(i => i.id == id);

      if (vState != null) {
        return vState;
      }

      return colorOptionsUnselected;

    }, [visualStates, selected_id, checked_ids])

  var hidden_id: string = null;

  if (selectedEditMode.edit_mode) {
    hidden_id = selected_id;
  }

  return (
    <React.Fragment>
      {
        markers?.circles?.map((marker, index) =>
          <MyCircle
            key={marker.id}
            center={marker.geometry}
            pathOptions={getColor(marker.id)}
            radius={marker.radius > 0 ? marker.radius : 10}
            hidden={marker.id == hidden_id}

            marker={marker}
            updateBaseMarker={updateBaseMarker}
          >
          </MyCircle>
        )}
      {
        markers?.polygons?.map((marker, index) =>
          <MyPolygon
            pathOptions={getColor(marker.id)}
            positions={marker.geometry}
            key={marker.id}
            hidden={marker.id == hidden_id}

            marker={marker}
            selectMe={selectMe}
            updateBaseMarker={updateBaseMarker}>
          </MyPolygon>
        )}

      {
        markers?.polylines?.map((marker, index) =>
          <MyPolyline
            pathOptions={getColor(marker.id)}
            positions={marker.geometry}
            key={marker.id}
            hidden={marker.id == hidden_id}

            marker={marker}
            updateBaseMarker={updateBaseMarker}>
          </MyPolyline>
        )}

      <EditableFigure/>

    </React.Fragment>
  );
}
