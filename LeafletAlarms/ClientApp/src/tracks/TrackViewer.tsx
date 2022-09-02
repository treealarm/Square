import * as React from 'react';
import * as L from 'leaflet';
import { useDispatch, useSelector } from "react-redux";
import * as TracksStore from '../store/TracksStates';
import { ApplicationState } from '../store';
import { BoxTrackDTO, IGeoObjectDTO, LineStringType, PointType } from '../store/Marker';

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

  const searchFilter = useSelector((state) => state?.tracksStates?.searchFilter);

  function UpdateTracks() {
    var bounds: L.LatLngBounds;
    bounds = parentMap.getBounds();
    var boundBox: BoxTrackDTO = {
      wn: [bounds.getWest(), bounds.getNorth()],
      es: [bounds.getEast(), bounds.getSouth()],
      zoom: parentMap.getZoom(),
      time_start: searchFilter?.time_start,
      time_end: searchFilter?.time_end
    };
    dispatch(TracksStore.actionCreators.requestRouts(boundBox));
    dispatch(TracksStore.actionCreators.requestTracks(boundBox));
  }

  useEffect(() => {
    console.log('ComponentDidMount TrackViewer');
    UpdateTracks();
  }, []);

  useEffect(() => {
    console.log('Search Filter Updated TrackViewer');
    UpdateTracks();
  }, [searchFilter]);

  const mapEvents = useMapEvents({
    moveend(e: L.LeafletEvent) {
      UpdateTracks();
    },
    async click(e: L.LeafletMouseEvent) {
      var ll: L.LatLng = e.latlng as L.LatLng;
      if (e.originalEvent.ctrlKey) {
        await navigator.clipboard.writeText(JSON.stringify(ll));
      }      
    }
  });

  const routs = useSelector((state) => state?.tracksStates?.routs);
  const tracks = useSelector((state) => state?.tracksStates?.tracks);

  return (
    <React.Fragment>
      {
        tracks?.map((track, index) =>
          <CommonTrack
            key={track.id}
            hidden={false}
            marker={track.figure}
          >
          </CommonTrack>
        )}
      {
        routs?.map((rout, index) =>
          <CommonTrack
            key={rout.id}
            hidden={false}
            marker={rout.figure}
          >
          </CommonTrack>
        )}

    </React.Fragment>
  );
}
