import { Action, Reducer } from "redux";
import { ApiRootString, AppThunkAction } from "./";
import { GetBySearchDTO, SearchFilter, TreeMarker } from "./Marker";

// -----------------
// STATE - This defines the type of data maintained in the Redux store.

export interface SearchResultState {
  list: TreeMarker[];
  start_id?: string;
  end_id?: string;
  searchFilter: SearchFilter;
  search_id: string
}

// -----------------
// ACTIONS - These are serializable (hence replayable) descriptions of state transitions.
// They do not themselves have any side-effects; they just describe something that is going to happen.

interface RequestSearchStateAction {
  type: "REQUEST_SEARCH_STATE";
  search_id: string;
}

interface ReceiveSearchStateAction {
  type: "RECEIVE_SEARCH_STATE";
  data: GetBySearchDTO
}

interface ApplySearchFilterAction {
  type: "APPLY_SEARCH_FILTER";
  searchFilter: SearchFilter
}



// Declare a 'discriminated union' type. This guarantees that all references to 'type' properties contain one of the
// declared type strings (and not any other arbitrary string).
type KnownAction =
  | RequestSearchStateAction
  | ReceiveSearchStateAction
  | ApplySearchFilterAction
  ;

// ----------------
// ACTION CREATORS - These are functions exposed to UI components that will trigger a state transition.
// They don't directly mutate state, but they can have external side-effects (such as loading data).

export const actionCreators = {

  getByFilter: (
    start_id: string | null,
    end_id: string | null
  ): AppThunkAction<KnownAction> => (
    dispatch,
    getState
  ) => {

      console.log("fetching by filter");
      var request = ApiRootString + "/GetByFilter?count=100";


      if (start_id != null) {
        request += "&start_id=" + start_id;
      }

      if (end_id != null) {
        request += "&end_id=" + end_id;
      }

      var fetched = fetch(request);

      console.log("fetched:", fetched);

      fetched
        .then(response => response.json() as Promise<GetBySearchDTO>)
        .then(data => {
          dispatch({
            type: "RECEIVE_SEARCH_STATE",
            data: data
          });
        });

    dispatch({ type: "REQUEST_SEARCH_STATE", search_id: (new Date()).toISOString() });
    },
};

// ----------------
// REDUCER - For a given state and action, returns the new state. To support time travel, this must not mutate the old state.

const unloadedState: SearchResultState = {
  list: [],
  start_id: null,
  end_id: null,
  searchFilter: null,
  search_id: ""
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
        search_id: action.search_id
      };

    case "RECEIVE_SEARCH_STATE":
      if (action.data.search_id == state.search_id) {
        return {
          ...state,
          list: action.data.list,
          start_id: action.data.start_id,
          end_id: action.data.end_id
        };
      }
      break;
  }

  return state;
};
