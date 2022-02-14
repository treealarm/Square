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
  id: string;
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

// Declare a 'discriminated union' type. This guarantees that all references to 'type' properties contain one of the
// declared type strings (and not any other arbitrary string).
type KnownAction = RequestMarkersAction | ReceiveMarkersAction;

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
    }

    return state;
};
