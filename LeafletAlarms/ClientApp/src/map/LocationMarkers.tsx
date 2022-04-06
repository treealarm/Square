import * as React from 'react';
import * as L from 'leaflet';
import { useDispatch, useSelector, useStore} from "react-redux";
import * as MarkersStore from '../store/MarkersStates';
import * as GuiStore from '../store/GUIStates';
import { ApplicationState } from '../store';
import { BoundBox, ICircle, IFigures, IPolygon } from '../store/Marker';
import { yellow } from '@mui/material/colors';

import { useCallback, useMemo, useState, useEffect } from 'react'
import {
  CircleMarker,
  Popup,
  useMap,
  useMapEvents,
  Circle,
  Polygon
} from 'react-leaflet'

import { LeafletEvent } from 'leaflet';
import { Figures } from '../store/EditStates';

declare module 'react-redux' {
  interface DefaultRootState extends ApplicationState { }
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
  const selectedTool = useSelector((state) => state.editState.figure);


   const mapEvents = useMapEvents({
    click(e) {
       var ll: L.LatLng = e.latlng as L.LatLng;

       var figures: IFigures = {
         
       };

       if (selectedTool == 'Circle')
       {
         var circle: ICircle = {
           name: ll.toString(),
           parent_id: selected_id,
           geometry: [ll.lat, ll.lng],
           type: 'Point'
         };

         figures.circles = [circle];
       }

       if (selectedTool == 'Polygon') {
         var figure: IPolygon = {
           name: ll.toString(),
           parent_id: selected_id,
           geometry: [[ll.lat, ll.lng], [ll.lat, ll.lng+0.01], [ll.lat+0.01, ll.lng+0.01]],
           type: 'Polygon'
         };

         figures.polygons = [figure];
       }

       dispatch(MarkersStore.actionCreators.sendMarker(figures));
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
       console.log('cursor', parentMap.getContainer().style.cursor);
     }
   });

  

  const eventHandlers = useMemo(
    () => ({
      mouseover() {
        //console.log('cursor', parentMap.getContainer().style.cursor);
      }
    }),
    [],
  )

  const deleteMe = useCallback(
    (marker, e) => {
      console.log(e.target.value);
      //alert('delete ' + marker.name);
      parentMap.closePopup();
      let idsToDelete: string[] = [marker.id];
      dispatch(MarkersStore.actionCreators.deleteMarker(idsToDelete));
      dispatch(GuiStore.actionCreators.selectTreeItem(null));
  }, [])

  const markers = useSelector((state) => state?.markersStates?.markers);


  const isChanging = useSelector((state) => state?.markersStates?.isChanging);
  useEffect(
    () => {
      dispatch(GuiStore.actionCreators.requestTreeUpdate());
    }, [isChanging]);

  const colorOptionsUnselected = { color: "green" };
  const colorOptionsSelected = { color: "yellow" };
  const colorOptionsChecked = { color: "blue" };
  

  const getColor = (id: string) => {
    let colorOption = colorOptionsUnselected;

    if (checked_ids.indexOf(id) !== -1) {
      colorOption = colorOptionsChecked;
    }

    if (selected_id == id) {
      colorOption = colorOptionsSelected;
    }

    return colorOption;
  }

  const purpleOptions = { color: 'purple' }
  const polygon = [
    [51.515, -0.09],
    [51.52, -0.1],
    [51.52, -0.12],
  ]

  return (
    <React.Fragment>
      {
        markers?.circles?.map((marker, index) =>
          <Circle
            key={index}
            center={marker.geometry}
            pathOptions={getColor(marker.id)}
            radius={100}
            eventHandlers={eventHandlers}
          >
            <Popup>
              <table>
                <tbody>
                  <tr><td>{marker.name}</td></tr>
                  <tr><td>
                    <span className="menu_item" onClick={(e) => deleteMe(marker, e)}>Delete</span>
                  </td></tr>
                </tbody>
              </table>
            </Popup>
          </Circle>
        )}
      {
        markers?.polygons?.map((marker, index) =>
          <Polygon pathOptions={purpleOptions} positions={marker.geometry} />
      )}
      
    </React.Fragment>
  );
}
