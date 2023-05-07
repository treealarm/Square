import dayjs from 'dayjs';
import { Action, Reducer } from 'redux';
import { AppThunkAction } from './';
import { SearchFilterGUI, ViewOption } from './Marker';

// -----------------
// STATE - This defines the type of data maintained in the Redux store.

export interface GUIState {
  selected_id: string | null;
  checked: string[],
  requestedTreeUpdate?: number,
  map_option: ViewOption | null;
  searchFilter: SearchFilterGUI;  
}


// -----------------
// ACTIONS - These are serializable (hence replayable) descriptions of state transitions.
// They do not themselves have any side-effects; they just describe something that is going to happen.

interface TreeSelectionAction {
  type: 'SELECT_TREE_ITEM';
  selected_id: string | null;  
}

interface TreeCheckingAction {
  type: 'CHECK_TREE_ITEM';
  checked: string[]
}

interface TreeUpdateRequestedAction {
  type: 'UPDATE_TREE';
}

interface SetMapOptionAction {
  type: 'SET_MAP_OPTION';
  map_option: ViewOption | null;
}

interface ApplySearchFilterAction {
  type: "APPLY_FILTER";
  searchFilter: SearchFilterGUI
}


// Declare a 'discriminated union' type. This guarantees that all references to 'type' properties contain one of the
// declared type strings (and not any other arbitrary string).
type KnownAction = TreeSelectionAction
  | TreeCheckingAction
  | TreeUpdateRequestedAction
  | SetMapOptionAction
  | ApplySearchFilterAction
  ;

// ----------------
// ACTION CREATORS - These are functions exposed to UI components that will trigger a state transition.
// They don't directly mutate state, but they can have external side-effects (such as loading data).

export const actionCreators = {
  selectTreeItem: (selected_id: string | null): AppThunkAction<KnownAction> => (dispatch, getState) => {
    console.log("selected_id:", selected_id);
    dispatch({
      type: 'SELECT_TREE_ITEM',
      selected_id: selected_id
    });
  },
  setMapOption: (map_option: ViewOption | null): AppThunkAction<KnownAction> => (dispatch, getState) => {
    dispatch({
      type: 'SET_MAP_OPTION',
      map_option: map_option
    });
  },
  checkTreeItem: (checked: string[]): AppThunkAction<KnownAction> => (dispatch, getState) => {
    dispatch({ type: 'CHECK_TREE_ITEM', checked: checked });
  }
  ,

  requestTreeUpdate: (): AppThunkAction<KnownAction> => (dispatch, getState) => {
    dispatch({ type: 'UPDATE_TREE'});
  }
  ,
  ///////////////////////////////////////////////////////////
  applyFilter: (filter: SearchFilterGUI): AppThunkAction<KnownAction> => (
    dispatch
  ) => {
    dispatch({ type: "APPLY_FILTER", searchFilter: filter });
  }
};

const unloadedState: GUIState = {
  selected_id: null,
  checked: [],
  requestedTreeUpdate: 0,
  map_option: { map_center: null }
  ,
  searchFilter: {
    time_start: dayjs().subtract(1, "day").toISOString(),
    time_end: dayjs().toISOString(),
    property_filter: {
      props: [{ prop_name: "track_name", str_val: "lisa_alert" }]
    },
    search_id: ""
  }
};

export const reducer: Reducer<GUIState> = (state: GUIState | undefined, incomingAction: Action): GUIState => {

  if (state === undefined) {
    return unloadedState;
  }

  const action = incomingAction as KnownAction;

  switch (action.type) {
    case 'SELECT_TREE_ITEM':
      return {
        ...state,
        selected_id: action.selected_id
      };
    case 'SET_MAP_OPTION':
      return {
        ...state,
        map_option: action.map_option
      };
      
    case 'CHECK_TREE_ITEM':
      return {
        ...state,
        checked: action.checked
      };
    case 'UPDATE_TREE':
      return {
        ...state,
        requestedTreeUpdate: state.requestedTreeUpdate + 1
      };

    case "APPLY_FILTER":
      return {
        ...state,
        searchFilter: action.searchFilter
      };
      break;
    default:
      break;
  };

  return state;
};
