/* eslint-disable react-hooks/exhaustive-deps */
import { Circle, Marker, Polygon, Polyline } from "react-leaflet";
import { ICircle, ICommonFig, IGeometryDTO, IPolygon, IPolyline, LineStringType, PointType, PolygonType, getExtraProp } from "../store/Marker";
import React, { useMemo } from "react";
import * as L from 'leaflet';
import { useAppDispatch } from "../store/configureStore";
import * as GuiStore from '../store/GUIStates';

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

  var imageUrl = getExtraProp(fig, "__image");

  if (imageUrl != null && imageUrl != "") {
    var image_width = getExtraProp(fig, "__image_width", "20");
    var image_height = getExtraProp(fig, "__image_height", "20");
    var image_rotate = getExtraProp(fig, "__image_rotate", "0");
    imageIcon = new L.DivIcon({
      iconSize: [0, 0],
      iconAnchor: [Number(image_width) / 2, Number(image_height) / 2],
      className: 'leaflet-div-icon',
      html: "<img style='transform: rotate(" + image_rotate + "deg); height:" + image_height + "px; width:" + image_width + "px;' src='" + imageUrl + "'>"
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


export function MyCommonFig(props: any) {

  var fig: ICommonFig = props.marker;
  var geo: IGeometryDTO = fig?.geometry;

  const appDispatch = useAppDispatch();

  const eventHandlers = useMemo(
    () => ({
      click() {
        appDispatch(GuiStore.selectTreeItem(props.marker.id));
      }
    }),
    [props.marker.id],
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