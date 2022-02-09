import React, {
  useCallback,
  useEffect,
  useMemo,
  useState,
  useRef,
} from "react";



import { Provider, useSelector } from 'react-redux';
import { configureStore } from './Store.ts';
import { useEvent } from './Events/EventContext'

import {
  MapContainer,
  TileLayer,
  Popup,
  Rectangle,
  Circle,
  CircleMarker,
  Polyline,
  Polygon,
  Marker,
  useMap,
  useMapEvents
} from "react-leaflet";
import usePrevious from "./Events/usePrevious";


const center = [51.505, -0.09];



const fillBlueOptions = { fillColor: "blue" };
const purpleOptions = { color: "purple" };
const redOptions = { color: "red" };

function LocationMarkers({timerVal}) {

  const initialMarkers = [[51.505, -0.09], [51.500, -0.091]];

  useEffect(() => {
    console.log('ComponentDidMount')
    for(var i = 0; i < 15000; i++)
    {
      
      const latlng = [51.5359 + i/10000, -0.093];
      initialMarkers.push(latlng);
    }
  }, [])

  
  const [markers, setMarkers] = useState(initialMarkers);


  const map = useMapEvents({
    click(e) {
      markers.push(e.latlng);
      setMarkers((prevValue) => [...prevValue, e.latlng]);
    }
  });
  const prevtimerVal = usePrevious(timerVal)

  const generateItemsFromAPI = useCallback(() => {
    
    //setMarkers((prevValue) => [...prevValue, latlng]);

  }, [timerVal])    

  if(prevtimerVal !== timerVal)
  {
    const latlng = [51.5359 + timerVal/10000, -0.093];
    markers.push(latlng);
  }
  return (
    <React.Fragment>
      {markers.map((marker, index) => <CircleMarker key={index} center={marker}><Popup>{index}</Popup></CircleMarker>)}
    </React.Fragment>
  );
}


const MyComponent = (props) => {
  const map = useMap();
  map.setView(props.center, map.getZoom());
  //console.log('map center:', props.temperature, map.getCenter())
  return       null
}

export function Page({timerVal, store}) {  

  const alert = useEvent()
  
  const working = useSelector((state) => state.working)
  const prevworking = usePrevious(working)

  return (
    <div>
    <MapContainer center={[51.5359 + timerVal/10000, -0.09]} zoom={13} scrollWheelZoom={false}>
    <Circle center={[51.5359 + timerVal/10000, -0.09]} pathOptions={redOptions} radius={200} />
    <MyComponent temperature={timerVal} center={[51.5359 + timerVal/10000, -0.09]}/>
    <LocationMarkers timerVal={timerVal} />
    <TileLayer
      attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
      url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
    />
  </MapContainer>
  </div>
  );
};
