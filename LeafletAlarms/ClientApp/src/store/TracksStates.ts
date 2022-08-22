import { Action, Reducer } from "redux";
import { ApiTracksRootString, AppThunkAction } from "./";
import { BoundBox, ITrackPointDTO } from "./Marker";
// -----------------
// STATE - This defines the type of data maintained in the Redux store.

export interface TracksState {
  isLoading: boolean;
  tracks: ITrackPointDTO[];
  box: BoundBox;
}

// -----------------
// ACTIONS - These are serializable (hence replayable) descriptions of state transitions.
// They do not themselves have any side-effects; they just describe something that is going to happen.

interface RequestTracksAction {
  type: "REQUEST_TRACKS";
  box: BoundBox;
}

interface ReceiveTracksAction {
  type: "RECEIVE_TRACKS";
  box: BoundBox;
  tracks: ITrackPointDTO[];
}


// Declare a 'discriminated union' type. This guarantees that all references to 'type' properties contain one of the
// declared type strings (and not any other arbitrary string).
type KnownAction =
  | RequestTracksAction
  | ReceiveTracksAction
  ;

// ----------------
// ACTION CREATORS - These are functions exposed to UI components that will trigger a state transition.
// They don't directly mutate state, but they can have external side-effects (such as loading data).

export const actionCreators = {
  requestTracks: (box: BoundBox): AppThunkAction<KnownAction> => (
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
      var request = ApiTracksRootString + "/GetRoutsByBox";

      var fetched = fetch(request, {
        method: "POST",
        headers: { "Content-type": "application/json" },
        body: body
      });

      fetched
        .then(response => {
          if (!response.ok) throw response.statusText;
          var json = response.json();
          return json as Promise<ITrackPointDTO[]>;
        })
        .then(data => {
          dispatch({ type: "RECEIVE_TRACKS", box: box, tracks: data });
        })
        .catch((error) => {
          const emtyMarkers = {} as ITrackPointDTO[];
          dispatch({ type: "RECEIVE_TRACKS", box: box, tracks: emtyMarkers });
        });

      dispatch({ type: "REQUEST_TRACKS", box: box });
    }
  }
};

// ----------------
// REDUCER - For a given state and action, returns the new state. To support time travel, this must not mutate the old state.

const unloadedState: TracksState = {
  tracks: null,
  isLoading: false,
  box: null
};

export const reducer: Reducer<TracksState> = (
  state: TracksState | undefined,
  incomingAction: Action
): TracksState => {
  if (state === undefined) {
    return unloadedState;
  }

  const action = incomingAction as KnownAction;

  switch (action.type) {
    case "REQUEST_TRACKS":
      return {
        ...state,
        box: action.box,
        tracks : state.tracks,
        isLoading: true
      };
    case "RECEIVE_TRACKS":
      // Only accept the incoming data if it matches the most recent request. This ensures we correctly
      // handle out-of-order responses.
      if (action.box === state.box) {
        return {
          ...state,
          box: action.box,
          tracks: action.tracks,
          isLoading: false
        };
      }
      break;
  }

  return state;
};
