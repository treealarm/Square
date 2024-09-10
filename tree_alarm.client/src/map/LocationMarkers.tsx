import * as React from 'react';
import * as L from 'leaflet';
import { useSelector } from "react-redux";
import { useAppDispatch } from '../store/configureStore';
import * as MarkersStore from '../store/MarkersStates';
import * as GuiStore from '../store/GUIStates';
import * as MarkersVisualStore from '../store/MarkersVisualStates';
import { ApplicationState } from '../store';
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
import { ApiDefaultMaxCountResult } from '../store/constants';


function MyPolygon(props: any) {

  var fig: IPolygon = props.marker;


  return (
    <React.Fragment>
      <Polygon
        pathOptions={props.pathOptions}
        positions={fig.geometry.coord}
        eventHandlers={props.eventHandlers}
      >
      </Polygon>
    </React.Fragment>
  );
}

function MyPolyline(props: any) {

  var fig: IPolyline = props.marker;

  return (
    <React.Fragment>
      <Polyline
        pathOptions={props.pathOptions}
        positions={fig.geometry.coord}
        eventHandlers={props.eventHandlers}
      >
      </Polyline>
    </React.Fragment>
  );
}

function MyCircle(props: any) {

  var fig: ICircle = props.marker;
  var center = fig.geometry.coord;  
  var radius = fig.radius > 0 ? fig.radius : 10;

  var imageIcon: L.DivIcon = null;
  var textIcon: L.DivIcon = null;

  var imageUrl =  getExtraProp(fig, "__image");

  if (imageUrl != null && imageUrl != "") {
    var image_width = getExtraProp(fig, "__image_width", "20");
    var image_height = getExtraProp(fig, "__image_height", "20");
    var image_rotate = getExtraProp(fig, "__image_rotate", "0");
    imageIcon = new L.DivIcon({
      iconSize: [0, 0],
      iconAnchor: [Number(image_width)/2, Number(image_height)/2],
      className: 'leaflet-div-icon',
      html: "<img style='transform: rotate(" + image_rotate + "deg); height:" + image_height +"px; width:" + image_width +"px;' src='" + imageUrl +"'>"
    });

    //imageIcon = new L.Icon({
    //  iconUrl: imageUrl,
    //  iconRetinaUrl: imageUrl,
    //  iconAnchor: null,
    //  popupAnchor: null,
    //  shadowUrl: null,
    //  shadowSize: null,
    //  shadowAnchor: null,
    //  iconSize: new L.Point(20, 20),
    //  className: 'leaflet-div-icon'
    //});
  }
  else {
    var obj_text = getExtraProp(fig, "__text");

    if (obj_text != null && obj_text != "") {
      textIcon = L.divIcon({ html: "<h2>" + obj_text + "</h2>", iconSize: [0, 0] });
    }
  }

  if (imageIcon != null) {
    return (
      <React.Fragment>
        {
          radius > 0 &&
            <Circle
              pathOptions={props.pathOptions}
              center={center}
              radius={radius}
            eventHandlers={props.eventHandlers}
            />
        }       

      <Marker
        position={center}
        icon={imageIcon}
          eventHandlers={props.eventHandlers}
        />
      </React.Fragment>
    );
  }

  if (textIcon != null) {
    return (
      <Marker
        position={center}
        icon={textIcon}
        eventHandlers={props.eventHandlers}
      />
    );
  }

  return (
      <Circle
        pathOptions={props.pathOptions}
        center={center}
        radius={radius}
        eventHandlers={props.eventHandlers}
      />
  );
}


function MyCommonFig(props: any) {

  var fig: ICommonFig = props.marker;
  var geo: IGeometryDTO = fig.geometry;

  const appDispatch = useAppDispatch();

  const eventHandlers = useMemo(
    () => ({
      click() {
        appDispatch<any>(GuiStore.actionCreators.selectTreeItem(props.marker.id));

        var __is_diagram = getExtraProp(fig, "__is_diagram", "0");

        if (__is_diagram == '1') {
          console.log('__is_diagram:', __is_diagram);
        }
      }
    }),
    [appDispatch, fig, props.marker.id],
  );

  if (props.hidden == true) {
    return null;
  }



  if (geo?.type == null) {
    return null;
  }
  
  if (geo.type == PointType) {
    return (
      <MyCircle {...props} eventHandlers={eventHandlers} >
        
      </MyCircle>
    );
  }

  if (geo.type == PolygonType) {
    return (
      <MyPolygon {...props} eventHandlers={eventHandlers}>
      </MyPolygon>
    );
  }

  if (geo.type == LineStringType) {
    return (
      <MyPolyline {...props} eventHandlers={eventHandlers}>
      </MyPolyline>
    );
  }
  return null;
}

export function LocationMarkers() {

  const appDispatch = useAppDispatch();
  const parentMap = useMap();

  const selected_id = useSelector((state: ApplicationState) => state?.guiStates?.selected_id);
  const checked_ids = useSelector((state: ApplicationState) => state?.guiStates?.checked);
  const searchFilter = useSelector((state: ApplicationState) => state?.guiStates?.searchFilter);

  const selectedEditMode = useSelector((state: ApplicationState) => state.editState);

  const markers = useSelector((state: ApplicationState) => state?.markersStates?.markers);
  const markersStates = useSelector((state: ApplicationState) => state?.markersStates);
  const isChanging = useSelector((state: ApplicationState) => state?.markersStates?.isChanging);
  const visualStates = useSelector((state: ApplicationState) => state?.markersVisualStates?.visualStates);
  const alarmedObjects = useSelector((state: ApplicationState) => state?.markersVisualStates?.visualStates?.alarmed_objects);
  const objProps = useSelector((state: ApplicationState) => state?.objPropsStates?.objProps);
  const user = useSelector((state: ApplicationState) => state?.rightsStates?.user);

  parentMap.attributionControl.options.prefix = 
    '<a href="https://www.leftfront.org" title="A JavaScript library for interactive maps">' + '<img width="12" height="8" src="https://upload.wikimedia.org/wikipedia/commons/a/a9/Flag_of_the_Soviet_Union.svg"></img>' + 'LeafletAlarms</a>';

  const RequestMarkersByBox = useCallback((bounds: L.LatLngBounds)=> {
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

    appDispatch(MarkersStore.fetchMarkersByBox(boundBox));
  },[appDispatch, parentMap, searchFilter?.applied, searchFilter?.property_filter])

  useEffect(() => {
    RequestMarkersByBox(null);
  }, [RequestMarkersByBox, user]);



  useEffect(
    () => {
      if (markers == null) {
        return;
      }
      var objArray2: string[] = [];
      markers.figs?.forEach(arr => objArray2.push(arr.id));
      appDispatch<any>(MarkersVisualStore.requestMarkersVisualStates(objArray2));
    }, [appDispatch, markers]);

   useMapEvents({
     preclick(e: LeafletMouseEvent) {
       console.log(e);
       if (!selectedEditMode?.edit_mode) {
         appDispatch<any>(GuiStore.actionCreators.selectTreeItem(null));
       }       
      },

       moveend(e: LeafletEvent) {
         var bounds: L.LatLngBounds;
         bounds = e.target.getBounds();

         RequestMarkersByBox(bounds);
         //console.log('LocationMarkers Chaged:', e.target.getBounds(), "->", e.target.getZoom());
       },
 
   });

  useEffect(
    () => {
      appDispatch<any>(GuiStore.actionCreators.requestTreeUpdate());
    }, [appDispatch, isChanging]);
  
  useEffect(
    () => {
      if (markers?.figs?.length > ApiDefaultMaxCountResult*2) {
        // Clear TODO time limit
        RequestMarkersByBox(null);
      }      
    }, [RequestMarkersByBox, markers]);

  useEffect(
    () => {

      if (searchFilter?.search_id != null &&
        searchFilter?.search_id != "") {
        RequestMarkersByBox(null);
      }      
    }, [RequestMarkersByBox, markersStates?.initiateUpdateAll, searchFilter?.search_id]);

  const getColor = useCallback(
    (marker: IObjProps) => {
      var id = marker.id;

      var retColor: L.PathOptions = {};
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
        && (vAlarmState.alarm)) {
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
        var color = getExtraProp(marker, "__color");

        if (color != null) {
          retColor.fillColor = color;
          retColor.color = color;
        }
      }

      return retColor;

    }, [visualStates, alarmedObjects, selected_id, checked_ids]);


  var hidden_id: string | null = null;

  if (selectedEditMode?.edit_mode) {
    hidden_id = objProps?.id??null;
  }


  return (
    <React.Fragment>
      {
        searchFilter?.show_objects != false &&
        markers?.figs?.map((marker) =>
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
