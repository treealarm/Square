﻿/* eslint-disable react-hooks/exhaustive-deps */
import * as React from 'react';
import * as L from 'leaflet';
import { useSelector } from "react-redux";
import * as ObjPropsStore from '../store/ObjPropsStates';
import * as MarkersStore from '../store/MarkersStates';
import { ApplicationState } from '../store';
import { DeepCopy, ICircle, ICommonFig, PointType, setExtraProp } from '../store/Marker';

import { useCallback, useEffect } from 'react'
import {
  Popup,
  useMap,
  useMapEvents,
  Circle
} from 'react-leaflet'

import { LeafletKeyboardEvent } from 'leaflet';
import { useAppDispatch } from '../store/configureStore';


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
              <span className="menu_item" onClick={(e: any) => props.DeleteVertex(props?.index, e)}>Delete vertex</span>
            </td></tr>
            <tr><td>
              <span className="menu_item" onClick={(e: any) => props.MoveVertex(props?.index, e)}>Move vertex</span>
            </td></tr>
            <tr><td>
              <span className="menu_item" onClick={(e: any) => props.FigureChanged(e)}>Save</span>
            </td></tr>
          </tbody>
        </table>
      </Popup>
    </React.Fragment>
  );
}


export function CircleMaker(props: any) {

  const appDispatch = useAppDispatch();
  const parentMap = useMap();


  const selected_id = useSelector((state: ApplicationState) => state?.guiStates?.selected_id);

  const [movedIndex, setMovedIndex] = React.useState(-1);

  const initFigure: ICircle = {
    name: 'New Circle',
    parent_id: selected_id,
    geometry: { coord: null, type: PointType },
    radius: 10,
    id: null
  }

  useEffect(() => {
    if (props.obj2Edit != null) {
      const obj2Edit: ICommonFig = props.obj2Edit;

      initFigure.name = obj2Edit.name;
      initFigure.parent_id = obj2Edit.parent_id;
      initFigure.geometry = obj2Edit.geometry;
      initFigure.id = obj2Edit.id;

      var radius2set = 100;

      if (obj2Edit.radius != null) {
          radius2set = obj2Edit.radius;
      }

      initFigure.radius = radius2set;

      if (initFigure?.geometry?.coord == null) {
        setMovedIndex(0)
      }
    }
  }, [props.obj2Edit]);

  const [figure, setFigure] = React.useState<ICircle>(initFigure);
  const [oldFigure, setOldFigure] = React.useState<ICircle>(initFigure);
  const objProps = useSelector((state: ApplicationState) => state?.objPropsStates?.objProps);

  useEffect(() => {
    var copy = DeepCopy(objProps);

    if (copy == null) {
      return;
    }

    setExtraProp(copy, "radius", figure.radius.toString(), null);
    appDispatch(MarkersStore.selectMarkerLocally(figure));
    appDispatch(ObjPropsStore.setObjPropsLocally(copy));
  }, [figure]);


  useMapEvents({
    click(e: any) {
      console.log('onclick map');
      var ll: L.LatLng = e.latlng as L.LatLng;

      if (movedIndex >= 0) {
        setMovedIndex(-1);
        return;
      }
      var updatedValue = { geometry: figure.geometry};
      updatedValue.geometry = { coord: [ll.lat, ll.lng], type: PointType };
      setFigure({
        ...figure,
        ...updatedValue
      });
    },

    mousemove(e: L.LeafletMouseEvent) {
      if (movedIndex >= 0) {
        var updatedValue = { geometry: figure.geometry };

        updatedValue.geometry = { coord: [e.latlng.lat, e.latlng.lng], type: PointType };

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
    (index: any, e: any) => {
      console.log(e.target.value);
      parentMap.closePopup();

      setMovedIndex(index);
    }, [])

  const deleteVertex = useCallback(
    () => {
      parentMap.closePopup();
      setFigure(initFigure);
    }, [figure])

  const colorOptions = {
    fillColor: 'yellow',
    fillOpacity: 0.5,
    color: 'yellow',
    opacity: 1,
    dashArray: '5,10'
  }

  const figureChanged = useCallback(
    (e: any) => {
      props.figureChanged(figure, e);
      setFigure(initFigure);
    }, [figure, initFigure, props]);

  if (figure?.geometry?.coord == null) {
    return null;
  }

  return (
    <React.Fragment>
      
      <div key={0}>
        <Circle
          pathOptions={colorOptions}
          center={figure.geometry.coord}
          radius={figure.radius}          
          key={0}
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

        </Circle>

        </div>      
    </React.Fragment>
  );
}
