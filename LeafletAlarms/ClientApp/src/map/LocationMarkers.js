import * as React from 'react';

import {
  useEffect,
  useState,
} from "react";

import {
  Popup,
  CircleMarker,
  useMapEvents
} from "react-leaflet";


export function LocationMarkers() {

  const initialMarkers = [[51.505, -0.09], [51.500, -0.091]];

  useEffect(() => {
    console.log('ComponentDidMount')
  }, [])

  const [markers, setMarkers] = useState(initialMarkers);

  const map = useMapEvents({
    click(e) {
      markers.push(e.latlng);
      setMarkers((prevValue) => [...prevValue, e.latlng]);
    }
  });

  return (
    <React.Fragment>
      {markers.map((marker, index) => <CircleMarker key={index} center={marker}><Popup>{index}</Popup></CircleMarker>)}
    </React.Fragment>
  );
}