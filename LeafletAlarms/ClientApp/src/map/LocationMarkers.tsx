import * as React from 'react';
import * as L from 'leaflet';
import { useDispatch, useSelector, useStore } from "react-redux";
import * as MarkersStore from '../store/MarkersStates';

import {
  useEffect,
  useState
} from "react";

import {
  Popup,
  CircleMarker,
  useMapEvents
} from "react-leaflet";
import { Marker } from '../store/MarkersStates';
import { ApplicationState } from '../store';

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
  }, [])

  const markers = useSelector((state) => state.markersStates.markers);

  //const [markers1, setMarkers] = useState(markers);

  const map = useMapEvents({
    click(e) {
      var ll: L.LatLng = e.latlng as L.LatLng;
      var marker: Marker = { points: [ll.lat, ll.lng], name: 'Initial', id: null };
      markers.push(marker);
      //setMarkers((prevValue) => [...prevValue, marker]);

      dispatch(MarkersStore.actionCreators.sendMarker(marker));
    }
  });

  return (
    <React.Fragment>
      {markers?.map((marker, index) =>
        <CircleMarker key={index} center={new L.LatLng(marker.points[0], marker.points[1])}><Popup>{index}</Popup></CircleMarker>)}
    </React.Fragment>
  );
}