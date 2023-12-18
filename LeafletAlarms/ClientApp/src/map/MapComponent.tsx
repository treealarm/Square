import * as React from "react";
import { LocationMarkers } from "./LocationMarkers";
import {
  MapContainer,
  TileLayer
} from "react-leaflet";

import { EditableFigure } from "./EditableFigure";
import { MapPositionChange } from "./MapPositionChange";
import { TrackViewer } from "../tracks/TrackViewer";
import L from "leaflet";
import { createLayerComponent, LayerProps } from '@react-leaflet/core'
import { ReactNode } from "react";




export function MapComponent(props: any) {

  var url = 'http://';

  if (window.location.protocol == "https:") {
    url = 'https://';
  }

  url = url
    + window.location.hostname
    + ':'
    + window.location.port
    + '/api/Map/GetTiles'
    + '/layer/{z}/{x}/{y}.png';


  //var url_classic = "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png";
  const center = new L.LatLng(55.752696480817086, 37.583007383349745);

  //const createWebGisLayer = (props: any, context: any) => {

  //  const instance = new L.TileLayer(props.url, { ...props })

  //  return { instance, context }

  //}

  //const updateWebGisLayer = (instance: any, props: any, prevProps: any) => {

  //  if (prevProps.url !== props.url) {
  //    if (instance.setUrl) instance.setUrl(props.url)
  //  }
  //}

  //const WebGisLayer = createLayerComponent(createWebGisLayer, updateWebGisLayer);

  return (
    <MapContainer
      center={center}
      zoom={13}
      scrollWheelZoom={true}
      id='map-container'
      
    >

      <TrackViewer />
      <LocationMarkers />
      
      <EditableFigure />
      <MapPositionChange />

      <TileLayer
        maxZoom={20}
        attribution="&copy; <a href=&quot;https://www.openstreetmap.org/copyright&quot;>OpenStreetMap</a> contributors"
        url={url}
        key={1}
      />
    </MapContainer>
  );
}
