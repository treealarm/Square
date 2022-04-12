import * as React from 'react';
import * as L from 'leaflet';
import { useDispatch, useSelector, useStore } from "react-redux";
import * as MarkersStore from '../store/MarkersStates';
import * as GuiStore from '../store/GUIStates';
import { ApplicationState } from '../store';
import { BoundBox, ICircle, IFigures, IPolygon } from '../store/Marker';
import { yellow } from '@mui/material/colors';

import { useCallback, useMemo, useState, useEffect } from 'react'
import {
  CircleMarker,
  Popup,
  useMap,
  useMapEvents,
  Circle,
  Polygon,
  Polyline
} from 'react-leaflet'

import { LeafletEvent } from 'leaflet';
import { ObjectPopup } from './ObjectPopup';

declare module 'react-redux' {
  interface DefaultRootState extends ApplicationState { }
}

export function PolygonMaker() {

  const dispatch = useDispatch();
  const parentMap = useMap();

  useEffect(() => {
    console.log('ComponentDidMount PolygonMaker');

  }, []);

  const selected_id = useSelector((state) => state?.guiStates?.selected_id);
  const selectedTool = useSelector((state) => state.editState.figure);

  const initPolygon: IPolygon = {
    name: 'New Polygon',
    parent_id: selected_id,
    geometry: [

    ],
    type: 'Polygon'
  };

  const [polygon, setPolygon] = React.useState<IPolygon>(initPolygon);

    
  const mapEvents = useMapEvents({
    click(e) {
      var ll: L.LatLng = e.latlng as L.LatLng;

      let updatedValue = {};
      updatedValue = { geometry: [...polygon.geometry, ll] };
      setPolygon(polygon => ({
        ...polygon,
        ...updatedValue
      }));
      if (selectedTool == 'Polygon') {

      }
    },

    moveend(e: LeafletEvent) {
      var bounds: L.LatLngBounds;
      bounds = e.target.getBounds();
      var boundBox: BoundBox = {
        wn: [bounds.getWest(), bounds.getNorth()],
        es: [bounds.getEast(), bounds.getSouth()],
        zoom: e.target.getZoom()
      };
    },
    mousemove(e: L.LeafletMouseEvent) {

    }
  });



  const eventHandlersCircle = useMemo(
    () => ({
      mouseover() {
        console.log('Mouse over polygon');
      }
    }),
    [],
  )

  const deleteMe = useCallback(
    (marker, e) => {
      console.log(e.target.value);
      parentMap.closePopup();
      let idsToDelete: string[] = [marker.id];
    }, [])

  const markers = useSelector((state) => state?.markersStates?.markers);
  const colorOptions = { color: 'green' }

  return (
    <React.Fragment>
      {
        polygon.geometry.map((point, index) =>
          <CircleMarker
            key={index}
            center={point}
            pathOptions={colorOptions}
            radius={10}
            eventHandlers={eventHandlersCircle}
          >
          </CircleMarker>
        )}
      {
        polygon.geometry.length > 2 
          ? <Polygon pathOptions={colorOptions} positions={polygon.geometry}>
              <ObjectPopup marker={polygon} deleteMe={deleteMe}>
              </ObjectPopup>
            </Polygon>
          : <Polyline pathOptions={colorOptions} positions={polygon.geometry}>
            
          </Polyline>
      }
    </React.Fragment>
  );
}
