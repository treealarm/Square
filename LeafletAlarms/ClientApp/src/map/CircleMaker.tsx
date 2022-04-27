import * as React from 'react';
import * as L from 'leaflet';
import { useDispatch, useSelector, useStore } from "react-redux";
import * as MarkersStore from '../store/MarkersStates';
import * as GuiStore from '../store/GUIStates';
import { ApplicationState } from '../store';
import { BoundBox, ICircle, IFigures, IPolyline, IPolygon, PointType } from '../store/Marker';
import { yellow } from '@mui/material/colors';

import { useCallback, useMemo, useState, useEffect } from 'react'
import {
  CircleMarker,
  Popup,
  useMap,
  useMapEvents,
  Marker,
  Polygon,
  Polyline,
  Circle
} from 'react-leaflet'

import { LeafletEvent, LeafletKeyboardEvent } from 'leaflet';
import { ObjectPopup } from './ObjectPopup';
import { PolylineTool, PolygonTool } from '../store/EditStates';

declare module 'react-redux' {
  interface DefaultRootState extends ApplicationState { }
}

function CirclePopup(props: any) {
  if (props.movedIndex >= 0) {
    return null;
  }
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
            <tr><td>
              <span className="menu_item" onClick={(e) => props.FigureChanged(e)}>Save</span>
            </td></tr>
          </tbody>
        </table>
      </Popup>
    </React.Fragment>
  );
}


export function CircleMaker(props: any) {

  const dispatch = useDispatch();
  const parentMap = useMap();

  useEffect(() => {
    console.log('ComponentDidMount CircleMaker');

  }, []);

  const selected_id = useSelector((state) => state?.guiStates?.selected_id);
  const selectedTool = useSelector((state) => state.editState.figure);

  const [movedIndex, setMovedIndex] = React.useState(-1);

  const initFigure: ICircle = {
    name: 'New Circle',
    parent_id: selected_id,
    geometry: null,
    type: PointType
  };

  const [figure, setFigure] = React.useState<ICircle>(initFigure);
  const [oldFigure, setOldFigure] = React.useState<ICircle>(initFigure);

  useEffect(() => {
    if (props.obj2Edit != null) {
      setFigure(props.obj2Edit);
    }
    else {
      setFigure(initFigure);
    }

  }, [props.obj2Edit]);

  const mapEvents = useMapEvents({
    click(e) {
      console.log('onclick map');
      var ll: L.LatLng = e.latlng as L.LatLng;

      if (movedIndex >= 0) {
        setMovedIndex(-1);
        return;
      }
      var updatedValue = { geometry: figure.geometry };
      updatedValue.geometry = [ll.lat, ll.lng];
      setFigure(fig => ({
        ...figure,
        ...updatedValue
      }));
    },

    mousemove(e: L.LeafletMouseEvent) {
      if (movedIndex >= 0) {
        var updatedValue = { geometry: figure.geometry };

        updatedValue.geometry = [e.latlng.lat, e.latlng.lng];

        setFigure(figure => ({
          ...figure,
          ...updatedValue
        }));
      }
    },

    keydown(e: LeafletKeyboardEvent) {
      if (e.originalEvent.code == 'Escape') {
        if (movedIndex >= 0) {
          setMovedIndex(-1);
          setFigure(oldFigure);
          setOldFigure(initFigure);
        }
      }
    }
  });


  const moveVertex = useCallback(
    (index, e) => {
      console.log(e.target.value);
      parentMap.closePopup();

      setMovedIndex(index);
    }, [])

  const deleteVertex = useCallback(
    (index, e) => {
      parentMap.closePopup();
      setFigure(initFigure);
    }, [figure])

  const colorOptions = { color: 'green' }

  const figureChanged = useCallback(
    (e) => {
      props.figureChanged(figure, e);
      setFigure(initFigure);
    }, [figure]);

  if (figure.geometry == null) {
    return null;
  }

  return (
    <React.Fragment>
      
      <div key={0}>
        <Circle pathOptions={colorOptions} center={figure.geometry} radius={1000}>
        </Circle>
        <CircleMarker
          key={0}
          center={figure.geometry}
          pathOptions={colorOptions}
          radius={10}
          >
          {
            movedIndex < 0
            ? <CirclePopup
                index={0}
                MoveVertex={moveVertex}
                DeleteVertex={deleteVertex}
                FigureChanged={figureChanged}
                movedIndex={movedIndex}
              >
            </CirclePopup> : < div />
          }

        </CircleMarker>

        </div>      
    </React.Fragment>
  );
}
