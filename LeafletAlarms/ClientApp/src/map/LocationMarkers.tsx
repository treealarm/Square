import * as React from 'react';
import * as L from 'leaflet';
import { useDispatch, useSelector, useStore } from "react-redux";
import * as MarkersStore from '../store/MarkersStates';
import * as GuiStore from '../store/GuiStates';

import {
  useCallback,
  useEffect,
  useState
} from "react";

import {
  Popup,
  CircleMarker,
  useMapEvents,
  useMap
} from "react-leaflet";

import { ApplicationState } from '../store';
import { Marker } from '../store/Marker';
import { GUIState } from '../store/GUIStates';

declare module 'react-redux' {
  interface DefaultRootState extends ApplicationState { }
}

export function LocationMarkers() {

  const dispatch = useDispatch();

  var initialMarkers: Marker[] = [];
  var marker1: Marker = { points: [51.500, -0.091], name :'Initial', id : '1234' };
  initialMarkers.push(marker1);

  useEffect(() => {
    console.log('ComponentDidMount');
    dispatch(MarkersStore.actionCreators.requestMarkers('initial_box'));
  }, []);

  const markers = useSelector((state) => state?.markersStates?.markers);
  const selected_id = useSelector((state1) => state1?.guiState?.selected_id);

  console.log('Selected:', selected_id);

  const mapEvents = useMapEvents({
    click(e) {
      var ll: L.LatLng = e.latlng as L.LatLng;
      var marker: Marker = { points: [ll.lat, ll.lng], name: 'Initial' };
      marker.parent_id = selected_id;
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

  return (
    <React.Fragment>
      { markers?.map((marker, index) =>
        <CircleMarker key={index} center={new L.LatLng(marker.points[0], marker.points[1])}>
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