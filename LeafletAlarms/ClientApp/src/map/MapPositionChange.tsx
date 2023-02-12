import * as React from "react";
import { useSelector } from "react-redux";
import {
  useMap
} from "react-leaflet";

import { useEffect } from "react";
import { ApplicationState } from "../store";


export function MapPositionChange() {

  const parentMap = useMap();

  const guiStates = useSelector((state: ApplicationState) => state.guiStates);

  useEffect(() => {
    let map_center = guiStates.map_option?.map_center;
    let zoom = guiStates.map_option?.zoom;
    if (map_center != null) {
      parentMap.setView(map_center, zoom);
    }
   
  }, [guiStates.map_option]);

  return (
    <React.Fragment>
    </React.Fragment >
  );
}



