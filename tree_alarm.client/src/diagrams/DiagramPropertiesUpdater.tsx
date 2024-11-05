/* eslint-disable react-hooks/exhaustive-deps */
import { useSelector } from "react-redux";
import { ApplicationState } from "../store";
import { useAppDispatch } from "../store/configureStore";
import { useEffect } from "react";

import * as DiagramsStore from '../store/DiagramsStates';

function DiagramPropertiesUpdater() {

  const appDispatch = useAppDispatch();
  const selected_id = useSelector((state: ApplicationState) => state?.guiStates?.selected_id);
  const diagrams_updated = useSelector((state: ApplicationState) => state?.diagramsStates?.diagrams_updated ?? false);

  useEffect(() => {
    if (selected_id) {
      appDispatch(DiagramsStore.fetchSingleDiagram(selected_id));
    }      
    else {
      appDispatch(DiagramsStore.update_single_diagram_locally(null));
    }
  }, [selected_id, diagrams_updated]);

  return (
   null
  );
}

export default DiagramPropertiesUpdater;