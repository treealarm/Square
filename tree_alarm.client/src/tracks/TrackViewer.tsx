import * as React from 'react';
import * as L from 'leaflet';
import { useSelector } from "react-redux";
import * as TracksStore from '../store/TracksStates';
import { ApplicationState } from '../store';
import {
  BoxTrackDTO,
  IGeoObjectDTO,
  LineStringType,
  PointType,
  PolygonType
} from '../store/Marker';

import { useCallback, useEffect } from 'react'
import {
  useMap,
  Polyline,
  useMapEvents,
  Circle,
  Polygon
} from 'react-leaflet'

import { useAppDispatch } from '../store/configureStore';

const pathOptionsTracks = {
  fillColor: 'purple',
  fillOpacity: 0.2,
  color: 'purple',
  opacity: 0.5,
  dashArray: ''
}

const pathOptionsTracksSelected = {
  fillColor: 'purple',
  fillOpacity: 0.5,
  color: 'red',
  opacity: 0.8,
  dashArray: '5,10'
}

function TrackPolygon(props: any) {
  const appDispatch = useAppDispatch();

  let figure = props.figure as IGeoObjectDTO;
  var positions = figure.location.coord;
  let track = props.track;

  const eventHandlers = React.useMemo(
    () => ({
      click() {
        appDispatch<any>(TracksStore.OnSelectTrack(track));
      }
    }),
    [appDispatch, track],
  )

  return (
    <React.Fragment>
      <Polygon
        pathOptions={props.pathOptions}
        positions={positions}
        eventHandlers={eventHandlers}
      >
      </Polygon>
    </React.Fragment>
  );
}

function TrackPolyline(props: any) {
  const appDispatch = useAppDispatch();

  let figure = props.figure as IGeoObjectDTO;
  var positions = figure.location.coord;

  let track = props.track;

  const eventHandlers = React.useMemo(
    () => ({
      click() {
        appDispatch<any>(TracksStore.OnSelectTrack(track));
      }
    }),
    [appDispatch, track],
  )

  return (
    <React.Fragment>
      <Polyline
        pathOptions={props.pathOptions}
        positions={positions}
        eventHandlers={eventHandlers}
      >
      </Polyline>
    </React.Fragment>
  );
}

function TrackCircle(props: any) { 

  const appDispatch = useAppDispatch();

  let figure = props.figure as IGeoObjectDTO;
  var positions = figure.location.coord as any;
  let track = props.track;

  const eventHandlers = React.useMemo(
    () => ({
      click() {
        appDispatch<any>(TracksStore.OnSelectTrack(track));
      }
    }),
    [appDispatch, track],
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
    // Can be not built route.
    return null;
  }

  if (figure.location.type == PolygonType) {
    return (
      <React.Fragment>
        <TrackPolygon
          figure={figure}
          pathOptions={props.pathOptions}
          track={props.track}
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
          pathOptions={props.pathOptions}
          track={props.track}
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
          track={props.track}
        >
        </TrackCircle>
      </React.Fragment>
    );
  }

  return null;
}

export function TrackViewer() {

  const appDispatch = useAppDispatch();
  const parentMap = useMap();

  const searchFilter = useSelector((state: ApplicationState) => state?.guiStates?.searchFilter);

  const selected_id = useSelector((state: ApplicationState) => state?.guiStates?.selected_id);
  const checked_ids = useSelector((state: ApplicationState) => state?.guiStates?.checked);
  const tracks = useSelector((state: ApplicationState) => state?.tracksStates?.tracks);
  const selected_track = useSelector((state: ApplicationState) => state?.tracksStates?.selected_track);
  const user = useSelector((state: ApplicationState) => state?.rightsStates?.user);

  const UpdateTracks = useCallback(() =>{
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
      //if (checked_ids != null) {
      //  boundBox.ids = [... checked_ids];
      //}
      //if (selected_id != null) {
      //  boundBox.ids.push(selected_id);
      //}
    }
    // We request routes by selection for now.
    //dispatch<any>(TracksStore.actionCreators.requestRoutes(boundBox));
    appDispatch<any>(TracksStore.fetchTracksByBox(boundBox));
  },[appDispatch, checked_ids, parentMap, searchFilter?.applied, searchFilter?.property_filter, searchFilter?.sort, searchFilter?.time_end, searchFilter?.time_start, selected_id])


  useEffect(() => {
    console.log('Search Filter Updated TrackViewer');
    if (searchFilter?.search_id != null &&
      searchFilter?.search_id != "") {
      UpdateTracks();
    }
    
  }, [searchFilter?.search_id, selected_id, checked_ids, user, UpdateTracks]);

  useMapEvents({
    moveend() {
      UpdateTracks();
    },
    preclick() {
      appDispatch<any>(TracksStore.OnSelectTrack(null));
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
        tracks?.map((track) =>          
          <CommonTrack
            key={track?.id }
            track={track}
            hidden={false}
            marker={track?.figure}
            pathOptions={selected_track?.id == track?.id ? pathOptionsTracksSelected : pathOptionsTracks}
          >
          </CommonTrack>
        )}
    </React.Fragment>
  );
}
