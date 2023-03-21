import * as React from 'react';
import * as L from 'leaflet';
import { useDispatch, useSelector} from "react-redux";
import * as MarkersStore from '../store/MarkersStates';
import * as GuiStore from '../store/GUIStates';
import * as MarkersVisualStore from '../store/MarkersVisualStates';
import { ApiDefaultMaxCountResult, ApplicationState } from '../store';
import { BoundBox, getExtraProp, ICircle, ICommonFig, IGeometryDTO, IObjProps, IPolygon, IPolyline, LineStringType, PointType, PolygonType } from '../store/Marker';


import { useCallback, useMemo, useEffect } from 'react'
import {
  useMap,
  useMapEvents,
  Circle,
  Polygon,
  Polyline,
  Marker
} from 'react-leaflet'


import { LeafletEvent, LeafletMouseEvent } from 'leaflet';

declare module 'react-redux' {
  interface DefaultRootState extends ApplicationState { }
}

function MyPolygon(props: any) {

  var fig: IPolygon = props.marker;

  const dispatch = useDispatch();

  const eventHandlers = useMemo(
    () => ({
      click(event: LeafletMouseEvent) {
        var selected_id = props.marker.id;
        dispatch<any>(GuiStore.actionCreators.selectTreeItem(selected_id));
      }
    }),
    [],
  )

  return (
    <React.Fragment>
      <Polygon
        pathOptions={props.pathOptions}
        positions={fig.geometry.coord}
        eventHandlers={eventHandlers}
      >
      </Polygon>
    </React.Fragment>
  );
}

function MyPolyline(props: any) {

  var fig: IPolyline = props.marker;

  const dispatch = useDispatch();
  const eventHandlers = useMemo(
    () => ({
      click(event: LeafletMouseEvent) {
        var selected_id = props.marker.id;
        dispatch<any>(GuiStore.actionCreators.selectTreeItem(selected_id));        
      }
    }),
    [],
  )

  return (
    <React.Fragment>
      <Polyline
        pathOptions={props.pathOptions}
        positions={fig.geometry.coord}
        eventHandlers={eventHandlers}
      >
      </Polyline>
    </React.Fragment>
  );
}

function MyCircle(props: any) {

  var fig: ICircle = props.marker;
  var center = fig.geometry.coord;  
  var radius = fig.radius > 0 ? fig.radius : 10;

  const dispatch = useDispatch();
  const eventHandlers = useMemo(
    () => ({
      click(event: LeafletMouseEvent) {
        dispatch<any>(GuiStore.actionCreators.selectTreeItem(props.marker.id));
      }
    }),
    [],
  )

  var obj_text = getExtraProp(fig, "text");

  var textIcon: L.DivIcon = null;

  if (obj_text != null && obj_text != "")
  {
    textIcon = L.divIcon({ html: "<h2>"+obj_text+"</h2>", iconSize: [0, 0] });
  }

  if (textIcon != null) {
    return (
      <Marker
        position={center}
        icon={textIcon}
        eventHandlers={eventHandlers}/>
    );
  }

  return (
    <React.Fragment>
      <Circle
        pathOptions={props.pathOptions}
        center={center}
        radius={radius}
        eventHandlers={eventHandlers}
      >

      </Circle>
    </React.Fragment>
  );
}

function MyCommonFig(props: any) {
  
  if (props.hidden == true) {
    return null;
  }

  var fig: ICommonFig = props.marker;
  var geo: IGeometryDTO = fig.geometry;
    
  
  if (geo.type == PointType) {
    const center = geo.coord as [number, number];
    return (
      <MyCircle {...props}>
        
      </MyCircle>
    );
  }

  if (geo.type == PolygonType) {
    const center = L.polygon(geo.coord).getBounds().getCenter();
    return (
      <MyPolygon {...props}>
      </MyPolygon>
    );
  }

  if (geo.type == LineStringType) {
    const center = L.polyline(geo.coord).getBounds().getCenter();
    return (
      <MyPolyline {...props}>
      </MyPolyline>
    );
  }
  return null;
}

export function LocationMarkers() {

  const dispatch = useDispatch();
  const parentMap = useMap();

  const selected_id = useSelector((state: ApplicationState) => state?.guiStates?.selected_id);
  const checked_ids = useSelector((state: ApplicationState) => state?.guiStates?.checked);
  const searchFilter = useSelector((state: ApplicationState) => state?.guiStates?.searchFilter);

  const selectedEditMode = useSelector((state: ApplicationState) => state.editState);

  const markers = useSelector((state: ApplicationState) => state?.markersStates?.markers);
  const markersStates = useSelector((state: ApplicationState) => state?.markersStates);
  const isChanging = useSelector((state: ApplicationState) => state?.markersStates?.isChanging);
  const visualStates = useSelector((state: ApplicationState) => state?.markersVisualStates?.visualStates);
  const alarmedObjects = useSelector((state: ApplicationState) => state?.markersVisualStates?.alarmed_objects);
  const objProps = useSelector((state: ApplicationState) => state?.objPropsStates?.objProps);
  const user = useSelector((state: ApplicationState) => state?.rightsStates?.user);

  function RequestMarkersByBox(bounds: L.LatLngBounds) {
    if (bounds == null) {
      bounds = parentMap.getBounds();
    }
    
    var boundBox: BoundBox = {
      wn: [bounds.getWest(), bounds.getNorth()],
      es: [bounds.getEast(), bounds.getSouth()],
      zoom: parentMap.getZoom(),
      property_filter: searchFilter?.property_filter
    };

    if (searchFilter?.applied != true) {
      boundBox.property_filter = null;
    }

    dispatch<any>(MarkersStore.actionCreators.requestMarkers(boundBox));
  }

  useEffect(() => {
    RequestMarkersByBox(null);
  }, [user]);



  useEffect(
    () => {
      if (markers == null) {
        return;
      }
      var objArray2: string[] = [];
      markers.figs?.forEach(arr => objArray2.push(arr.id));
      dispatch<any>(MarkersVisualStore.actionCreators.requestMarkersVisualStates(objArray2));
    }, [markers]);

   const mapEvents = useMapEvents({
      click(e: any) {
       var ll: L.LatLng = e.latlng as L.LatLng;
       //dispatch(GuiStore.actionCreators.selectTreeItem(null));
      },

       moveend(e: LeafletEvent) {
         var bounds: L.LatLngBounds;
         bounds = e.target.getBounds();

         RequestMarkersByBox(bounds);
         console.log('LocationMarkers Chaged:', e.target.getBounds(), "->", e.target.getZoom());
       },
        mousemove(e: L.LeafletMouseEvent) {

     }
   });

  
  const selectMe = useCallback(
    (marker: any, e: any) => {
      parentMap.closePopup();
      var selected_id = marker.id;
      dispatch<any>(GuiStore.actionCreators.selectTreeItem(selected_id));
    }, [])

  useEffect(
    () => {
      dispatch<any>(GuiStore.actionCreators.requestTreeUpdate());
    }, [isChanging]);
  
  useEffect(
    () => {
      if (markers?.figs?.length > ApiDefaultMaxCountResult*2) {
        // Clear TODO time limit
        RequestMarkersByBox(null);
      }      
    }, [markers]);

  useEffect(
    () => {

      if (searchFilter?.search_id != null &&
        searchFilter?.search_id != "") {
        RequestMarkersByBox(null);
      }      
    }, [markersStates?.initiateUpdateAll, searchFilter?.search_id]);

  const getColor = useCallback(
    (marker: IObjProps) => {
      var id = marker.id;

      var retColor: L.PathOptions    = {    };
      retColor.fillColor = 'green';
      retColor.dashArray = '';
      retColor.color = 'green';
      
      if (checked_ids.indexOf(id) !== -1) {
        retColor.dashArray = '5,10';
      }

      if (selected_id == id) {
        retColor.dashArray = '5,10';
      }

      {
        var vState = visualStates.states.find(i => i.id == id);

        if (vState != null && vState.states.length > 0) {
          var vStateFirst = vState.states[0];
          var vStateDescr = visualStates.states_descr.find(s => s.state == vStateFirst);
          if (vStateDescr != null) {
            retColor.fillColor = vStateDescr.state_color
            retColor.color = vStateDescr.state_color
          }
        }
      }

      var vAlarmState = alarmedObjects.find(i => i.id == id);

      if (vAlarmState != null
        && (vAlarmState.alarm || vAlarmState.children_alarms > 0)) {
        //const colorOptions = {
        //  fillColor: 'yellow',
        //  fillOpacity: 0.5,
        //  color: 'yellow',
        //  opacity: 1,
        //  dashArray: '5,10'
        //}
        retColor.fillColor = 'red';
        retColor.color = 'red';
      }
      else {
        var color = getExtraProp(marker, "color");

        if (color != null) {
          retColor.fillColor = color;
          retColor.color = color;
        }
      }

      return retColor;

    }, [visualStates, alarmedObjects, selected_id, checked_ids])

  var hidden_id: string = null;

  if (selectedEditMode.edit_mode) {
    hidden_id = objProps?.id;
  }


  return (
    <React.Fragment>
      {
        searchFilter?.show_objects != false &&
        markers?.figs?.map((marker, index) =>
          <MyCommonFig
            key={marker.id} 
            marker={marker}
            hidden={marker.id == hidden_id}
            pathOptions={getColor(marker)}
          >
          </MyCommonFig>
        )}
    
    </React.Fragment>
  );
}
