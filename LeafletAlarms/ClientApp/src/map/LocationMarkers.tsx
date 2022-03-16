import * as React from 'react';
import * as L from 'leaflet';
import { useDispatch, useSelector, useStore} from "react-redux";
import * as MarkersStore from '../store/MarkersStates';
import * as GuiStore from '../store/GUIStates';

import { useCallback, useEffect, useState } from "react";

import { Popup, CircleMarker, useMapEvents, useMap } from "react-leaflet";

import { ApplicationState } from '../store';
import { Marker } from '../store/Marker';
import { yellow } from '@mui/material/colors';

declare module 'react-redux' {
  interface DefaultRootState extends ApplicationState { }
}

export function LocationMarkers() {

  const dispatch = useDispatch();
  
  useEffect(() => {
    console.log('ComponentDidMount');
    dispatch(MarkersStore.actionCreators.requestMarkers('initial_box'));
  }, []);

  const selected_id = useSelector((state) => state?.guiStates?.selected_id);
  const checked_ids = useSelector((state) => state?.guiStates?.checked);

   const mapEvents = useMapEvents({
    click(e) {
      var ll: L.LatLng = e.latlng as L.LatLng;
       var marker: Marker = {
         points: [ll.lat, ll.lng],
         name: ll.toString(),
         parent_id : selected_id
       };

       dispatch(MarkersStore.actionCreators.sendMarker(marker));
    }
  });

  const map = useMap();

  const deleteMe = useCallback(
    (marker, e) => {
      console.log(e.target.value);
      //alert('delete ' + marker.name);
      map.closePopup();
      dispatch(MarkersStore.actionCreators.deleteMarker(marker));
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
            center={new L.LatLng(marker.points[0], marker.points[1])}
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
