import * as React from 'react';
import * as L from 'leaflet';
import { useDispatch, useSelector, useStore } from "react-redux";
import * as MarkersStore from '../store/MarkersStates';
import * as GuiStore from '../store/GUIStates';
import { ApplicationState } from '../store';
import { BoundBox, ICircle, IFigures, IPolygon, PolygonType } from '../store/Marker';
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

import { LeafletEvent, LeafletKeyboardEvent } from 'leaflet';
import { ObjectPopup } from './ObjectPopup';
import { PolygonTool } from '../store/EditStates';

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
            <tr><td>
              <span className="menu_item" onClick={(e) => props.AddVertex(props?.index, e)}>Add vertex</span>
            </td></tr>
          </tbody>
        </table>
      </Popup>
    </React.Fragment>
  );
}

function PolygonPopup(props: any) {

  if (props.movedIndex >= 0 || props.isMoveAll) {
    return null;
  }
  return (
    <React.Fragment>
      <Popup>
        <table>
          <tbody>
            <tr><td>
              <span className="menu_item" onClick={(e) => props.PolygonChanged(e)}>Save Polygon</span>
            </td></tr>
            <tr><td>
              <span className="menu_item" onClick={(e) => props.MoveAllPoints(e)}>Move Polygon</span>
            </td></tr>
          </tbody>
        </table>
      </Popup>
    </React.Fragment>
  );
}

export function PolygonMaker(props: any) {

  const parentMap = useMap();

  const selected_id = useSelector((state) => state?.guiStates?.selected_id);

  const [movedIndex, setMovedIndex] = React.useState(-1);
  const [isMoveAll, setIsMoveAll] = React.useState(false);

  const initPolygon: IPolygon = {
    name: 'New Polygon',
    parent_id: selected_id,
    geometry: [

    ],
    type: PolygonType
  };

  const [curPolygon, setPolygon] = React.useState<IPolygon>(initPolygon);
  const [oldPolygon, setOldPolygon] = React.useState<IPolygon>(initPolygon);

  useEffect(() => {
    if (props.obj2Edit != null) {
      setPolygon(props.obj2Edit);
    }
    else {
      setPolygon(initPolygon);
    }

  }, [props.obj2Edit]);

    
  const mapEvents = useMapEvents({
    click(e) {
      
      var ll: L.LatLng = e.latlng as L.LatLng;

      if (movedIndex >= 0) {
        setMovedIndex(-1);
        return;
      }

      if (isMoveAll) {
        setIsMoveAll(false);
        return;
      }
      

      let updatedValue = {};
      updatedValue = { geometry: [...curPolygon.geometry, [ll.lat, ll.lng]] };
      setPolygon(polygon => ({
        ...polygon,
        ...updatedValue
      }));
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
      if (isMoveAll) {

        var shift_y = e.latlng.lat - oldPolygon.geometry[0][0];
        var shift_x = e.latlng.lng - oldPolygon.geometry[0][1];

        const updatedValue =
        {
          geometry: []
        }

        for (var _i = 0; _i < oldPolygon.geometry.length; _i++) {
          updatedValue.geometry.push([shift_y + oldPolygon.geometry[_i][0], shift_x + oldPolygon.geometry[_i][1]]);
        }

        setPolygon(polygon => ({
          ...polygon,
          ...updatedValue
        }));
      }

      if (movedIndex >= 0) {
        var updatedValue = { geometry: [...curPolygon.geometry]};

        updatedValue.geometry.splice(movedIndex, 1, [e.latlng.lat, e.latlng.lng]);

        setPolygon(polygon => ({
          ...polygon,
          ...updatedValue
        }));
      }
    },

    keydown(e: LeafletKeyboardEvent) {
      if (e.originalEvent.code == 'Escape') {
        if (movedIndex >= 0 || isMoveAll) {
          setMovedIndex(-1);
          setIsMoveAll(false);
          setPolygon(oldPolygon);
          setOldPolygon(initPolygon);
        }
      }
    }
  });

    
  const moveVertex = useCallback(
    (index, e) => {
      parentMap.closePopup();
      setOldPolygon(curPolygon);
      setMovedIndex(index);
    }, [curPolygon])

  const addVertex = useCallback(
    (index, e) => {
      parentMap.closePopup();
      setOldPolygon(curPolygon);

      var updatedValue = { geometry: [...curPolygon.geometry] };
      updatedValue.geometry.splice(index, 0, updatedValue.geometry[index]);

      setPolygon(polygon1 => ({
        ...curPolygon,
        ...updatedValue
      }));

      setMovedIndex(index);
    }, [curPolygon])

  const deleteVertex = useCallback(
    (index, e) => {      
      parentMap.closePopup();

      var updatedValue = { geometry: [...curPolygon.geometry] };
      updatedValue.geometry.splice(index, 1);

      setPolygon(polygon1 => ({
        ...curPolygon,
        ...updatedValue
      }));

    }, [curPolygon])

  const markers = useSelector((state) => state?.markersStates?.markers);
  const colorOptions = { color: 'green' }
  const colorOptionsCircle = { color: 'red' }

  const polygonChanged = useCallback(
    (e) => {
      props.polygonChanged(curPolygon, e);
      setPolygon(initPolygon);
    }, [curPolygon])

  function moveAllPoints(e)
  {
      parentMap.closePopup();
      setOldPolygon(curPolygon);
      setIsMoveAll(true);
  }
  

  return (
    <React.Fragment>
      {
        curPolygon.geometry.map((point, index) =>
          <div key={index}>
            <CircleMarker
              key={index}
              center={point}
              pathOptions={colorOptionsCircle}
              radius={10}
              >
              {
                movedIndex < 0 && !isMoveAll
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
        curPolygon.geometry.length > 2 
          ? <Polygon pathOptions={colorOptions} positions={curPolygon.geometry}>
            <PolygonPopup
              PolygonChanged={polygonChanged}
              MoveAllPoints={moveAllPoints}
              movedIndex={movedIndex}
              isMoveAll={isMoveAll}
            />
            </Polygon>
          : <Polyline pathOptions={colorOptions} positions={curPolygon.geometry}>
            
          </Polyline>
      }
    </React.Fragment>
  );
}
