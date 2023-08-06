import * as React from 'react';

import { useCallback } from 'react';

import { ApplicationState } from '../store';
import { IconButton, Stack, TextField, Tooltip } from '@mui/material';
import LocationSearchingIcon from '@mui/icons-material/LocationSearching';

import {
  PointType,
  LineStringType,
  PolygonType, ViewOption, LatLngPair,
  IGeometryDTO,
  IPointCoord
} from '../store/Marker';
import * as GuiStore from '../store/GUIStates';
import * as L from 'leaflet';
import { useAppDispatch } from '../store/configureStore';


export function FlyToMyLocationButton() {

  const appDispatch = useAppDispatch();
  const searchMeOnMap = useCallback(
    () => {

      var viewOption: ViewOption = {
        map_center: null,
        zoom: null,
        find_current_pos: true
      };

      appDispatch<any>(GuiStore.actionCreators.setMapOption(viewOption));
    },[]);

  return (
    <Tooltip title={"Search me on Map"}>
      <IconButton aria-label="search" size="medium" onClick={(e: any) => searchMeOnMap()}>
        <LocationSearchingIcon fontSize="inherit" />
      </IconButton>
    </Tooltip>
  );
}