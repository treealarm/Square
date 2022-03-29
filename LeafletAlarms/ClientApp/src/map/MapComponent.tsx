import * as React from 'react';

import { LocationMarkers } from './LocationMarkers'
import {
  MapContainer,
  TileLayer,
  Circle,
  Pane,
  Rectangle,
} from "react-leaflet";



export function MapComponent() {

  const redOptions = { color: "red" };

  return (
    <div>
      <MapContainer center={[51.5359, -0.09]} zoom={13} scrollWheelZoom={false}>
        <Circle center={[51.5359, -0.09]} pathOptions={redOptions} radius={200} />
        <LocationMarkers />

        <TileLayer
          attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
          url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
        />
      </MapContainer>
    </div>
  );
};