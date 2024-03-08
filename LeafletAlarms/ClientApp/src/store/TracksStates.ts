import { Action, Reducer } from "redux";
import { ApiDefaultMaxCountResult, ApiRouterRootString, ApiTracksRootString, AppThunkAction } from "./";
import { DoFetch } from "./Fetcher";
import { BoxTrackDTO, IRoutDTO, IRoutLineDTO, ITrackPointDTO, SearchFilterGUI } from "./Marker";
// -----------------
// STATE - This defines the type of data maintained in the Redux store.

export interface TracksState {
  routes: IRoutLineDTO[];
  tracks: ITrackPointDTO[]
  box: BoxTrackDTO; 
  selected_track: ITrackPointDTO;
}

// -----------------
// ACTIONS - These are serializable (hence replayable) descriptions of state transitions.
// They do not themselves have any side-effects; they just describe something that is going to happen.

interface RequestRoutesAction {
  type: "REQUEST_ROUTS";
  box: BoxTrackDTO;
}

interface ReceiveRoutesAction {
  type: "RECEIVE_ROUTS";
  box: BoxTrackDTO;
  routes: IRoutLineDTO[];
}

interface ReceiveRoutesByIdAction {
  type: "RECEIVE_ROUTS_BY_ID";
  routes: IRoutLineDTO[];
}

interface RequestTracksAction {
  type: "REQUEST_TRACKS";
  box: BoxTrackDTO;
}

interface ReceiveTracksAction {
  type: "RECEIVE_TRACKS";
  box: BoxTrackDTO;
  tracks: ITrackPointDTO[];
}

interface SelectTrackAction {
  type: "SELECT_TRACK";
  selected_track: ITrackPointDTO;
}

// Declare a 'discriminated union' type. This guarantees that all references to 'type' properties contain one of the
// declared type strings (and not any other arbitrary string).
type KnownAction =
  | RequestRoutesAction
  | ReceiveRoutesAction
  | RequestTracksAction
  | ReceiveTracksAction  
  | SelectTrackAction
  | ReceiveRoutesByIdAction
  ;

// ----------------
// ACTION CREATORS - These are functions exposed to UI components that will trigger a state transition.
// They don't directly mutate state, but they can have external side-effects (such as loading data).

export const actionCreators = {

  OnSelectTrack: (selected_track: ITrackPointDTO): AppThunkAction<KnownAction> => (
    dispatch,
    getState
  ) => {
    let body = JSON.stringify([selected_track?.id]);
    var request = ApiRouterRootString + "/GetRoutesByTracksIds";

    var fetched = DoFetch(request, {
      method: "POST",
      headers: { "Content-type": "application/json" },
      body: body
    });

    fetched
      .then(response => {
        if (!response.ok) throw response.statusText;
        var json = response.json();
        return json as Promise<IRoutLineDTO[]>;
      })
      .then(data => {
        dispatch({ type: "RECEIVE_ROUTS_BY_ID", routes: data });
      })
      .catch((error) => {

      });

    dispatch({ type: "SELECT_TRACK", selected_track: selected_track });
  },

  requestRoutes: (box: BoxTrackDTO): AppThunkAction<KnownAction> => (
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
      box.count = ApiDefaultMaxCountResult;

      let body = JSON.stringify(box);
      var request = ApiRouterRootString + "/GetRoutesByBox";

      var fetched = DoFetch(request, {
        method: "POST",
        headers: { "Content-type": "application/json" },
        body: body
      });

      fetched
        .then(response => {
          if (!response.ok) throw response.statusText;
          var json = response.json();
          return json as Promise<IRoutLineDTO[]>;
        })
        .then(data => {
          dispatch({ type: "RECEIVE_ROUTS", box: box, routes: data });
        })
        .catch((error) => {
          const emtyMarkers: IRoutLineDTO[] = [];
          dispatch({ type: "RECEIVE_ROUTS", box: box, routes: emtyMarkers });
        });

      dispatch({ type: "REQUEST_ROUTS", box: box });
    }
  },
  requestRoutesByLine: (routRequest: IRoutDTO): AppThunkAction<KnownAction> => (
    dispatch,
    getState
  ) => {

    let body = JSON.stringify(routRequest);
    var request = ApiRouterRootString + "/GetRoute";

    var fetched = DoFetch(request, {
      method: "POST",
      headers: { "Content-type": "application/json" },
      body: body
    });

    fetched
      .then(response => {
        if (!response.ok) throw response.statusText;
        var json = response.json();
        return json as Promise<IRoutLineDTO[]>;
      })
      .then(data => {
        dispatch({ type: "RECEIVE_ROUTS_BY_ID", routes: data });
      })
      .catch((error) => {
        console.error(error);
      });
  },
  ///////////////////////////////////////////////////////////
  requestTracks: (box: BoxTrackDTO): AppThunkAction<KnownAction> => (
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
      var request = ApiTracksRootString + "/GetTracksByBox";

      var fetched = DoFetch(request, {
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
          const emtyMarkers : ITrackPointDTO[] = [];
          dispatch({ type: "RECEIVE_TRACKS", box: box, tracks: emtyMarkers });
        });

      dispatch({ type: "REQUEST_TRACKS", box: box });
    }
  }

  
};

// ----------------
// REDUCER - For a given state and action, returns the new state. To support time travel, this must not mutate the old state.

const unloadedState: TracksState = {
  routes: null,
  tracks: null,
  box: null,
  selected_track: null
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

    case "RECEIVE_ROUTS_BY_ID":
    return {
      ...state,
      routes: action.routes
    };

    case "SELECT_TRACK":
      {
          return {
            ...state,
            selected_track: action.selected_track
          };
      }
      

    case "REQUEST_ROUTS":
      return {
        ...state,
        box: action.box
      };
    case "RECEIVE_ROUTS":
      if (action.box === state.box) {
        return {
          ...state,
          box: action.box,
          routes: action.routes
        };
      }
      break;

    case "REQUEST_TRACKS":
      return {
        ...state,
        box: action.box
      };
    case "RECEIVE_TRACKS":
      if (action.box === state.box) {
        return {
          ...state,
          box: action.box,
          tracks: action.tracks
        };
      }
      break;

  }

  return state;
};
