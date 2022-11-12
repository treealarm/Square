import { Action, Reducer } from "redux";
import { ApiRootString, AppThunkAction } from "./";
import { GetBySearchDTO, SearchFilterDTO, TreeMarker } from "./Marker";

// -----------------
// STATE - This defines the type of data maintained in the Redux store.

export interface SearchResultState {
  list: TreeMarker[];
  filter: SearchFilterDTO;
}

// -----------------
// ACTIONS - These are serializable (hence replayable) descriptions of state transitions.
// They do not themselves have any side-effects; they just describe something that is going to happen.

interface RequestSearchStateAction {
  type: "REQUEST_SEARCH_STATE";
  filter: SearchFilterDTO;
}

interface ReceiveSearchStateAction {
  type: "RECEIVE_SEARCH_STATE";
  data: GetBySearchDTO
}


// Declare a 'discriminated union' type. This guarantees that all references to 'type' properties contain one of the
// declared type strings (and not any other arbitrary string).
type KnownAction =
  | RequestSearchStateAction
  | ReceiveSearchStateAction
  ;

// ----------------
// ACTION CREATORS - These are functions exposed to UI components that will trigger a state transition.
// They don't directly mutate state, but they can have external side-effects (such as loading data).

export const actionCreators = {

  setEmptyResult: (
  ): AppThunkAction<KnownAction> => (
    dispatch,
    getState
  ) => {
    var emptyResult: GetBySearchDTO = {
      search_id: "",
      list: []
    }
    dispatch({
      type: "RECEIVE_SEARCH_STATE",
      data: emptyResult
    });
    },

  getByFilter: (
    filter: SearchFilterDTO
  ): AppThunkAction<KnownAction> => (
    dispatch,
    getState
  ) => {


    let body = JSON.stringify(filter);
    var request = ApiRootString + "/GetByFilter";

    var fetched = fetch(request, {
      method: "POST",
      headers: { "Content-type": "application/json" },
      body: body
    });

      fetched
        .then(response => response.json() as Promise<GetBySearchDTO>)
        .then(data => {
          dispatch({
            type: "RECEIVE_SEARCH_STATE",
            data: data
          });
        });

    dispatch({ type: "REQUEST_SEARCH_STATE", filter: filter });
    }
};

// ----------------
// REDUCER - For a given state and action, returns the new state. To support time travel, this must not mutate the old state.

const unloadedState: SearchResultState = {
  list: [],
  filter: null
};

export const reducer: Reducer<SearchResultState> = (
  state: SearchResultState | undefined,
  incomingAction: Action
): SearchResultState => {
  if (state === undefined) {
    return unloadedState;
  }

  const action = incomingAction as KnownAction;

  switch (action.type) {
    case "REQUEST_SEARCH_STATE":
      return {
        ...state,
        filter: action.filter
      };

    case "RECEIVE_SEARCH_STATE":
      if (action.data.search_id == state.filter.search_id ||
        action.data.search_id == "") {
        return {
          ...state,
          list: action.data.list
        };
      }
      break;
  }

  return state;
};
