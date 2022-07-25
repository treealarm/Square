import * as React from "react";

import { LocationMarkers } from "./LocationMarkers";
import {
  MapContainer,
  TileLayer,
  Circle,
  Pane,
  Rectangle,
  ImageOverlay
} from "react-leaflet";
import { LatLngBounds } from "leaflet";

export function MapComponent() {
  const redOptions = { color: "red" };
  const bounds = new LatLngBounds([51.5359, -0.09], [51.55, -0.088])
  return (
        <MapContainer
          center={[51.5359, -0.09]}
          zoom={13}
          scrollWheelZoom={true}
        >
          <LocationMarkers />

          <TileLayer
            attribution="&copy; <a href=&quot;https://www.openstreetmap.org/copyright&quot;>OpenStreetMap</a> contributors"
            url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
          />
        </MapContainer>
  );
}
