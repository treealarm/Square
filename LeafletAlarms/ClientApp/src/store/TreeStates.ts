import { Action, Reducer } from 'redux';
import { ApiRootString, AppThunkAction } from './';
import { Marker } from './Marker';

// -----------------
// STATE - This defines the type of data maintained in the Redux store.

export interface TreeState {
  isLoading: boolean;
  parent_id: string|null;
  markers: Marker[];
}


// -----------------
// ACTIONS - These are serializable (hence replayable) descriptions of state transitions.
// They do not themselves have any side-effects; they just describe something that is going to happen.

interface RequestTreeStateAction {
  type: 'REQUEST_TREE_STATE';
  parent_id: string|null;
}

interface ReceiveTreeStateAction {
  type: 'RECEIVE_TREE_STATE';
  parent_id: string|null;
  markers: Marker[];
}

// Declare a 'discriminated union' type. This guarantees that all references to 'type' properties contain one of the
// declared type strings (and not any other arbitrary string).
type KnownAction = RequestTreeStateAction | ReceiveTreeStateAction;

// ----------------
// ACTION CREATORS - These are functions exposed to UI components that will trigger a state transition.
// They don't directly mutate state, but they can have external side-effects (such as loading data).

export const actionCreators = {
  getByParent: (parent_id: string|null): AppThunkAction<KnownAction> => (dispatch, getState) => {

    console.log("fetching by parent_id=", parent_id);
    var request = ApiRootString + '/GetByParent?parent_id=';

    if (parent_id != null)
    {
      request += parent_id;
    }

    var fetched = fetch(request);

    console.log("fetched:", fetched);

    fetched.then(response => response.json() as Promise<Marker[]>)
      .then(data => {
        dispatch({ type: 'RECEIVE_TREE_STATE', parent_id: parent_id, markers: data });
      });

    dispatch({ type: 'REQUEST_TREE_STATE', parent_id: parent_id });
  }
};

// ----------------
// REDUCER - For a given state and action, returns the new state. To support time travel, this must not mutate the old state.

const unloadedState: TreeState = {
  markers: [],
  isLoading: false,
  parent_id: null
};

export const reducer: Reducer<TreeState> = (state: TreeState | undefined, incomingAction: Action): TreeState => {
  if (state === undefined) {
    return unloadedState;
  }

  const action = incomingAction as KnownAction;

  switch (action.type) {
    case 'REQUEST_TREE_STATE':
      return {
        parent_id: action.parent_id,
        markers: state.markers,
        isLoading: true
      };
    case 'RECEIVE_TREE_STATE':
      // Only accept the incoming data if it matches the most recent request. This ensures we correctly
      // handle out-of-order responses.
      if (action.parent_id === state.parent_id) {
        return {
          parent_id: action.parent_id,
          markers: action.markers,
          isLoading: false
        };
      }
      break;
  }

  return state;
};
