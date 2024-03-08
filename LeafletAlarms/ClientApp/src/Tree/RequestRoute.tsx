import * as React from 'react';

import { useCallback } from 'react';

import { ApplicationState } from '../store';
import { IconButton, Stack, TextField, Tooltip } from '@mui/material';
import AltRouteIcon from '@mui/icons-material/AltRoute';

import {
  PointType,
  LineStringType,
  PolygonType, ViewOption, LatLngPair,
  IGeometryDTO,
  IPointCoord,
  IRoutDTO
} from '../store/Marker';
import * as TracksStore from '../store/TracksStates';
import * as L from 'leaflet';
import { useAppDispatch } from '../store/configureStore';

declare module 'react-redux' {
  interface DefaultRootState extends ApplicationState { }
}

interface IRequestRoute {
  geometry: IGeometryDTO;
}

export function RequestRoute(props: IRequestRoute) {

  let geometry: IGeometryDTO = props.geometry;

  const appDispatch = useAppDispatch();

  const requestSelectedRoute = useCallback(
    (geo: IGeometryDTO, e: any) => {

      var myFigure: LatLngPair[];

      switch (geo.type) {
        case LineStringType:
          myFigure = geo.coord;
          break;
        default:
          break;
      }

      if (myFigure != null) {
        var routDto: IRoutDTO =
        {
          InstanceName: "RU-MOS",
          Profile: "bicycle",
          Coordinates: myFigure
        }
        appDispatch<any>(TracksStore.actionCreators.requestRoutesByLine(routDto));
      }
      

    }, [props])

  if (geometry?.type != LineStringType) {
    return null;
  }
  return (

      <Tooltip title={"Build route"} hidden={geometry == null}>
        <IconButton aria-label="search" size="medium" onClick={(e: any) => requestSelectedRoute(geometry, e)}>
          <AltRouteIcon fontSize="inherit" />
        </IconButton>
      </Tooltip>
  );
}