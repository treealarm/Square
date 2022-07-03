import { Action, Reducer } from 'redux';
import { ApiRootString, AppThunkAction } from '.';
import { IObjProps } from './Marker';


// -----------------

export interface ObjPropsState {
  objProps: IObjProps;
  updated: boolean;
}

// -----------------
// ACTIONS - These are serializable (hence replayable) descriptions of state transitions.
// They do not themselves have any side-effects; they just describe something that is going to happen.
// Use @typeName and isActionType for type detection that works even after serialization/deserialization.

export interface GetPropsAction {
  type: 'GET_PROPS';
  object_id: string;
}

export interface SetPropsAction {
  type: 'SET_PROPS';
  objProps: IObjProps;
}

export interface SetPropsLocallyAction {
  type: 'SET_PROPS_LOCALLY';
  objProps: IObjProps;
}

export interface UpdatedPropsAction {
  type: 'UPDATED_PROPS';
  objProps: IObjProps;
}

export interface UpdatingDPropsAction {
  type: 'UPDATING_PROPS';
  objProps: IObjProps;
}

// Declare a 'discriminated union' type. This guarantees that all references to 'type' properties contain one of the
// declared type strings (and not any other arbitrary string).
export type KnownAction =
  GetPropsAction | SetPropsAction | UpdatedPropsAction | UpdatingDPropsAction | SetPropsLocallyAction;

// ----------------
// ACTION CREATORS - These are functions exposed to UI components that will trigger a state transition.
// They don't directly mutate state, but they can have external side-effects (such as loading data).

export const actionCreators = {
  getObjProps: (object_id: string): AppThunkAction<KnownAction> => (dispatch, getState) => {

    if (object_id == null) {
      dispatch({ type: "SET_PROPS", objProps: null });
      return;
    }

    var request = ApiRootString + "/GetObjProps?id=" + object_id;

    var fetched = fetch(request);

    fetched
      .then(response => {
        if (!response.ok) throw response.statusText;
        var json = response.json();
        return json as Promise<IObjProps>;
      })
      .then(data => {
        dispatch({ type: "SET_PROPS", objProps: data });
      })
      .catch((error) => {
        dispatch({ type: "SET_PROPS", objProps: null });
      });

    dispatch({ type: "GET_PROPS", object_id: object_id });
  },
  updateObjProps: (marker: IObjProps): AppThunkAction<KnownAction> => (
    dispatch,
    getState
  ) => {

    // Send data to the backend via POST
    let body = JSON.stringify(marker);
    var fetched = fetch(ApiRootString + "/UpdateProperties", {
      method: "POST",
      headers: { "Content-type": "application/json" },
      body: body
    });


    fetched.then(response => response.json()).then(data => {
      let m: IObjProps = data as IObjProps;
      dispatch({ type: "UPDATED_PROPS", objProps: m });
    });

    dispatch({ type: "UPDATING_PROPS", objProps: marker });
  },
  setObjPropsLocally: (marker: IObjProps): AppThunkAction<KnownAction> => (
    dispatch,
    getState
  ) => {
    dispatch({ type: "SET_PROPS_LOCALLY", objProps: marker });
  }
};

const unloadedState: ObjPropsState = {
  objProps: null,
  updated: false
};
// ----------------
// REDUCER - For a given state and action, returns the new state. To support time travel, this must not mutate the old state.

export const reducer: Reducer<ObjPropsState> = (state: ObjPropsState | undefined, incomingAction: Action): ObjPropsState => {
  if (state === undefined) {
    return unloadedState;
  }

  const action = incomingAction as KnownAction;
  switch (action.type) {
    case 'GET_PROPS':
      return {
        ...state,
        objProps: state.objProps
      };
    case 'SET_PROPS':
      return { objProps: action.objProps, updated: false };
    case 'SET_PROPS_LOCALLY':
      return { objProps: action.objProps, updated: false };

    case 'UPDATED_PROPS':
      return {
        ...state,
        objProps: action.objProps,
        updated: true
      };
    case 'UPDATING_PROPS':
      return { objProps: action.objProps, updated: false };
    default:
      return state;
  }
};
