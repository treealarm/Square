import * as React from 'react';
import * as L from 'leaflet';
import { useDispatch, useSelector } from "react-redux";
import * as TracksStore from '../store/TracksStates';
import { ApplicationState } from '../store';
import { BoundBox } from '../store/Marker';


import { useCallback, useEffect } from 'react'
import {
  useMap,
  Polyline,
  useMapEvents
} from 'react-leaflet'

declare module 'react-redux' {
  interface DefaultRootState extends ApplicationState { }
}


function TrackPolyline(props: any) {
  if (props.hidden == true || props.positions == null) {
    return null;
  }

  return (
    <React.Fragment>
      <Polyline
        pathOptions={props.pathOptions}
        positions={props.positions}
      >
      </Polyline>
    </React.Fragment>
  );
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

      dispatch(TracksStore.actionCreators.requestTracks(boundBox));
    }
  });

  const selected_id = useSelector((state) => state?.guiStates?.selected_id);
  const tracks = useSelector((state) => state?.tracksStates?.tracks);



  const colorOptionsUnselected = { color: "red" };


  const getColor = useCallback(
    (id: string) => {
      return colorOptionsUnselected;
    }, [selected_id])


  return (
    <React.Fragment>
      {
        tracks?.map((track, index) =>
          <TrackPolyline
            pathOptions={getColor(track.figure.id)}
            positions={track.figure.location.coord}
            key={track.id}
            hidden={false}

            marker={track.figure}
          >
          </TrackPolyline>
        )}
    </React.Fragment>
  );
}
