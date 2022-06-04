import * as React from 'react';
import * as L from 'leaflet';
import { useDispatch, useSelector, useStore} from "react-redux";
import * as MarkersStore from '../store/MarkersStates';
import * as GuiStore from '../store/GUIStates';
import { ApplicationState } from '../store';
import { BoundBox, ICircle, IFigures, IPolyline, IPolygon, PointType, PolygonType, LineStringType, Marker } from '../store/Marker';
import * as EditStore from '../store/EditStates';

import { useCallback, useMemo, useState, useEffect } from 'react'
import {
  useMap,
  useMapEvents,
  Circle,
  Polygon,
  Polyline
} from 'react-leaflet'


import { LeafletEvent, LeafletMouseEvent } from 'leaflet';
import { CircleTool, Figures, PolylineTool, PolygonTool } from '../store/EditStates';
import { ObjectPopup } from './ObjectPopup';
import { PolygonMaker } from './PolygonMaker';
import { PolylineMaker } from './PolylineMaker';
import { CircleMaker } from './CircleMaker';

declare module 'react-redux' {
  interface DefaultRootState extends ApplicationState { }
}

function MyPolygon(props: any) {
  if (props.hidden == true) {
    return null;
  }
  const dispatch = useDispatch();
  const eventHandlers = useMemo(
    () => ({
      click(event: LeafletMouseEvent) {
        var selected_id = event.target.options.marker.id;
        dispatch(GuiStore.actionCreators.selectTreeItem(selected_id));
      }
    }),
    [],
  )

  return (
    <React.Fragment>
      <Polygon
        marker={props.marker}
        pathOptions={props.pathOptions}
        positions={props.positions}
        eventHandlers={eventHandlers}
      >
        <ObjectPopup
          marker={props.marker}
          selectMe={props.selectMe}
          updateBaseMarker={props.updateBaseMarker}>
        </ObjectPopup>
      </Polygon>
    </React.Fragment>
  );
}

function MyPolyline(props: any) {
  if (props.hidden == true) {
    return null;
  }

  const dispatch = useDispatch();
  const eventHandlers = useMemo(
    () => ({
      click(event: LeafletMouseEvent) {
        var selected_id = event.target.options.marker.id;
        dispatch(GuiStore.actionCreators.selectTreeItem(selected_id));
      }
    }),
    [],
  )

  return (
    <React.Fragment>
      <Polyline
        marker={props.marker}
        pathOptions={props.pathOptions}
        positions={props.positions}
        eventHandlers={eventHandlers}
      >
        <ObjectPopup
          marker={props.marker}
          updateBaseMarker={props.updateBaseMarker}>
        </ObjectPopup>
      </Polyline>
    </React.Fragment>
  );
}

function MyCircle(props: any) {
  if (props.hidden == true) {
    return null;
  }

  const dispatch = useDispatch();
  const eventHandlers = useMemo(
    () => ({
      click(event: LeafletMouseEvent) {
        var selected_id = event.target.options.marker.id;
        dispatch(GuiStore.actionCreators.selectTreeItem(selected_id));
      }
    }),
    [],
  )

  return (
    <React.Fragment>
      <Circle
        marker={props.marker}
        pathOptions={props.pathOptions}
        center={props.center}
        radius={props.radius}
        eventHandlers={eventHandlers}
      >
        <ObjectPopup
          marker={props.marker}
          updateBaseMarker={props.updateBaseMarker}>
        </ObjectPopup>
      </Circle>
    </React.Fragment>
  );
}

export function LocationMarkers() {

  const dispatch = useDispatch();
  const parentMap = useMap();
  
  useEffect(() => {
    console.log('ComponentDidMount LocationMarkers');
    var bounds: L.LatLngBounds;
    bounds = parentMap.getBounds();
    var boundBox: BoundBox = {
      wn: [bounds.getWest(), bounds.getNorth()],
      es: [bounds.getEast(), bounds.getSouth()],
      zoom: parentMap.getZoom()
    };
    dispatch(MarkersStore.actionCreators.requestMarkers(boundBox));
  }, []);

  const selected_id = useSelector((state) => state?.guiStates?.selected_id);
  const checked_ids = useSelector((state) => state?.guiStates?.checked);
  const selectedTool = useSelector((state) => state.editState.figure);

  const guiStates = useSelector((state) => state?.guiStates);

  const markers = useSelector((state) => state?.markersStates?.markers);
  const isChanging = useSelector((state) => state?.markersStates?.isChanging);

  const [obj2Edit, setObj2Edit] = React.useState<Marker>(null);
  
  useEffect(() => {
    let map_center = guiStates.map_option?.map_center;
    map_center = map_center ? map_center : [51.5359, -0.09];
    //parentMap.setView(map_center);
  }, [guiStates.map_option?.map_center]);

  useEffect(() => {
    if (selected_id != null && selectedTool != EditStore.NothingTool) {

      let circle = markers?.circles.find(f => f.id == selected_id);

      if (circle != null) {
        dispatch(EditStore.actionCreators.setFigure(CircleTool));
        setObj2Edit(circle);
        return;
      }

      let polygon = markers?.polygons.find(f => f.id == selected_id);

      if (polygon != null) {
        dispatch(EditStore.actionCreators.setFigure(PolygonTool));
        setObj2Edit(polygon);
        return;
      }

      let polyline = markers?.polylines.find(f => f.id == selected_id);

      if (polyline != null) {
        dispatch(EditStore.actionCreators.setFigure(PolylineTool));
        setObj2Edit(polyline);
        return;
      }
    }

    if (selectedTool == EditStore.NothingTool && selected_id != null) {
      setObj2Edit(null);
    }
  }, [selected_id, selectedTool]);


   const mapEvents = useMapEvents({
      click(e) {
         var ll: L.LatLng = e.latlng as L.LatLng;
      },

       moveend(e: LeafletEvent) {
         var bounds: L.LatLngBounds;
         bounds = e.target.getBounds();
         var boundBox: BoundBox = {
           wn: [bounds.getWest(), bounds.getNorth()],
           es: [bounds.getEast(), bounds.getSouth()],
           zoom: e.target.getZoom()
         };

         dispatch(MarkersStore.actionCreators.requestMarkers(boundBox));

         console.log('Locat  ionMarkers Chaged:', e.target.getBounds(), "->", e.target.getZoom());
       },
        mousemove(e: L.LeafletMouseEvent) {

     }
   });

  
  const polygonChanged = useCallback(
    (polygon: IPolygon, e) => {
      var figures: IFigures = {

      };
      setObj2Edit(null);
      figures.polygons = [polygon];
      dispatch(MarkersStore.actionCreators.sendMarker(figures));

    }, [])

  const polylineChanged = useCallback(
    (figure: IPolyline, e) => {
      var figures: IFigures = {

      };
      setObj2Edit(null);
      figures.polylines = [figure];
      dispatch(MarkersStore.actionCreators.sendMarker(figures));

    }, [])

  const circleChanged = useCallback(
    (figure: ICircle, e) => {
      var figures: IFigures = {

      };
      setObj2Edit(null);
      figures.circles = [figure];
      dispatch(MarkersStore.actionCreators.sendMarker(figures));

    }, [])

  const selectMe = useCallback(
    (marker, e) => {
      parentMap.closePopup();
      var selected_id = marker.id;
      dispatch(GuiStore.actionCreators.selectTreeItem(selected_id));
    }, [])

  
  const updateBaseMarker = useCallback(
    (marker, e) => {
      dispatch(MarkersStore.actionCreators.updateBaseInfo(marker));
    }, [])

  
  useEffect(
    () => {
      dispatch(GuiStore.actionCreators.requestTreeUpdate());
    }, [isChanging]);

  const colorOptionsUnselected = { color: "green" };
  const colorOptionsSelected = { color: "yellow" };
  const colorOptionsChecked = { color: "blue" };
  

  const getColor = (id: string) => {
    let colorOption = colorOptionsUnselected;

    if (checked_ids.indexOf(id) !== -1) {
      colorOption = colorOptionsChecked;
    }

    if (selected_id == id) {
      colorOption = colorOptionsSelected;
    }

    return colorOption;
  }

  return (
    <React.Fragment>
      {
        markers?.circles?.map((marker, index) =>
          <MyCircle
            key={index}
            center={marker.geometry}
            pathOptions={getColor(marker.id)}
            radius={50}
            hidden={marker.id == obj2Edit?.id}

            marker={marker}
            updateBaseMarker={updateBaseMarker}
          >
            <ObjectPopup marker={marker} updateBaseMarker={updateBaseMarker}>
            </ObjectPopup>
          </MyCircle>
        )}
      {
        markers?.polygons?.map((marker, index) =>
          <MyPolygon
            pathOptions={getColor(marker.id)}
            positions={marker.geometry}
            key={index}
            hidden={marker.id == obj2Edit?.id}

            marker={marker}
            selectMe={selectMe}
            updateBaseMarker={updateBaseMarker}>
          </MyPolygon>
        )}

      {
        markers?.polylines?.map((marker, index) =>
          <MyPolyline
            pathOptions={getColor(marker.id)}
            positions={marker.geometry}
            key={index}
            hidden={marker.id == obj2Edit?.id}

            marker={marker}
            updateBaseMarker={updateBaseMarker}>
          </MyPolyline>
        )}

      {selectedTool == PolygonTool ?
        <PolygonMaker polygonChanged={polygonChanged} obj2Edit={obj2Edit}/> : <div />}
      {selectedTool == PolylineTool ?
        <PolylineMaker figureChanged={polylineChanged} obj2Edit={obj2Edit}/> : <div />}
      {selectedTool == CircleTool ?
        <CircleMaker figureChanged={circleChanged} obj2Edit={obj2Edit} /> : <div />}

    </React.Fragment>
  );
}
