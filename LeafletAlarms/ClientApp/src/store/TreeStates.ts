import { Action, Reducer } from "redux";
import { ApiRootString, AppThunkAction } from "./";
import { DoFetch } from "./Fetcher";
import { GetByParentDTO, TreeMarker } from "./Marker";

// -----------------
// STATE - This defines the type of data maintained in the Redux store.

export interface TreeState {
  isLoading: boolean;
  parent_id: string | null;
  parents: TreeMarker[];
  children: TreeMarker[];  
  start_id?: string;
  end_id?: string;
}

// -----------------
// ACTIONS - These are serializable (hence replayable) descriptions of state transitions.
// They do not themselves have any side-effects; they just describe something that is going to happen.

interface RequestTreeStateAction {
  type: "REQUEST_TREE_STATE";
  parent_marker_id: string | null;
}

interface ReceiveTreeStateAction {
  type: "RECEIVE_TREE_STATE";
  data: GetByParentDTO
}

interface SetTreeItemAction {
  type: "SET_TREE_ITEM";
  item: TreeMarker
}



// Declare a 'discriminated union' type. This guarantees that all references to 'type' properties contain one of the
// declared type strings (and not any other arbitrary string).
type KnownAction =
  | RequestTreeStateAction
  | ReceiveTreeStateAction
  | SetTreeItemAction
  ;

// ----------------
// ACTION CREATORS - These are functions exposed to UI components that will trigger a state transition.
// They don't directly mutate state, but they can have external side-effects (such as loading data).

export const actionCreators = {

  getByParent: (
    parent_id: string | null,
    start_id: string | null,
    end_id: string | null
  ): AppThunkAction<KnownAction> => (
    dispatch,
    getState
  ) => {

    console.log("fetching by parent_id=", parent_id);
    var request = ApiRootString + "/GetByParent?count=100";

    if (parent_id != null) {
      request += "&parent_id=" + parent_id;
    }

    if (start_id != null) {
      request += "&start_id=" + start_id;
    }

    if (end_id != null) {
      request += "&end_id=" + end_id;
    }

    var fetched = DoFetch(request);

    console.log("fetched:", fetched);

    fetched
      .then(response => response.json() as Promise<GetByParentDTO>)
      .then(data => {
        dispatch({
          type: "RECEIVE_TREE_STATE",
          data: data
        });
      });

    dispatch({ type: "REQUEST_TREE_STATE", parent_marker_id: parent_id });
  },

  setTreeItem: (treeItem: TreeMarker | null): AppThunkAction<KnownAction> => (
    dispatch,
    getState
  ) => {
    dispatch({ type: "SET_TREE_ITEM", item: treeItem });
  }
};

// ----------------
// REDUCER - For a given state and action, returns the new state. To support time travel, this must not mutate the old state.

const unloadedState: TreeState = {
  children: [],
  isLoading: false,
  parent_id: null,
  parents:[]
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
        parent_id: action.parent_marker_id,
        children: state.children,
        isLoading: true
      };
    case "RECEIVE_TREE_STATE":
      if (action.data.parent_id == state.parent_id) {
        return {
          ...state,
          parent_id: action.data.parent_id,
          children: action.data.children,
          isLoading: false,
          parents: [null, ...action.data.parents],
          start_id: action.data.start_id,
          end_id: action.data.end_id
        };
      }
      break;
    case "SET_TREE_ITEM":

      var found = state.children.find(i => i.id == action.item.id);

      if (found == null) {
        return state;
      }

      var newMarkers = state.children.map((item : any): TreeMarker => {
        if (item.id == action.item.id) {
          item.name = action.item.name;
        }
        return item;
      }
      );

        return {
          ...state,
          children: newMarkers,
        };

      break;
      
  }

  return state;
};
