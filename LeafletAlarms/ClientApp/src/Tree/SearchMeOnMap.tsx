import * as React from 'react';

import { useCallback } from 'react';

import { ApplicationState } from '../store';
import { Box, IconButton, TextField } from '@mui/material';
import FlareIcon from '@mui/icons-material/Flare';
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

declare module 'react-redux' {
  interface DefaultRootState extends ApplicationState { }
}

export function SearchMeOnMap(props: any) {

  let geometry: IGeometryDTO = props.geometry;
  let text: string = props.text;

  const appDispatch = useAppDispatch();

  const searchMeOnMap = useCallback(
    (geo: IGeometryDTO, e: any) => {

      var myFigure = null;
      var center: L.LatLng = null;

      switch (geo.type) {
        case PointType:
          var coord: LatLngPair = (geo as IPointCoord).coord;
          center = new L.LatLng(coord[0], coord[1]);
          break;
        case LineStringType:
          var coordArr: LatLngPair[] = geo.coord;
          myFigure = new L.Polyline(coordArr)
          break;
        case PolygonType:
          var coordArr: LatLngPair[] = geo.coord;
          myFigure = new L.Polygon(coordArr)
          break;
        default:
          break;
      }

      if (center == null) {
        let bounds: L.LatLngBounds = myFigure.getBounds();
        center = bounds.getCenter();
      }

      var viewOption: ViewOption = {
        map_center: [center.lat, center.lng],
        zoom: props.zoom_min
      };

      appDispatch<any>(GuiStore.actionCreators.setMapOption(viewOption));

    }, [props])


  return (
    <Box sx={{
      width: '100%',

      bgcolor: 'background.paper',
      overflow: 'auto',
      height: '100%',
      border: 1
    }}>

      <TextField
            fullWidth
            label='Id'
            size="small"
        value={text ? text : ''}
            inputProps={{ readOnly: true }}>
      </TextField>

      <IconButton aria-label="search" size="medium" onClick={(e: any) => searchMeOnMap(geometry, e)}>
        <FlareIcon fontSize="inherit" />
      </IconButton>
       
    </Box>
  );
}