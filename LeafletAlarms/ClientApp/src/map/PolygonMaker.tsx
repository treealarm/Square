import * as React from 'react';
import * as L from 'leaflet';
import { useDispatch, useSelector } from "react-redux";
import { ApplicationState } from '../store';
import { BoundBox, IPolygon, PolygonType, IObjProps, IArrayCoord, setExtraProp, getExtraProp } from '../store/Marker';
import * as ObjPropsStore from '../store/ObjPropsStates';

import { useCallback, useEffect } from 'react'
import {
  CircleMarker,
  Popup,
  useMap,
  useMapEvents,
  Polygon,
  Polyline
} from 'react-leaflet'

import { LeafletEvent, LeafletKeyboardEvent } from 'leaflet';

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
              <span className="menu_item" onClick={(e) => props.FigureChanged(e)}>Save Polygon</span>
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

  const dispatch = useDispatch();
  const parentMap = useMap();

  const selected_id = useSelector((state) => state?.guiStates?.selected_id);

  const [movedIndex, setMovedIndex] = React.useState(-1);
  const [isMoveAll, setIsMoveAll] = React.useState(false);
  const objProps = useSelector((state) => state?.objPropsStates?.objProps);

  const initPolygon: IPolygon = {
    id: null,
    name: 'New Polygon',
    parent_id: selected_id,
    geometry: {coord:[]},
    type: PolygonType
  };

  useEffect(() => {
    if (props.obj2Edit != null) {
      const obj2Edit: IObjProps = props.obj2Edit;

      initPolygon.name = obj2Edit.name;
      initPolygon.parent_id = obj2Edit.parent_id;
      initPolygon.geometry = JSON.parse(getExtraProp(obj2Edit, "geometry"));
      initPolygon.id = obj2Edit.id;
    }
  }, [props.obj2Edit]);

  const [curPolygon, setPolygon] = React.useState<IPolygon>(initPolygon);
  const [oldPolygon, setOldPolygon] = React.useState<IPolygon>(initPolygon);

  useEffect(() => {    
    var copy = Object.assign({}, objProps);

    if (copy == null) {
      return;
    }

    setExtraProp(copy,"geometry",JSON.stringify(curPolygon.geometry), null);
    dispatch(ObjPropsStore.actionCreators.setObjPropsLocally(copy));
  }, [curPolygon]);

    
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
      updatedValue = {
        geometry:
        {
          coord: [...curPolygon.geometry.coord, [ll.lat, ll.lng]]
        }          
      };

      setPolygon(polygon => ({
        ...polygon,
        ...updatedValue
      }));
    },

    moveend(e: LeafletEvent) {
    },

    mousemove(e: L.LeafletMouseEvent) {
      if (isMoveAll) {

        var shift_y = e.latlng.lat - oldPolygon.geometry.coord[0][0];
        var shift_x = e.latlng.lng - oldPolygon.geometry.coord[0][1];

        const updatedValue =
        {
          geometry:
          {
            coord: new Array<[number, number]>()
          }            
        }

        for (var _i = 0; _i < oldPolygon.geometry.coord.length; _i++) {
          updatedValue.geometry.coord.push(
            [
              shift_y + oldPolygon.geometry.coord[_i][0],
              shift_x + oldPolygon.geometry.coord[_i][1]
            ]);
        }

        setPolygon(polygon => ({
          ...polygon,
          ...updatedValue
        }));
      }

      if (movedIndex >= 0) {
        var updatedValue = {
          geometry:
          {
            coord: [...curPolygon.geometry.coord]
          }            
        };

        updatedValue.geometry.coord.splice(movedIndex, 1, [e.latlng.lat, e.latlng.lng]);

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

      var updatedValue = {
        geometry:
        {
          coord: [...curPolygon.geometry.coord]
        }          
      };
      updatedValue.geometry.coord.splice(index, 0, updatedValue.geometry.coord[index]);

      setPolygon(polygon1 => ({
        ...curPolygon,
        ...updatedValue
      }));

      setMovedIndex(index);
    }, [curPolygon])

  const deleteVertex = useCallback(
    (index, e) => {      
      parentMap.closePopup();

      var updatedValue = {
        geometry:
        {
          coord: [...curPolygon.geometry.coord]
        }          
      };
      updatedValue.geometry.coord.splice(index, 1);

      setPolygon(polygon1 => ({
        ...curPolygon,
        ...updatedValue
      }));

    }, [curPolygon])

  const colorOptions = { color: 'green' }
  const colorOptionsCircle = { color: 'red' }

  const figureChanged = useCallback(
    (e) => {
      props.figureChanged(curPolygon, e);
      setPolygon(initPolygon);
    }, [curPolygon])

  function moveAllPoints( e: React.MouseEventHandler)
  {
      parentMap.closePopup();
      setOldPolygon(curPolygon);
      setIsMoveAll(true);
  }
  

  return (
    <React.Fragment>
      {
        curPolygon.geometry.coord.map((point, index) =>
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
        curPolygon.geometry.coord.length > 2
          ? <Polygon pathOptions={colorOptions} positions={curPolygon.geometry.coord}>
            <PolygonPopup
              FigureChanged={figureChanged}
              MoveAllPoints={moveAllPoints}
              movedIndex={movedIndex}
              isMoveAll={isMoveAll}
            />
          </Polygon>
          : <Polyline pathOptions={colorOptions} positions={curPolygon.geometry.coord}>
            
          </Polyline>
      }
    </React.Fragment>
  );
}
