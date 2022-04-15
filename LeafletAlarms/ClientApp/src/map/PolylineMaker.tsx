import * as React from 'react';
import * as L from 'leaflet';
import { useDispatch, useSelector, useStore } from "react-redux";
import * as MarkersStore from '../store/MarkersStates';
import * as GuiStore from '../store/GUIStates';
import { ApplicationState } from '../store';
import { BoundBox, ICircle, IFigures, IPolyline, IPolygon } from '../store/Marker';
import { yellow } from '@mui/material/colors';

import { useCallback, useMemo, useState, useEffect } from 'react'
import {
  CircleMarker,
  Popup,
  useMap,
  useMapEvents,
  Marker,
  Polygon,
  Polyline
} from 'react-leaflet'

import { LeafletEvent } from 'leaflet';
import { ObjectPopup } from './ObjectPopup';
import { PolylineTool, PolygonTool } from '../store/EditStates';

declare module 'react-redux' {
  interface DefaultRootState extends ApplicationState { }
}

function CirclePopup(props: any) {
  return (
    <React.Fragment>
      <Popup>
        <table>
          <tbody>
            <tr><td>
              <span className="menu_item" onClick={(e) => props.DeleteVertex(props?.index, e)}>Delete vertex</span>
            </td></tr>
            <tr><td>
              <span className="menu_item" onClick={(e) => props.MoveVertex(props?.index, e)}>Move vertex</span>
            </td></tr>
          </tbody>
        </table>
      </Popup>
    </React.Fragment>
  );
}

function FigurePopup(props: any) {
  return (
    <React.Fragment>
      <Popup>
        <table>
          <tbody>
            <tr><td>
              <span className="menu_item" onClick={(e) => props.FigureChanged(e)}>Save</span>
            </td></tr>
          </tbody>
        </table>
      </Popup>
    </React.Fragment>
  );
}

export function PolylineMaker(props: any) {

  const dispatch = useDispatch();
  const parentMap = useMap();

  useEffect(() => {
    console.log('ComponentDidMount PolylineMaker');

  }, []);

  const selected_id = useSelector((state) => state?.guiStates?.selected_id);
  const selectedTool = useSelector((state) => state.editState.figure);

  const [movedIndex, setMovedIndex] = React.useState(-1);

  const initFigure: IPolyline = {
    name: 'New Polyline',
    parent_id: selected_id,
    geometry: [

    ],
    type: 'LineString'
  };

  const [figure, setFigure] = React.useState<IPolyline>(initFigure);

    
  const mapEvents = useMapEvents({
    click(e) {
      console.log('onclick map');
      var ll: L.LatLng = e.latlng as L.LatLng;

      if (movedIndex >= 0) {
        setMovedIndex(-1);
        return;
      }
      

      let updatedValue = {};
      updatedValue = { geometry: [...figure.geometry, [ll.lat, ll.lng]] };
      setFigure(polygon => ({
        ...figure,
        ...updatedValue
      }));
      if (selectedTool == PolylineTool) {

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
      if (movedIndex >= 0) {
        var updatedValue = { geometry: [...figure.geometry]};

        updatedValue.geometry.splice(movedIndex, 1, [e.latlng.lat, e.latlng.lng]);

        setFigure(polygon => ({
          ...polygon,
          ...updatedValue
        }));
      }
    }
  });



  const eventHandlersCircle = useMemo(
    () => ({
      mouseover() {
        
      },
      onclick() {
        console.log('onclick circle');
      }
    }),
    [],
  )

  const moveVertex = useCallback(
    (index, e) => {
      console.log(e.target.value);
      parentMap.closePopup();
      
      setMovedIndex(index);
    }, [])

  const deleteVertex = useCallback(
    (index, e) => {      
      parentMap.closePopup();

      var updatedValue = { geometry: [...figure.geometry] };
      updatedValue.geometry.splice(index, 1);

      setFigure(polygon => ({
        ...polygon,
        ...updatedValue
      }));

    }, [figure])

  const markers = useSelector((state) => state?.markersStates?.markers);
  const colorOptions = { color: 'green' }

  const figureChanged = useCallback(
    (e) => {
      props.figureChanged(figure, e);
      setFigure(initFigure);
    }, [figure])

  return (
    <React.Fragment>
      {
        figure.geometry.map((point, index) =>
          <div key={index}>
            <CircleMarker
              key={index}
              center={point}
              pathOptions={colorOptions}
              radius={10}
              eventHandlers={eventHandlersCircle}>
              {
                movedIndex < 0
                  ? <CirclePopup
                    index={index}
                    MoveVertex={moveVertex}
                    DeleteVertex={deleteVertex}
                  >
                  </CirclePopup> : < div />
              }
              
            </CircleMarker>
          </div>
        )
      }
      {
        figure.geometry.length > 1 
          ? 
          <Polyline pathOptions={colorOptions} positions={figure.geometry}>
            <FigurePopup FigureChanged={figureChanged} />
          </Polyline>
          :<div/>
      }
    </React.Fragment>
  );
}
