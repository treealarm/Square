import { useCallback } from 'react';

import { IconButton, Tooltip } from '@mui/material';
import AltRouteIcon from '@mui/icons-material/AltRoute';

import {
  LineStringType,
  LatLngPair,
  IGeometryDTO,
  IRoutDTO
} from '../store/Marker';
import * as TracksStore from '../store/TracksStates';
import { useAppDispatch } from '../store/configureStore';

interface IRequestRoute {
  geometry: IGeometryDTO|null;
}

export function RequestRoute(props: IRequestRoute) {

  let geometry: IGeometryDTO|null = props.geometry;

  const appDispatch = useAppDispatch();

  const requestSelectedRoute = useCallback(
    (geo: IGeometryDTO) => {

      var myFigure: LatLngPair[]|null = null;

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
      

    }, [appDispatch])

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