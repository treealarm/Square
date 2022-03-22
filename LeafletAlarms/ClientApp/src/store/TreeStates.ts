﻿import { Action, Reducer } from "redux";
import { ApiRootString, AppThunkAction } from "./";
import { TreeMarker } from "./Marker";

// -----------------
// STATE - This defines the type of data maintained in the Redux store.

export interface TreeState {
  isLoading: boolean;
  parent_marker: TreeMarker | null;
  markers: TreeMarker[];
  parent_list: TreeMarker[];
}

// -----------------
// ACTIONS - These are serializable (hence replayable) descriptions of state transitions.
// They do not themselves have any side-effects; they just describe something that is going to happen.

interface RequestTreeStateAction {
  type: "REQUEST_TREE_STATE";
  parent_marker: TreeMarker | null;
}

interface ReceiveTreeStateAction {
  type: "RECEIVE_TREE_STATE";
  parent_marker: TreeMarker | null;
  markers: TreeMarker[];
}



// Declare a 'discriminated union' type. This guarantees that all references to 'type' properties contain one of the
// declared type strings (and not any other arbitrary string).
type KnownAction =
  | RequestTreeStateAction
  | ReceiveTreeStateAction
  ;

// ----------------
// ACTION CREATORS - These are functions exposed to UI components that will trigger a state transition.
// They don't directly mutate state, but they can have external side-effects (such as loading data).

export const actionCreators = {

  getByParent: (parent_marker: TreeMarker | null): AppThunkAction<KnownAction> => (
    dispatch,
    getState
  ) => {

    var parent_id = parent_marker?.id;

    console.log("fetching by parent_id=", parent_id);
    var request = ApiRootString + "/GetByParent?parent_id=";

    if (parent_id != null) {
      request += parent_id;
    }

    var fetched = fetch(request);

    console.log("fetched:", fetched);

    fetched
      .then(response => response.json() as Promise<TreeMarker[]>)
      .then(data => {
        dispatch({
          type: "RECEIVE_TREE_STATE",
          parent_marker: parent_marker,
          markers: data
        });
      });

    dispatch({ type: "REQUEST_TREE_STATE", parent_marker: parent_marker });
  }

};

// ----------------
// REDUCER - For a given state and action, returns the new state. To support time travel, this must not mutate the old state.

const unloadedState: TreeState = {
  markers: [],
  isLoading: false,
  parent_marker: null,
  parent_list:[]
};

export const reducer: Reducer<TreeState> = (
  state: TreeState | undefined,
  incomingAction: Action
): TreeState => {
  if (state === undefined) {
    return unloadedState;
  }

  const action = incomingAction as KnownAction;

  switch (action.type) {
    case "REQUEST_TREE_STATE":
      return {
        ...state,
        parent_marker: action.parent_marker,
        markers: state.markers,
        isLoading: true
      };
    case "RECEIVE_TREE_STATE":
      if (action.parent_marker?.id === state.parent_marker?.id) {

        var newArr = state.parent_list;
        var index = state.parent_list.findIndex((marker) => marker?.id == action?.parent_marker?.id);

        if (index >= 0) {
          newArr = state.parent_list.slice(0, index);
        }

        return {
          ...state,
          parent_marker: action.parent_marker,
          markers: action.markers,
          isLoading: false,
          parent_list: newArr.concat(action.parent_marker)
        };
      }
      break;
  }

  return state;
};