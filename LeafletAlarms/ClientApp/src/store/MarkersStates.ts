import { Action, Reducer } from 'redux';
import { AppThunkAction } from './';

// -----------------
// STATE - This defines the type of data maintained in the Redux store.

export interface MarkersState {
  isLoading: boolean;
  markers: Marker[];
  box: string;
}

export interface Marker {
  id?: string;
  name: string;
  points: number[];
}

// -----------------
// ACTIONS - These are serializable (hence replayable) descriptions of state transitions.
// They do not themselves have any side-effects; they just describe something that is going to happen.

interface RequestMarkersAction {
  type: 'REQUEST_MARKERS';
  box: string;
}

interface ReceiveMarkersAction {
  type: 'RECEIVE_MARKERS';
  box: string;
  markers: Marker[];
}

interface PostingMarkerAction {
  type: 'POSTING_MARKERS';
  marker: Marker;
}

interface PostedMarkerAction {
  type: 'POSTED_MARKERS';
  marker: Marker;
  success: boolean;
}

interface DeletingMarkerAction {
  type: 'DELETING_MARKERS';
  marker: Marker;
}

interface DeletedMarkerAction {
  type: 'DELETED_MARKERS';
  marker: Marker;
  success: boolean;
}

// Declare a 'discriminated union' type. This guarantees that all references to 'type' properties contain one of the
// declared type strings (and not any other arbitrary string).
type KnownAction =
  RequestMarkersAction | ReceiveMarkersAction |
  PostingMarkerAction | PostedMarkerAction |
  DeletingMarkerAction | DeletedMarkerAction;

// ----------------
// ACTION CREATORS - These are functions exposed to UI components that will trigger a state transition.
// They don't directly mutate state, but they can have external side-effects (such as loading data).

export const actionCreators = {
  requestMarkers: (box: string): AppThunkAction<KnownAction> => (dispatch, getState) => {
        // Only load data if it's something we don't already have (and are not already loading)
    const appState = getState();

    if (appState && appState.markersStates && box !== appState.markersStates.box) {

      console.log("fetching....");

      var fetched = fetch('api/map');

      console.log("fetched:", fetched);

      fetched.then(response => response.json() as Promise<Marker[]>)
        .then(data => {
          dispatch({ type: 'RECEIVE_MARKERS', box: box, markers: data });
        });

      dispatch({ type: 'REQUEST_MARKERS', box: box });
    }
  },

  sendMarker: (marker: Marker): AppThunkAction<KnownAction> => (dispatch, getState) => {

    let appState = getState();
    appState.markersStates.isLoading = false;
    //let marker: Marker = {} as Marker;

    // Send data to the backend via POST
    let body = JSON.stringify(marker);
    var fetched = fetch('api/map', {
      method: 'POST',
      headers: { 'Content-type': 'application/json' },
      body: body
    });

    console.log("posted:", fetched);

    fetched.then(response => response.json())
      .then(data => {
        let m: Marker = data as Marker;
        dispatch({ type: 'POSTED_MARKERS', success: true , marker: m});
      });

    dispatch({ type: 'POSTING_MARKERS', marker: marker });
  },

  deleteMarker: (marker: Marker): AppThunkAction<KnownAction> => (dispatch, getState) => {

    let appState = getState();
    appState.markersStates.isLoading = false;
    //let marker: Marker = {} as Marker;

    // Send data to the backend via DELETE
    let body = JSON.stringify(marker);

    var fetched = fetch('api/map/' + marker.id, {
      method: 'DELETE',
      //headers: { 'Content-type': 'application/json' },
      //body: body
    });

    console.log("deleted:", fetched);

    fetched.then(response => response.json())
      .then(data => {
        let m: Marker = data as Marker;
        dispatch({ type: 'DELETED_MARKERS', success: true, marker: m });
      });

    dispatch({ type: 'DELETING_MARKERS', marker: marker });
  }
};

// ----------------
// REDUCER - For a given state and action, returns the new state. To support time travel, this must not mutate the old state.

const unloadedState: MarkersState = { markers: [], isLoading: false , box: ''};

export const reducer: Reducer<MarkersState> = (state: MarkersState | undefined, incomingAction: Action): MarkersState => {
    if (state === undefined) {
        return unloadedState;
    }

  const action = incomingAction as KnownAction;

    switch (action.type) {
        case 'REQUEST_MARKERS':
            return {
                box: action.box,
                markers: state.markers,
                isLoading: true
            };
        case 'RECEIVE_MARKERS':
            // Only accept the incoming data if it matches the most recent request. This ensures we correctly
        // handle out-of-order responses.
        if (action.box === state.box) {
                return {
                  box: action.box,
                  markers: action.markers,
                    isLoading: false
                };
            }
        break;

      case 'POSTING_MARKERS':
        return {
          box: state.box,
          markers: state.markers,
          isLoading: false
        };
      case 'POSTED_MARKERS':
        return {
          ...state,
          markers: [...state.markers, action.marker]
        };

      case 'DELETING_MARKERS':
        return {
          box: state.box,
          markers: state.markers,
          isLoading: false
        };
      case 'DELETED_MARKERS':
        return {
          ...state,
          markers: state.markers.filter(item => item.id !== action.marker.id),
        };
    }

    return state;
};
