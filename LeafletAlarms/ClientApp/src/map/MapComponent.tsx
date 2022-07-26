import * as React from "react";
import { useDispatch, useSelector } from "react-redux";
import { LocationMarkers } from "./LocationMarkers";
import {
  MapContainer,
  TileLayer
} from "react-leaflet";

import { useEffect, useState } from "react";
import * as L from 'leaflet';
import { EditableFigure } from "./EditableFigure";
import { MapPositionChange } from "./MapPositionChange";

export function MapComponent() {

  return (
        <MapContainer
          center={[51.5359, -0.09]}
          zoom={13}
          scrollWheelZoom={true}
        >
          <LocationMarkers />
          <EditableFigure />
          <MapPositionChange/>
          <TileLayer
            attribution="&copy; <a href=&quot;https://www.openstreetmap.org/copyright&quot;>OpenStreetMap</a> contributors"
            url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
          />
        </MapContainer>
  );
}
