import { useCallback } from 'react';

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

interface ISearchMeOnMapProps {
  geometry: IGeometryDTO;
  text: string;
  zoom_min?:number
}

export function SearchMeOnMap(props: ISearchMeOnMapProps) {

  let geometry: IGeometryDTO = props.geometry;

  let text: string = props.text;

  const appDispatch = useAppDispatch();

  const searchMeOnMap = useCallback(
    (geo: IGeometryDTO) => {

      var myFigure = null;
      var center: L.LatLng = null;

      switch (geo.type) {
        case PointType:
          var coord: LatLngPair = (geo as IPointCoord).coord;
          center = new L.LatLng(coord[0], coord[1]);
          break;
        case LineStringType:
          {
            var coordArr: LatLngPair[] = geo.coord;
            myFigure = new L.Polyline(coordArr)
            break;
          }          
        case PolygonType:
          {
            var coordArr1: LatLngPair[] = geo.coord;
            myFigure = new L.Polygon(coordArr1)
            break;
          }          
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

      appDispatch(GuiStore.setMapOption(viewOption));

    }, [appDispatch, props.zoom_min])

  return (
    <Stack direction="row" spacing={2}
      justifyContent="space-around"
      sx={{
        width: '100%',
      }}>

      <TextField
            fullWidth
            label='Id'
            size="small"
        value={text ? text : ''}
            inputProps={{ readOnly: true }}>
      </TextField>

      <Tooltip title={"Find object on map"} hidden={geometry == null}>
      <IconButton aria-label="search" size="medium" onClick={() => searchMeOnMap(geometry)}>
        <LocationSearchingIcon fontSize="inherit" />
        </IconButton>
      </Tooltip>
    </Stack>
  );
}