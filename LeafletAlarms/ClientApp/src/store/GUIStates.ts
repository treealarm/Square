import { Action, Reducer } from 'redux';
import { ApiRootString, AppThunkAction } from './';

// -----------------
// STATE - This defines the type of data maintained in the Redux store.

export interface GUIState {
  selected_id: string | null;
  checked: string[]
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


// Declare a 'discriminated union' type. This guarantees that all references to 'type' properties contain one of the
// declared type strings (and not any other arbitrary string).
type KnownAction = TreeSelectionAction | TreeCheckingAction;

// ----------------
// ACTION CREATORS - These are functions exposed to UI components that will trigger a state transition.
// They don't directly mutate state, but they can have external side-effects (such as loading data).

export const actionCreators = {
  selectTreeItem: (selected_id: string|null): AppThunkAction<KnownAction> => (dispatch, getState) => {
    dispatch({ type: 'SELECT_TREE_ITEM', selected_id: selected_id });
  },
  
  checkTreeItem: (checked: string[]): AppThunkAction<KnownAction> => (dispatch, getState) => {
    dispatch({ type: 'CHECK_TREE_ITEM', checked: checked });
  }
};

const unloadedState: GUIState = {
  selected_id: null,
  checked: []
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
    case 'CHECK_TREE_ITEM':
      return {
        ...state,
        checked: action.checked
      };
  };

  return state;
};
