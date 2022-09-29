import * as React from "react";
import { LocationMarkers } from "./LocationMarkers";
import {
  MapContainer,
  TileLayer
} from "react-leaflet";

import { EditableFigure } from "./EditableFigure";
import { MapPositionChange } from "./MapPositionChange";
import { TrackViewer } from "../tracks/TrackViewer";

export function MapComponent() {

  return (
    <MapContainer
      center={[55.752696480817086, 37.583007383349745]}
      zoom={13}
      scrollWheelZoom={true}    >
      <TrackViewer />
      <LocationMarkers />
      <EditableFigure />
      <MapPositionChange />

      <TileLayer
        attribution="&copy; <a href=&quot;https://www.openstreetmap.org/copyright&quot;>OpenStreetMap</a> contributors"
        url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
      />
    </MapContainer>
  );
}
