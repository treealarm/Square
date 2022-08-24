import * as React from 'react';
import * as L from 'leaflet';
import { useDispatch, useSelector } from "react-redux";
import * as TracksStore from '../store/TracksStates';
import { ApplicationState } from '../store';
import { BoundBox, IGeoObjectDTO, LineStringType, PointType } from '../store/Marker';


import { useCallback, useEffect } from 'react'
import {
  useMap,
  Polyline,
  useMapEvents,
  Circle
} from 'react-leaflet'

declare module 'react-redux' {
  interface DefaultRootState extends ApplicationState { }
}


function TrackPolyline(props: any) {

  var pathOptions = { color: "red" };
  let figure = props.figure as IGeoObjectDTO;
  var positions = figure.location.coord;
  return (
    <React.Fragment>
      <Polyline
        pathOptions={pathOptions}
        positions={positions}
      >
      </Polyline>
    </React.Fragment>
  );
}

function TrackCircle(props: any) {

  var pathOptions = { color: "red" };
  let figure = props.figure as IGeoObjectDTO;
  var positions = figure.location.coord as any;

  return (
    <React.Fragment>
      <Circle
        pathOptions={pathOptions}
        center={positions}
        radius={figure.radius}
      >
      </Circle>
    </React.Fragment>
  );
}

function CommonTrack(props: any) {
  if (props.hidden == true || props.marker == null) {
    return null;
  }

  let figure = props.marker as IGeoObjectDTO; 

  if (figure.location.figure_type == LineStringType) {
    return (
      <React.Fragment>
        <TrackPolyline
          figure={figure}
        >
        </TrackPolyline>
      </React.Fragment>
    );
  }
  if (figure.location.figure_type == PointType) {
    return (
      <React.Fragment>
        <TrackCircle
          figure={figure}
        >
        </TrackCircle>
      </React.Fragment>
    );
  }

  return null;
}

export function TrackViewer() {

  const dispatch = useDispatch();
  const parentMap = useMap();

  useEffect(() => {
    console.log('ComponentDidMount TrackViewer');
    var bounds: L.LatLngBounds;
    bounds = parentMap.getBounds();
    var boundBox: BoundBox = {
      wn: [bounds.getWest(), bounds.getNorth()],
      es: [bounds.getEast(), bounds.getSouth()],
      zoom: parentMap.getZoom()
    };
    dispatch(TracksStore.actionCreators.requestRouts(boundBox));
    dispatch(TracksStore.actionCreators.requestTracks(boundBox));
  }, []);

  const mapEvents = useMapEvents({
    moveend(e: L.LeafletEvent) {
      var bounds: L.LatLngBounds;
      bounds = e.target.getBounds();
      var boundBox: BoundBox = {
        wn: [bounds.getWest(), bounds.getNorth()],
        es: [bounds.getEast(), bounds.getSouth()],
        zoom: e.target.getZoom()
      };

      dispatch(TracksStore.actionCreators.requestRouts(boundBox));
      dispatch(TracksStore.actionCreators.requestTracks(boundBox));
    }
  });

  const selected_id = useSelector((state) => state?.guiStates?.selected_id);
  const routs = useSelector((state) => state?.tracksStates?.routs);
  const tracks = useSelector((state) => state?.tracksStates?.tracks);

  return (
    <React.Fragment>
      {
        routs?.map((rout, index) =>
          <CommonTrack
            key={rout.id}
            hidden={false}
            marker={rout.figure}
          >
          </CommonTrack>
        )}
      {
        tracks?.map((track, index) =>
          <CommonTrack
            key={track.id}
            hidden={false}
            marker={track.figure}
          >
          </CommonTrack>
        )}
    </React.Fragment>
  );
}
