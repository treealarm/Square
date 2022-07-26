import * as React from "react";
import { useDispatch, useSelector } from "react-redux";
import { LocationMarkers } from "./LocationMarkers";
import {
  MapContainer,
  TileLayer,
  useMap
} from "react-leaflet";

import { useEffect, useState } from "react";
import * as L from 'leaflet';

export function MapPositionChange() {

  const parentMap = useMap();

  const guiStates = useSelector((state) => state.guiStates);

  useEffect(() => {
    let map_center = guiStates.map_option?.map_center;

    if (map_center != null) {
      parentMap.setView(map_center);
    }
   
  }, [guiStates.map_option?.map_center]);

  return (
    <React.Fragment>
    </React.Fragment >
  );
}



