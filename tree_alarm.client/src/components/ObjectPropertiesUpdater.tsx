/* eslint-disable react-hooks/exhaustive-deps */
import * as React from 'react';


import { useEffect } from 'react';
import { useSelector } from "react-redux";

import { ApplicationState } from '../store';
import * as ObjPropsStore from '../store/ObjPropsStates';
import * as EditStore from '../store/EditStates';
import * as DiagramsStore from '../store/DiagramsStates';

import { useAppDispatch } from '../store/configureStore';


export function ObjectPropertiesUpdater(): React.ReactElement|null {

  const appDispatch = useAppDispatch();

  const selected_id = useSelector((state: ApplicationState) => state?.guiStates?.selected_id);
  const markers = useSelector((state: ApplicationState) => state?.markersStates?.markers);
  const diagrams_updated = useSelector((state: ApplicationState) => state?.diagramsStates?.diagrams_updated ?? false);

  

  useEffect(() => {
    if (selected_id == null) {
      appDispatch<any>(EditStore.setEditMode(false));
      appDispatch(ObjPropsStore.setObjPropsLocally(null));
    }
    else if (selected_id?.startsWith('00000000', 0)) {
      var selectedMarker = markers?.figs?.find(m => m.id == selected_id);

      if (selectedMarker != null) {
        appDispatch(ObjPropsStore.setObjPropsLocally(selectedMarker));
        return;
      }
    }
    if (selected_id) {
      appDispatch(ObjPropsStore.fetchObjProps(selected_id));
      appDispatch(DiagramsStore.fetchSingleDiagram(selected_id));
    }
    else {
      appDispatch(DiagramsStore.update_single_diagram_locally(null));
   
    }
  }, [selected_id, diagrams_updated]);

  
  return (null);
}