import * as React from "react";
import { LocationMarkers } from "./LocationMarkers";
import {
  MapContainer,
  TileLayer
} from "react-leaflet";

import { EditableFigure } from "./EditableFigure";
import { MapPositionChange } from "./MapPositionChange";
import { TrackViewer } from "../tracks/TrackViewer";
import { Map as LeafletMap } from 'leaflet';

export function MapComponent(props: any) {

  const setMap = (map: LeafletMap) => {
    const resizeObserver = new ResizeObserver(() => {
      map.invalidateSize();
    });

    var container = document.getElementById('map-container')
    resizeObserver.observe(container)
  }

  var url = 'http://';

  if (window.location.protocol == "https:") {
    url = 'https://';
  }

  url = url
    + window.location.hostname
    + ':'
    + window.location.port
    + '/api/Map/GetTiles'
    + '/{z}/{x}/{y}.png';

  var url_classic = "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png";

  return (
    <MapContainer
      center={[55.752696480817086, 37.583007383349745]}
      zoom={13}
      scrollWheelZoom={true}
      id='map-container'
      whenCreated={setMap}
    >

      <TrackViewer />
      <LocationMarkers />
      
      <EditableFigure />
      <MapPositionChange />

      <TileLayer
        attribution="&copy; <a href=&quot;https://www.openstreetmap.org/copyright&quot;>OpenStreetMap</a> contributors"
        url={url}
      />
    </MapContainer>
  );
}
