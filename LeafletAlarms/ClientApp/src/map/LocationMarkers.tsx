import * as React from 'react';
import * as L from 'leaflet';
import { useDispatch, useSelector, useStore } from "react-redux";
import * as MarkersStore from '../store/MarkersStates';

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

import { Marker } from '../store/MarkersStates';
import { ApplicationState } from '../store';
import { Table } from 'reactstrap';

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

  //const [markers1, setMarkers] = useState(markers);

  const mapEvents = useMapEvents({
    click(e) {
      var ll: L.LatLng = e.latlng as L.LatLng;
      var marker: Marker = { points: [ll.lat, ll.lng], name: 'Initial' };
      //markers?.push(marker);
      //setMarkers((prevValue) => [...prevValue, marker]);

      dispatch(MarkersStore.actionCreators.sendMarker(marker));
    }
  });

  const map = useMap();

  const deleteMe = useCallback(
    (marker, e) => {
      console.log(e.target.value);
      //alert('delete ' + marker.name);
      map.closePopup();
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