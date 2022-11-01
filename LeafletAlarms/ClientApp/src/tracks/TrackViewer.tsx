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
import { Console } from 'console';


declare module 'react-redux' {
  interface DefaultRootState extends ApplicationState { }
}

var pathOptionsTracks = { color: "yellow" };
var pathOptionsRouts = { color: "blue" };

function TrackPolyline(props: any) {
  let figure = props.figure as IGeoObjectDTO;
  var positions = figure.location.coord;
  return (
    <React.Fragment>
      <Polyline
        pathOptions={props.pathOptions}
        positions={positions}
      >
      </Polyline>
    </React.Fragment>
  );
}

function TrackCircle(props: any) {  
  let figure = props.figure as IGeoObjectDTO;
  var positions = figure.location.coord as any;

  return (
    <React.Fragment>
      <Circle
        pathOptions={props.pathOptions}
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

  if (figure.location == null) {
    // Can be not built rout.
    return null;
  }

  if (figure.location.type == LineStringType) {
    return (
      <React.Fragment>
        <TrackPolyline
          figure={figure}
          pathOptions={ props.pathOptions }
        >
        </TrackPolyline>
      </React.Fragment>
    );
  }
  if (figure.location.type == PointType) {
    return (
      <React.Fragment>
        <TrackCircle
          figure={figure}
          pathOptions={props.pathOptions}
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

  const searchFilter = useSelector((state) => state?.guiStates?.searchFilter);
  const selected_id = useSelector((state) => state?.guiStates?.selected_id);
  const checked_ids = useSelector((state) => state?.guiStates?.checked);
  const routs = useSelector((state) => state?.tracksStates?.routs);
  const tracks = useSelector((state) => state?.tracksStates?.tracks);

  function UpdateTracks() {
    var bounds: L.LatLngBounds;
    bounds = parentMap.getBounds();
    var boundBox: BoxTrackDTO = {
      wn: [bounds.getWest(), bounds.getNorth()],
      es: [bounds.getEast(), bounds.getSouth()],
      zoom: parentMap.getZoom(),
      time_start: searchFilter?.time_start,
      time_end: searchFilter?.time_end,
      property_filter: searchFilter?.property_filter,
      sort: searchFilter?.sort
    };

    if (selected_id != null || checked_ids != null) {
      boundBox.ids = [];
      if (checked_ids != null) {
        boundBox.ids = [... checked_ids];
      }
      if (selected_id != null) {
        boundBox.ids.push(selected_id);
      }
    }
    dispatch(TracksStore.actionCreators.requestRouts(boundBox));
    dispatch(TracksStore.actionCreators.requestTracks(boundBox));
  }

  useEffect(() => {
    console.log('ComponentDidMount TrackViewer');
    //UpdateTracks();
  }, []);

  useEffect(() => {
    console.log('Search Filter Updated TrackViewer');
    if (searchFilter?.search_id != null &&
      searchFilter?.search_id != "") {
      UpdateTracks();
    }
    
  }, [searchFilter?.search_id, selected_id, checked_ids]);

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

  return (
    <React.Fragment>
      {
        tracks?.map((track, index) =>
          <CommonTrack
            key={track?.id}
            hidden={false}
            marker={track?.figure}
            pathOptions={pathOptionsTracks}
          >
          </CommonTrack>
        )}
      {
        routs?.map((rout, index) =>
          <CommonTrack
            key={rout?.id}
            hidden={false}
            marker={rout?.figure}
            pathOptions={pathOptionsRouts}
          >
          </CommonTrack>
        )}

    </React.Fragment>
  );
}
