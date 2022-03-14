import { Action, Reducer } from 'redux';
import { ApiRootString, AppThunkAction } from './';

// -----------------
// STATE - This defines the type of data maintained in the Redux store.

export interface GUIState {
  selected_id: string | null;
}


// -----------------
// ACTIONS - These are serializable (hence replayable) descriptions of state transitions.
// They do not themselves have any side-effects; they just describe something that is going to happen.

interface TreeSelectionAction {
  type: 'SELECT_TREE_ITEM';
  selected_id: string | null;
}


// Declare a 'discriminated union' type. This guarantees that all references to 'type' properties contain one of the
// declared type strings (and not any other arbitrary string).
type KnownAction = TreeSelectionAction;

// ----------------
// ACTION CREATORS - These are functions exposed to UI components that will trigger a state transition.
// They don't directly mutate state, but they can have external side-effects (such as loading data).

export const actionCreators = {
  selectTreeItem: (selected_id: string|null): AppThunkAction<KnownAction> => (dispatch, getState) => {
    // Only load data if it's something we don't already have (and are not already loading)

    dispatch({ type: 'SELECT_TREE_ITEM', selected_id: selected_id });
  }
};

const unloadedState: GUIState = {
  selected_id: null
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
  };

  return state;
};
