﻿import * as React from 'react';


import { useEffect } from 'react';
import { useSelector } from "react-redux";

import { ApplicationState } from '../store';
import * as ObjPropsStore from '../store/ObjPropsStates';

import * as EditStore from '../store/EditStates';
import { useAppDispatch } from '../store/configureStore';

export function ObjectPropertiesUpdater(): JSX.Element {

  const dispatch = useAppDispatch();

  const selected_id = useSelector((state: ApplicationState) => state?.guiStates?.selected_id);
  const markers = useSelector((state: ApplicationState) => state?.markersStates?.markers);

  useEffect(() => {
    if (selected_id == null) {
      dispatch<any>(EditStore.actionCreators.setFigureEditMode(false));
    }
    else if (selected_id?.startsWith('00000000', 0)) {
      var selectedMarker = markers.figs.find(m => m.id == selected_id);

      if (selectedMarker != null) {
        dispatch<any>(ObjPropsStore.actionCreators.setObjPropsLocally(selectedMarker));
        return;
      }
    }

    dispatch<any>(ObjPropsStore.actionCreators.getObjProps(selected_id));
  }, [selected_id]);

  
  return (null);
}