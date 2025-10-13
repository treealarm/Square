import * as React from "react";
import { useSelector } from "react-redux";
import {
  useMap, useMapEvents
} from "react-leaflet";

import { useEffect } from "react";
import { ApplicationState } from "../store";
import { useAppDispatch } from "../store/configureStore";

import * as GuiStore from '../store/GUIStates';
export function MapPositionChange() {

  const parentMap = useMap();
  const appDispatch = useAppDispatch();
  const guiStates = useSelector((state: ApplicationState) => state.guiStates);

  useEffect(() => {

    var container = document.getElementById('map-container')

    if (container == null) {
      return;
    }
    const resizeObserver = new ResizeObserver(() => {
      parentMap.invalidateSize();
    });
    resizeObserver.observe(container);

    return () => {
      if (container != null) {
        resizeObserver.unobserve(container);
      }
     
      resizeObserver.disconnect();
    };
  }, [parentMap]);

  

  useEffect(() => {
    let map_center = guiStates?.map_option?.map_center;
    let zoom = guiStates?.map_option?.zoom;

    if (map_center != null) {
      if (zoom != null) {
        parentMap.setView(map_center, zoom);
      } else {
        parentMap.setView(map_center);
      }
    }
    else {
      if (guiStates?.map_option?.find_current_pos) {
        parentMap.locate();
      }
    }
   
  }, [guiStates?.map_option, parentMap]);

  useMapEvents({
    moveend: () => {
      const center = parentMap.getCenter();
      const zoom = parentMap.getZoom();

      console.log("Map moved:", center, "Zoom:", zoom);

      appDispatch(
        GuiStore.updateCurMapOption({
          map_center: [center.lat, center.lng],
          zoom: zoom,
        })
      );
    },
    zoomend: () => {
      const center = parentMap.getCenter();
      const zoom = parentMap.getZoom();

      console.log("Zoom changed:", zoom, "Center:", center);

      appDispatch(
        GuiStore.updateCurMapOption({
          map_center: [center.lat, center.lng],
          zoom: zoom,
        })
      );
    },
    locationfound: (location:any) => {
      parentMap.setView(location.latlng);
      console.log('location found:', location)
    },
  });

  return (
    <React.Fragment>
    </React.Fragment >
  );
}



