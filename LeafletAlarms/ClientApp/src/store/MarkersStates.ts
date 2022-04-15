import { Action, Reducer } from "redux";
import { ApiRootString, AppThunkAction } from "./";
import { BoundBox, IFigures, Marker } from "./Marker";
import * as TreeStore from "../store/TreeStates";
// -----------------
// STATE - This defines the type of data maintained in the Redux store.

export interface MarkersState {
  isLoading: boolean;
  markers: IFigures;
  box: BoundBox;
  isChanging?: number;
}

// -----------------
// ACTIONS - These are serializable (hence replayable) descriptions of state transitions.
// They do not themselves have any side-effects; they just describe something that is going to happen.

interface RequestMarkersAction {
  type: "REQUEST_MARKERS";
  box: BoundBox;
}

interface ReceiveMarkersAction {
  type: "RECEIVE_MARKERS";
  box: BoundBox;
  markers: IFigures;
}

interface PostingMarkerAction {
  type: "POSTING_MARKERS";
  markers: IFigures;
}

interface PostedMarkerAction {
  type: "POSTED_MARKERS";
  markers: IFigures;
  success: boolean;
}

interface DeletingMarkerAction {
  type: "DELETING_MARKERS";
  ids_to_delete: string[];
}

interface DeletedMarkerAction {
  type: "DELETED_MARKERS";
  deleted_ids: string[];
  success: boolean;
}

// Declare a 'discriminated union' type. This guarantees that all references to 'type' properties contain one of the
// declared type strings (and not any other arbitrary string).
type KnownAction =
  | RequestMarkersAction
  | ReceiveMarkersAction
  | PostingMarkerAction
  | PostedMarkerAction
  | DeletingMarkerAction
  | DeletedMarkerAction;

// ----------------
// ACTION CREATORS - These are functions exposed to UI components that will trigger a state transition.
// They don't directly mutate state, but they can have external side-effects (such as loading data).

export const actionCreators = {
  requestMarkers: (box: BoundBox): AppThunkAction<KnownAction> => (
    dispatch,
    getState
  ) => {
    // Only load data if it's something we don't already have (and are not already loading)
    const appState = getState();

    if (
      appState &&
      appState.markersStates &&
      box !== appState.markersStates.box
    ) {

      let body = JSON.stringify(box);
      var request = ApiRootString + "/GetByBox";

      var fetched = fetch(request, {
        method: "POST",
        headers: { "Content-type": "application/json" },
        body: body
      });

      fetched
        .then(response => {
          if (!response.ok) throw response.statusText;
          var json = response.json();
          return json as Promise<IFigures>;
        })
        .then(data => {
          dispatch({ type: "RECEIVE_MARKERS", box: box, markers: data });
        })
        .catch((error) => {
          const emtyMarkers = {} as IFigures;
          dispatch({ type: "RECEIVE_MARKERS", box: box, markers: emtyMarkers });
        });

      dispatch({ type: "REQUEST_MARKERS", box: box });
    }
  },

  sendMarker: (markers: IFigures): AppThunkAction<KnownAction> => (
    dispatch,
    getState
  ) => {
    //let marker: Marker = {} as Marker;

    // Send data to the backend via POST
    let body = JSON.stringify(markers);
    var fetched = fetch(ApiRootString, {
      method: "POST",
      headers: { "Content-type": "application/json" },
      body: body
    });

    console.log("posted:", fetched);

    fetched.then(response => response.json()).then(data => {
      let m: IFigures = data as IFigures;
      dispatch({ type: "POSTED_MARKERS", success: true, markers: m });
    });

    dispatch({ type: "POSTING_MARKERS", markers: markers });
  },

  deleteMarker: (ids: string[]): AppThunkAction<KnownAction> => (
    dispatch,
    getState
  ) => {

    let body = JSON.stringify(ids);

    // Send data to the backend via DELETE

    var fetched = fetch(ApiRootString, {
      method: "DELETE",
      headers: { 'Content-type': 'application/json' },
      body: body
    });

    console.log("deleted:", fetched);

    fetched.then(response => response.json()).then(data => {
      let deleted_ids: string[] = data as string[];
      dispatch({ type: "DELETED_MARKERS", success: true, deleted_ids: deleted_ids });
    });

    dispatch({ type: "DELETING_MARKERS", ids_to_delete: ids });
  }
};

// ----------------
// REDUCER - For a given state and action, returns the new state. To support time travel, this must not mutate the old state.

const unloadedState: MarkersState = {
  markers: null,
  isLoading: false,
  box: null,
  isChanging: 0
};

export const reducer: Reducer<MarkersState> = (
  state: MarkersState | undefined,
  incomingAction: Action
): MarkersState => {
  if (state === undefined) {
    return unloadedState;
  }

  const action = incomingAction as KnownAction;

  switch (action.type) {
    case "REQUEST_MARKERS":
      return {
        ...state,
        box: action.box,
        markers: state.markers,
        isLoading: true
      };
    case "RECEIVE_MARKERS":
      // Only accept the incoming data if it matches the most recent request. This ensures we correctly
      // handle out-of-order responses.
      if (action.box === state.box) {
        return {
          ...state,
          box: action.box,
          markers: action.markers,
          isLoading: false
        };
      }
      break;

    case "POSTING_MARKERS":
      return {
        ...state,
        box: state.box,
        markers: state.markers,
        isLoading: true
      };
    case "POSTED_MARKERS":
      {
        var cur_markers: IFigures =
        {
          circles: state.markers.circles.concat(action.markers.circles),
          polygons: state.markers.polygons.concat(action.markers.polygons),
          polylines: state.markers.polylines.concat(action.markers.polylines)
        };

        return {
          ...state,
          markers: cur_markers,
          isLoading: false,
          isChanging: state.isChanging + 1
        };
      }

    case "DELETING_MARKERS":
      return {
        ...state,
        isLoading: true
      };
    case "DELETED_MARKERS":
      {
        var cur_markers: IFigures =
        {
          circles: state.markers.circles.filter(item => !(action.deleted_ids.includes(item.id))),
          polygons: state.markers.polygons.filter(item => !(action.deleted_ids.includes(item.id))),
          polylines: state.markers.polylines.filter(item => !(action.deleted_ids.includes(item.id)))
        };

        
        return {
          ...state,
          markers: cur_markers,
          isLoading: false,
          isChanging: state.isChanging + 1
        };
      }
  }

  return state;
};
