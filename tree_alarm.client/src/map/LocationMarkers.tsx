/* eslint-disable react-hooks/exhaustive-deps */
import * as React from 'react';
import * as L from 'leaflet';
import { useSelector } from "react-redux";
import { useAppDispatch } from '../store/configureStore';
import * as MarkersStore from '../store/MarkersStates';
import * as GuiStore from '../store/GUIStates';
import * as MarkersVisualStore from '../store/MarkersVisualStates';
import { ApplicationState } from '../store';
import { BoundBox, getExtraProp, ICommonFig, IObjProps } from '../store/Marker';


import { useCallback, useEffect, useMemo, useState } from 'react'
import {
  useMap,
  useMapEvents,
} from 'react-leaflet'


import { LeafletEvent } from 'leaflet';
import { ApiDefaultMaxCountResult } from '../store/constants';
import { MyCommonFig } from './MyCommonFig';



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

  const [selectedMarker, setSelectedMarker] = useState<ICommonFig | null>(null)

  useEffect(() => {
    parentMap.attributionControl.options.prefix =
      '<a href="https://www.leftfront.org" title="A JavaScript library for interactive maps">' + '<img width="12" height="8" src="https://upload.wikimedia.org/wikipedia/commons/a/a9/Flag_of_the_Soviet_Union.svg"></img>' + 'LeafletAlarms</a>';
  }, [parentMap]);

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
    const marker = markers?.figs.find((marker) => marker.id === selected_id);
    if (marker) {
      setSelectedMarker(marker);
    }
  }, [selected_id, markers?.figs]);

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
    }, [markers]);

   useMapEvents({
     preclick() {
       if (!selectedEditMode?.edit_mode) {
         appDispatch(GuiStore.selectTreeItem(null));
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
      appDispatch(GuiStore.requestTreeUpdate());
    }, [isChanging]);
  
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

  // Реверс-индексы по id — иначе getColor делал бы по 3 линейных
  // поиска на каждый маркер при каждом рендере (O(N*M)).
  const stateById = useMemo(
    () => new Map(visualStates.states.map(i => [i.id, i])),
    [visualStates.states]);

  const stateDescrByState = useMemo(
    () => new Map(visualStates.states_descr.map(s => [s.state, s])),
    [visualStates.states_descr]);

  const alarmById = useMemo(
    () => new Map(alarmedObjects.map(i => [i.id, i])),
    [alarmedObjects]);

  const checkedSet = useMemo(() => new Set(checked_ids), [checked_ids]);

  const getColor = useCallback(
    (marker: IObjProps) => {
      var id = marker.id ?? '';

      var retColor: L.PathOptions = {};
      retColor.fillColor = 'green';
      retColor.dashArray = '';
      retColor.color = 'green';

      if (checkedSet.has(id)) {
        retColor.dashArray = '5,10';
      }

      if (selected_id == id) {
        retColor.dashArray = '5,10';
      }

      {
        var vState = stateById.get(id);

        if (vState != null && vState.states.length > 0) {
          var vStateFirst = vState.states[0];
          var vStateDescr = stateDescrByState.get(vStateFirst);
          if (vStateDescr != null) {
            retColor.fillColor = vStateDescr.state_color
            retColor.color = vStateDescr.state_color
          }
        }
      }

      var vAlarmState = alarmById.get(id);

      if (vAlarmState != null
        && (vAlarmState.alarm || vAlarmState.children_alarms > 0)) {
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

    }, [stateById, stateDescrByState, alarmById, selected_id, checkedSet]);


  var hidden_id: string | null = null;

  if (selectedEditMode?.edit_mode) {
    hidden_id = objProps?.id??null;
  }


  return (
    <React.Fragment>
      {
        searchFilter?.show_objects != false &&
        markers?.figs.map((marker) => {
          // Пропускаем выделенный маркер при рендеринге
          if (marker.id === selected_id) return null;

          return (
            <MyCommonFig
              key={marker.id}
              marker={marker}
              hidden={marker.id == hidden_id}
              pathOptions={getColor(marker)}
            />
          );
        })
      }

      {/* Отдельно рендерим выбранный маркер */}
      {selectedMarker && (
        <MyCommonFig
          key={selectedMarker.id}
          marker={selectedMarker}
          hidden={selectedMarker.id == hidden_id}
          pathOptions={getColor(selectedMarker)}
        />
      )}
    </React.Fragment>
  );
}
