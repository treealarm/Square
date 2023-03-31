import * as React from 'react';
import * as L from 'leaflet';
import { useDispatch, useSelector } from "react-redux";
import * as TracksStore from '../store/TracksStates';
import { ApplicationState } from '../store';
import {
  BoxTrackDTO,
  IGeoObjectDTO,
  LineStringType,
  PointType,
  PolygonType
} from '../store/Marker';

import { useEffect } from 'react'
import {
  useMap,
  Polyline,
  useMapEvents,
  Circle,
  Polygon
} from 'react-leaflet'
import { LeafletMouseEvent } from 'leaflet';
import { useAppDispatch } from '../store/configureStore';

declare module 'react-redux' {
  interface DefaultRootState extends ApplicationState { }
}

const pathOptionsTracks = {
  fillColor: 'white',
  fillOpacity: 0.2,
  color: 'purple',
  opacity: 1
}

var pathOptionsRouts = { color: "blue" };

function TrackPolygon(props: any) {
  let figure = props.figure as IGeoObjectDTO;
  var positions = figure.location.coord;
  return (
    <React.Fragment>
      <Polygon
        pathOptions={props.pathOptions}
        positions={positions}
      >
      </Polygon>
    </React.Fragment>
  );
}

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

  const appDispatch = useAppDispatch();

  let figure = props.figure as IGeoObjectDTO;
  var positions = figure.location.coord as any;
  let track_id = props.track_id;

  const eventHandlers = React.useMemo(
    () => ({
      click(event: LeafletMouseEvent) {
        appDispatch<any>(TracksStore.actionCreators.GetRoutsByTracksIds([track_id]));
      }
    }),
    [track_id],
  )

  return (
    <React.Fragment>
      <Circle
        pathOptions={props.pathOptions}
        center={positions}
        radius={figure.radius}
        eventHandlers={eventHandlers}
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

  if (figure.location.type == PolygonType) {
    return (
      <React.Fragment>
        <TrackPolygon
          figure={figure}
          pathOptions={props.pathOptions}
        >
        </TrackPolygon>
      </React.Fragment>
    );
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
          track_id={props.track_id}
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

  const searchFilter = useSelector((state: ApplicationState) => state?.guiStates?.searchFilter);

  const selected_id = useSelector((state: ApplicationState) => state?.guiStates?.selected_id);
  const checked_ids = useSelector((state: ApplicationState) => state?.guiStates?.checked);
  const routs = useSelector((state: ApplicationState) => state?.tracksStates?.routs);
  const tracks = useSelector((state: ApplicationState) => state?.tracksStates?.tracks);

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

    if (searchFilter?.applied != true) {
      boundBox.time_start = null;
      boundBox.time_end = null;
      boundBox.property_filter = null;
    }

    if (selected_id != null || checked_ids != null) {
      boundBox.ids = [];
      if (checked_ids != null) {
        boundBox.ids = [... checked_ids];
      }
      if (selected_id != null) {
        boundBox.ids.push(selected_id);
      }
    }
    // We request routs by selection for now.
    //dispatch<any>(TracksStore.actionCreators.requestRouts(boundBox));
    dispatch<any>(TracksStore.actionCreators.requestTracks(boundBox));
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
        searchFilter?.show_tracks != false &&
        tracks?.map((track, index) =>          
          <CommonTrack
            key={track?.id }
            track_id={track?.id}
            hidden={false}
            marker={track?.figure}
            pathOptions={pathOptionsTracks}
          >
          </CommonTrack>
        )}
      {
        searchFilter?.show_routs != false &&
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
