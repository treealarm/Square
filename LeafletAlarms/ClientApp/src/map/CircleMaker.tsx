import * as React from 'react';
import * as L from 'leaflet';
import { useDispatch, useSelector } from "react-redux";
import * as ObjPropsStore from '../store/ObjPropsStates';
import { ApplicationState } from '../store';
import { ICircle, IObjProps, PointType } from '../store/Marker';

import { useCallback, useEffect } from 'react'
import {
  CircleMarker,
  Popup,
  useMap,
  useMapEvents,
  Circle
} from 'react-leaflet'

import { LeafletKeyboardEvent } from 'leaflet';

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


  const selected_id = useSelector((state) => state?.guiStates?.selected_id);

  const [movedIndex, setMovedIndex] = React.useState(-1);

  const initFigure: ICircle = {
    name: 'New Circle',
    parent_id: selected_id,
    geometry: null,
    type: PointType,
    radius: 10
  };

  useEffect(() => {
    if (props.obj2Edit != null) {
      const obj2Edit: IObjProps = props.obj2Edit;

      initFigure.name = obj2Edit.name;
      initFigure.parent_id = obj2Edit.parent_id;
      initFigure.geometry = JSON.parse(obj2Edit.geometry);
      initFigure.id = obj2Edit.id;

      var radius2set = 100;

      if (obj2Edit.extra_props != null) {
        var radius = obj2Edit.extra_props.find(p => p.prop_name == "radius");
        if (radius != null) {
          radius2set = parseInt(radius.str_val);
        }
      }

      initFigure.radius = radius2set;

      if (initFigure.geometry == null) {
        setMovedIndex(0)
      }
    }
  }, [props.obj2Edit]);

  const [figure, setFigure] = React.useState<ICircle>(initFigure);
  const [oldFigure, setOldFigure] = React.useState<ICircle>(initFigure);
  const objProps = useSelector((state) => state?.objPropsStates?.objProps);

  useEffect(() => {
    var copy = Object.assign({}, objProps);

    if (copy == null) {
      return;
    }

    copy.extra_props = [{
      prop_name: "radius",
      str_val: figure.radius.toString(),
      visual_type: null
    }];

    copy.geometry = JSON.stringify(figure.geometry);
    dispatch(ObjPropsStore.actionCreators.setObjPropsLocally(copy));
  }, [figure]);


  const mapEvents = useMapEvents({
    click(e) {
      console.log('onclick map');
      var ll: L.LatLng = e.latlng as L.LatLng;

      if (movedIndex >= 0) {
        setMovedIndex(-1);
        return;
      }
      var updatedValue = { geometry: figure.geometry};
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

  const colorOptions = {
    fillColor: 'yellow',
    fillOpacity: 0.5,
    color: 'yellow',
    opacity: 1,
    dashArray: '5,10'
  }

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
        <Circle
          pathOptions={colorOptions}
          center={figure.geometry}
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
