/* eslint-disable no-undef */
/* eslint-disable no-unused-vars */
/* eslint-disable react-hooks/exhaustive-deps */
import { Circle, Marker, Polygon, Polyline, Tooltip } from "react-leaflet";
import { ICircle, ICommonFig, IGeometryDTO, IPolygon, IPolyline, IValueDTO, LineStringType, PointType, PolygonType, calculateCenter, getExtraProp } from "../store/Marker";
import React, { useEffect, useMemo, useState } from "react";
import * as L from 'leaflet';
import { useAppDispatch } from "../store/configureStore";
import * as GuiStore from '../store/GUIStates';
import * as ValuesStore from '../store/ValuesStates';
import { useSelector } from "react-redux";
import { ApplicationState } from "../store";
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

  const update_values_periodically = useSelector((state: ApplicationState) => state?.valuesStates?.update_values_periodically);
  const cur_values: IValueDTO[] = useSelector(ValuesStore.selectValuesMapForOwner(fig.id ?? '')) ?? [];
  const selected_id = useSelector((state: ApplicationState) => state?.guiStates?.selected_id);

  const appDispatch = useAppDispatch();

  const [tooltipOpacity, setTooltipOpacity] = useState(0.7);

  useEffect(() => {
    // Обновляем состояние opacity, когда selected_id изменяется
    if (!selected_id || selected_id=='') {
      setTooltipOpacity(0.7);
    }
    else {
      setTooltipOpacity(selected_id == fig?.id ? 1.0 : 0.5);
    }
    
  }, [selected_id, fig?.id]);

  const eventHandlers = useMemo(
    () => ({
      click() {
        appDispatch(GuiStore.selectTreeItem(props.marker?.id));
      }
    }),
    [props.marker?.id],
  );

  if (fig == null) {
    return null;
  }
  
  if (props.hidden == true) {
    return null;
  }

  if (geo?.type == null) {
    return null;
  }

  const center = calculateCenter(geo);

  return (<React.Fragment>
    {geo.type === PointType && <MyCircle {...props} eventHandlers={eventHandlers} />}
    {geo.type === PolygonType && <MyPolygon {...props} eventHandlers={eventHandlers} />}
    {geo.type === LineStringType && <MyPolyline {...props} eventHandlers={eventHandlers} />}

    {/* Рендеринг значений как маркеров */}
    {(update_values_periodically && cur_values.length > 0) && (
      <Marker
        position={center} // Позиция маркера
        opacity={0} // Скрыть маркер
        key={fig?.id ?? 'unknown_id'}
        eventHandlers={eventHandlers}
      >
        <Tooltip
          direction="right"
          offset={[0, 0]}
          opacity= {0.7}
          permanent>
          <div style={{
            border: selected_id === fig?.id ? '1px solid black' : '',
            textAlign: 'left', whiteSpace: 'normal', maxWidth: '300px', overflowY: 'auto'}}>
            <ul style={{ margin: 0, padding: 0, listStyle: 'none' }}>
              {cur_values.map(value => (
                <li key={value.id} style={{ margin: 0, padding: '0px 0', whiteSpace: 'nowrap' }}>
                  {`${value.name}: ${value.value}`}
                </li>
              ))}
            </ul>
          </div>
        </Tooltip>



      </Marker>
    )}




  </React.Fragment>
);
}
