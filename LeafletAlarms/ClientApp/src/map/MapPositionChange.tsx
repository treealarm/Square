import * as React from "react";
import { useSelector } from "react-redux";
import {
  useMap, useMapEvents
} from "react-leaflet";

import { useEffect } from "react";
import { ApplicationState } from "../store";
import { Map as LeafletMap } from 'leaflet';

export function MapPositionChange() {

  const parentMap = useMap();

  useEffect(() => {

    var container = document.getElementById('map-container')

    const resizeObserver = new ResizeObserver(() => {
      parentMap.invalidateSize();
    });
    resizeObserver.observe(container);

    return () => {
      resizeObserver.unobserve(container);
      resizeObserver.disconnect();
    };
  }, [parentMap]);

  const guiStates = useSelector((state: ApplicationState) => state.guiStates);

  useEffect(() => {
    let map_center = guiStates.map_option?.map_center;
    let zoom = guiStates.map_option?.zoom;

    if (map_center != null) {
      if (zoom != null) {
        parentMap.setView(map_center, zoom);
      } else {
        parentMap.setView(map_center);
      }
    }
    else {
      if (guiStates.map_option.find_current_pos) {
        parentMap.locate();
      }
    }
   
  }, [guiStates.map_option]);

  const map = useMapEvents({
    locationfound: (location) => {
      parentMap.setView(location.latlng);
      console.log('location found:', location)
    },
  });

  return (
    <React.Fragment>
    </React.Fragment >
  );
}



