﻿/* eslint-disable react-hooks/exhaustive-deps */
import * as React from 'react';
import * as L from 'leaflet';
import { useSelector } from "react-redux";
import { ApplicationState } from '../store';
import { DeepCopy, ICommonFig, IPolyline, IPolylineCoord, LineStringType } from '../store/Marker';

import { useCallback, useMemo, useEffect } from 'react'
import {
  CircleMarker,
  Popup,
  useMap,
  useMapEvents,
  Polyline
} from 'react-leaflet'

import { LeafletKeyboardEvent } from 'leaflet';
import * as ObjPropsStore from '../store/ObjPropsStates';
import { useAppDispatch } from '../store/configureStore';
import * as MarkersStore from '../store/MarkersStates';

function CirclePopup(props: any) {
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
              <span className="menu_item" onClick={(e: any) => props.AddVertex(props?.index,false, e)}>Insert vertex</span>
            </td></tr>
            <tr><td>
              <span className="menu_item" onClick={(e: any) => props.AddVertex(props?.index,true, e)}>Add vertex</span>
            </td></tr>
          </tbody>
        </table>
      </Popup>
    </React.Fragment>
  );
}

function FigurePopup(props: any) {
  if (props.movedIndex >= 0) {
    return null;
  }
  return (
    <React.Fragment>
      <Popup>
        <table>
          <tbody>
            <tr><td>
              <span className="menu_item" onClick={(e: any) => props.FigureChanged(e)}>Save</span>
            </td></tr>
          </tbody>
        </table>
      </Popup>
    </React.Fragment>
  );
}

export function PolylineMaker(props: any) {

  const appDispatch = useAppDispatch();
  const parentMap = useMap();

  const selected_id = useSelector((state: ApplicationState) => state?.guiStates?.selected_id);

  const [movedIndex, setMovedIndex] = React.useState(-1);
  const [click, setClick] = React.useState(0);
  function DoSetMovedIndex(index: number, place:string) {
    console.log("DoSetMovedIndex ", place, " ", index);
    setMovedIndex(index);
  }

  const initFigure: IPolyline = {
    id: null,
    name: 'New Polyline',
    parent_id: selected_id,
    geometry: {
      type: LineStringType,
      coord: []
    }
  };


  useEffect(() => {
    if (props.obj2Edit != null) {
      const obj2Edit: ICommonFig = props.obj2Edit;

      initFigure.name = obj2Edit.name;
      initFigure.parent_id = obj2Edit.parent_id;
      initFigure.geometry = obj2Edit.geometry;
      initFigure.id = obj2Edit.id;
    }
  }, [props.obj2Edit]);

  const [figure, setFigure] = React.useState<IPolyline>(initFigure);
  const [oldFigure, setOldFigure] = React.useState<IPolyline>(initFigure);
  const objProps = useSelector((state: ApplicationState) => state?.objPropsStates?.objProps);

  useEffect(() => {
    var copy = DeepCopy(objProps);

    if (copy == null) {
      return;
    }

    appDispatch(MarkersStore.selectMarkerLocally(figure));
    appDispatch(ObjPropsStore.setObjPropsLocally(copy));
  }, [figure]);
    
  useMapEvents({
    click(e: any) {

      if (click == 1) {
        setClick(0);
        return;
      }
      var ll: L.LatLng = e.latlng as L.LatLng;

      if (movedIndex >= 0) {
        DoSetMovedIndex(-1, "click");
        return;
      }
      
      var geometry_upd: IPolylineCoord = {
        coord: [...figure.geometry.coord, [ll.lat, ll.lng]],
        type: LineStringType
        }

      setFigure({
        ...figure,
        geometry: geometry_upd
      });
    },

    mousemove(e: L.LeafletMouseEvent) {
      if (movedIndex >= 0) {

        var geometry_upd: IPolylineCoord = {
          type: LineStringType,
            coord: [...figure.geometry.coord]
        };

        geometry_upd.coord.splice(movedIndex, 1, [e.latlng.lat, e.latlng.lng]);

        setFigure(polygon => ({
          ...polygon,
          geometry: geometry_upd
        }));
      }
    },

    keydown(e: LeafletKeyboardEvent) {
      if (e.originalEvent.code == 'Escape') {
        if (movedIndex >= 0) {
          DoSetMovedIndex(-1, "keydown");
          setFigure(oldFigure);
          setOldFigure(initFigure);
        }
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
    (index: any, e: any) => {
      console.log(e.target.value);
      parentMap.closePopup();
      
      DoSetMovedIndex(index, "moveVertex");
      setClick(1);
    }, [figure])

  const addVertex = useCallback(
    (index: any, toEnd: any) => {
      parentMap.closePopup();
      setOldFigure(figure);

      var geometry_upd: IPolylineCoord = {
        type: LineStringType,
        coord: [...figure.geometry.coord]
      };

      geometry_upd.coord.splice(index, 0, geometry_upd.coord[index]);

      setFigure({
        ...figure,
        geometry: geometry_upd
      });

      if (toEnd) {
        index++;
      }

      DoSetMovedIndex(index, "AddVertex");
      setClick(1);
    }, [figure])

  const deleteVertex = useCallback(
    (index: any) => {      
      parentMap.closePopup();

      var geometry_upd: IPolylineCoord = {
        type: LineStringType,
        coord: [...figure.geometry.coord]
      };
    
      geometry_upd.coord.splice(index, 1);

      setFigure(polygon => ({
        ...polygon,
        geometry: geometry_upd
      }));

      setClick(1);

    }, [figure])

  const colorOptions = { color: 'green' }

  const figureChanged = useCallback(
    (e: any) => {
      props.figureChanged(figure, e);
      setFigure(initFigure);
    }, [figure, initFigure, props])

  return (
    <React.Fragment>
      {
        figure.geometry.coord?.map((point, index) =>
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
                    AddVertex={addVertex}
                  >
                  </CirclePopup> : < div />
              }
              
            </CircleMarker>
          </div>
        )
      }
      {
        figure.geometry.coord?.length > 1 
          ?
          <Polyline pathOptions={colorOptions} positions={figure.geometry.coord}>
            <FigurePopup
              FigureChanged={figureChanged}
              movedIndex={movedIndex}
            />
          </Polyline>
          :<div/>
      }
    </React.Fragment>
  );
}
