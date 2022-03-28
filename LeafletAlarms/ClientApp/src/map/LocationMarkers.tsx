import * as React from 'react';
import * as L from 'leaflet';
import { useDispatch, useSelector, useStore} from "react-redux";
import * as MarkersStore from '../store/MarkersStates';
import * as GuiStore from '../store/GUIStates';
import { ApplicationState } from '../store';
import { BoundBox, GeoPart, Marker } from '../store/Marker';
import { yellow } from '@mui/material/colors';

import { useCallback, useMemo, useState, useEffect } from 'react'
import {
    CircleMarker,
  MapContainer,
  Popup,
  Rectangle,
  TileLayer,
  useMap,
  useMapEvent,
  useMapEvents
  } from 'react-leaflet'
import { LeafletEvent } from 'leaflet';

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


   const mapEvents = useMapEvents({
    click(e) {
       var ll: L.LatLng = e.latlng as L.LatLng;

       var geoPart: GeoPart = {
         lng: ll.lng,
         lat: ll.lat
       };

       var marker: Marker = {
         geometry: geoPart,
         name: ll.toString(),
         parent_id : selected_id
       };

       dispatch(MarkersStore.actionCreators.sendMarker(marker));
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
     }
  });

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

  return (
    <React.Fragment>
      {
        markers?.map((marker, index) =>
          <CircleMarker
            key={index}
            center={new L.LatLng(marker.geometry.lat, marker.geometry.lng)}
            pathOptions={getColor(marker.id)}>
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
          </CircleMarker>
      )}
    </React.Fragment>
  );
}
