import { Action, Reducer } from 'redux';
import { ApiRightsRootString, AppThunkAction } from '.';
import { IObjectRightsDTO } from './Marker';
import { DoFetch } from './Fetcher';
// -----------------

export interface ObjectRights {
  rights: IObjectRightsDTO[];
  updated: boolean;
}

// -----------------
// ACTIONS - These are serializable (hence replayable) descriptions of state transitions.
// They do not themselves have any side-effects; they just describe something that is going to happen.
// Use @typeName and isActionType for type detection that works even after serialization/deserialization.

export interface GetRightsAction {
  type: 'GET_RIGHTS';
  object_id: string[];
}

export interface SetRightsAction {
  type: 'SET_RIGHTS';
  rights: IObjectRightsDTO[];
}

// Declare a 'discriminated union' type. This guarantees that all references to 'type' properties contain one of the
// declared type strings (and not any other arbitrary string).
export type KnownAction =
  GetRightsAction | SetRightsAction;

// ----------------
// ACTION CREATORS - These are functions exposed to UI components that will trigger a state transition.
// They don't directly mutate state, but they can have external side-effects (such as loading data).

export const actionCreators = {
  getObjRight: (object_ids: string[]): AppThunkAction<KnownAction> => (dispatch, getState) => {

    if (object_ids == null) {
      dispatch({ type: "SET_RIGHTS", rights: null });
      return;
    }

    let body = JSON.stringify(object_ids);
    var fetched = DoFetch(ApiRightsRootString + "/GetRights", {
      method: "POST",
      headers: { "Content-type": "application/json" },
      body: body
    });

    fetched
      .then(response => {
        if (!response.ok) throw response.statusText;
        var json = response.json();
        return json as Promise<IObjectRightsDTO[]>;
      })
      .then(data => {
        dispatch({ type: "SET_RIGHTS", rights: data });
      })
      .catch((error: any) => {
        dispatch({ type: "SET_RIGHTS", rights: null });
      });

    dispatch({ type: "GET_RIGHTS", object_id: object_ids });
  }
};

const unloadedState: ObjectRights = {
  rights: null,
  updated: false
};
// ----------------
// REDUCER - For a given state and action, returns the new state. To support time travel, this must not mutate the old state.

export const reducer: Reducer<ObjectRights> =
  (state: ObjectRights | undefined, incomingAction: Action): ObjectRights => {
    if (state === undefined) {
      return unloadedState;
    }

    const action = incomingAction as KnownAction;
    switch (action.type) {
      case 'GET_RIGHTS':
        return {
          ...state,
          rights: state.rights
        };
      case 'SET_RIGHTS':
        return { rights: action.rights, updated: false };
     
      default:
        return state;
    }
  };
