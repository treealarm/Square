import * as React from 'react';
import * as L from 'leaflet';
import { useDispatch, useSelector } from "react-redux";
import { ApplicationState } from '../store';
import { IPolygon, PolygonType, IObjProps, IPolygonCoord, setExtraProp, getExtraProp, DeepCopy } from '../store/Marker';
import * as ObjPropsStore from '../store/ObjPropsStates';

import { useCallback, useEffect, useMemo } from 'react'
import {
  CircleMarker,
  Popup,
  useMap,
  useMapEvents,
  Polygon,
  Polyline
} from 'react-leaflet'

import { LeafletKeyboardEvent } from 'leaflet';

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
              <span className="menu_item" onClick={(e: any) => props.AddVertex(props?.index, e)}>Add vertex</span>
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
              <span className="menu_item" onClick={(e: any) => props.FigureChanged(e)}>Save Polygon</span>
            </td></tr>
            <tr><td>
              <span className="menu_item" onClick={(e: any) => props.MoveAllPoints(e)}>Move Polygon</span>
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

  const selected_id = useSelector((state: ApplicationState) => state?.guiStates?.selected_id);

  const [click, setClick] = React.useState(0);

  const [movedIndex, setMovedIndex] = React.useState(-1);
  const [isMoveAll, setIsMoveAll] = React.useState(false);
  const objProps = useSelector((state: ApplicationState) => state?.objPropsStates?.objProps);

  const initPolygon: IPolygon = useMemo(()=>( {
    id: null,
    name: 'New Polygon',
    parent_id: selected_id,
    geometry: { coord: [], type: PolygonType }
  }),[selected_id]);

  function DoSetMovedIndex(index: number, place: any) {
    console.log("DoSetMovedIndex ", place, " ", index);
    setMovedIndex(index);
  }

  useEffect(() => {
    if (props.obj2Edit != null) {
      const obj2Edit: IObjProps = props.obj2Edit;

      initPolygon.name = obj2Edit.name;
      initPolygon.parent_id = obj2Edit.parent_id;
      initPolygon.geometry = JSON.parse(getExtraProp(obj2Edit, "geometry"));
      initPolygon.id = obj2Edit.id;
    }
  }, [initPolygon, props.obj2Edit]);

  const [curPolygon, setPolygon] = React.useState<IPolygon>(initPolygon);
  const [oldPolygon, setOldPolygon] = React.useState<IPolygon>(initPolygon);

  useEffect(() => {    
    var copy = DeepCopy(objProps);

    if (copy == null) {
      return;
    }

    setExtraProp(copy,"geometry",JSON.stringify(curPolygon.geometry), null);
    dispatch<any>(ObjPropsStore.actionCreators.setObjPropsLocally(copy));
  }, [curPolygon, dispatch, objProps]);

    
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

      if (isMoveAll) {
        setIsMoveAll(false);
        return;
      }      

      var geometry_upd: IPolygonCoord =
      {
        type: PolygonType,
        coord: [...curPolygon.geometry.coord, [ll.lat, ll.lng]]
      }          

      setPolygon(polygon => ({
        ...polygon,
        geometry: geometry_upd
      }));
    },


    mousemove(e: L.LeafletMouseEvent) {
      if (isMoveAll) {
        var shift_y = e.latlng.lat - oldPolygon.geometry.coord[0][0];
        var shift_x = e.latlng.lng - oldPolygon.geometry.coord[0][1];

        var updated_geometry: IPolygonCoord =
        {
          type: PolygonType,
          coord: new Array<[number, number]>()            
        }

        for (var _i = 0; _i < oldPolygon.geometry.coord.length; _i++) {
          updated_geometry.coord.push(
            [
              shift_y + oldPolygon.geometry.coord[_i][0],
              shift_x + oldPolygon.geometry.coord[_i][1]
            ]);
        }

        setPolygon(polygon => ({
          ...polygon,
          geometry: updated_geometry
        }));
      }

      if (movedIndex >= 0) {
         var updated_geometry1: IPolygonCoord =
        {
          coord: [...curPolygon.geometry.coord],
          type: PolygonType
        }            


        updated_geometry1.coord.splice(movedIndex, 1, [e.latlng.lat, e.latlng.lng]);

        setPolygon(polygon => ({
          ...polygon,
          geometry: updated_geometry1
        }));
      }
    },

    keydown(e: LeafletKeyboardEvent) {
      if (e.originalEvent.code == 'Escape') {
        if (movedIndex >= 0 || isMoveAll) {
          DoSetMovedIndex(-1, "keydown");
          setIsMoveAll(false);
          setPolygon(oldPolygon);
          setOldPolygon(initPolygon);
        }
      }
    }
  });

    
  const moveVertex = useCallback(
    (index: any) => {
      parentMap.closePopup();
      setOldPolygon(curPolygon);
      DoSetMovedIndex(index, "moveVertex");
      setClick(1);
    }, [curPolygon, parentMap])

  const addVertex = useCallback(
    (index: any) => {
      parentMap.closePopup();
      setOldPolygon(curPolygon);

      var geometry_upd: IPolygonCoord = 
      {
        type: PolygonType,
        coord: [...curPolygon.geometry.coord]
      }          
      console.log("addVertex:", index);
      geometry_upd.coord.splice(index, 0, geometry_upd.coord[index]);

      setPolygon({
        ...curPolygon,
        geometry: geometry_upd
      });

      DoSetMovedIndex(index, "AddVertex");
      setClick(1);
    }, [curPolygon, parentMap])

  const deleteVertex = useCallback(
    (index: any) => {      
      parentMap.closePopup();

      var geometry_upd: IPolygonCoord =
      {
        type: PolygonType,
        coord: [...curPolygon.geometry.coord]
      };
      
      geometry_upd.coord.splice(index, 1);
      console.log("deleteVertex:", index, " ", geometry_upd.coord.length);
      setPolygon({
        ...curPolygon,
        geometry: geometry_upd
      });

      setClick(1);

    }, [curPolygon, parentMap])

  const colorOptions = { color: 'green' }
  const colorOptionsCircle = { color: 'red' }

  const figureChanged = useCallback(
    (e: any) => {
      props.figureChanged(curPolygon, e);
      setPolygon(initPolygon);
    }, [curPolygon, initPolygon, props])

  function moveAllPoints()
  {
      parentMap.closePopup();
      setOldPolygon(curPolygon);
      setIsMoveAll(true);
      setClick(1);
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
