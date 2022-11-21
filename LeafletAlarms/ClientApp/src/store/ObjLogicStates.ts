import { Action, Reducer } from 'redux';
import { ApiLogicRootString, AppThunkAction } from '.';
import { IStaticLogicDTO } from './Marker';


// -----------------

export interface ObjLogicState {
  logic: IStaticLogicDTO[];
  updated: boolean;
}

// -----------------
// ACTIONS - These are serializable (hence replayable) descriptions of state transitions.
// They do not themselves have any side-effects; they just describe something that is going to happen.
// Use @typeName and isActionType for type detection that works even after serialization/deserialization.

export interface GetLogicAction {
  type: 'GET_LOGIC';
  object_id: string;
}

export interface SetLogicAction {
  type: 'SET_LOGIC';
  logic: IStaticLogicDTO[];
}

export interface SetLogicLocallyAction {
  type: 'SET_LOGIC_LOCALLY';
  logic: IStaticLogicDTO[];
}

export interface UpdateLogicAction {
  type: 'UPDATE_LOGIC';
  logic: IStaticLogicDTO[];
}

export interface UpdatingLogicAction {
  type: 'UPDATING_LOGIC';
  logic: IStaticLogicDTO[];
}

export interface UpdatedLogicAction {
  type: 'UPDATED_LOGIC';
  logic: IStaticLogicDTO[];
}

export interface DeleteLogicAction {
  type: 'DELETE_LOGIC';
  id: string;
}

// Declare a 'discriminated union' type. This guarantees that all references to 'type' properties contain one of the
// declared type strings (and not any other arbitrary string).
export type KnownAction =
  GetLogicAction | SetLogicAction | UpdateLogicAction
  | UpdatingLogicAction | SetLogicLocallyAction | UpdatedLogicAction
  | DeleteLogicAction;

// ----------------
// ACTION CREATORS - These are functions exposed to UI components that will trigger a state transition.
// They don't directly mutate state, but they can have external side-effects (such as loading data).

export const actionCreators = {
  getObjLogic: (object_id: string): AppThunkAction<KnownAction> => (dispatch, getState) => {

    if (object_id == null) {
      dispatch({ type: "SET_LOGIC", logic: null });
      return;
    }

    var request = ApiLogicRootString + "/GetByFigureAsync?id=" + object_id;

    var fetched = fetch(request);

    fetched
      .then(response => {
        if (!response.ok) throw response.statusText;
        var json = response.json();
        return json as Promise<IStaticLogicDTO[]>;
      })
      .then(data => {
        dispatch({ type: "SET_LOGIC", logic: data });
      })
      .catch((error) => {
        dispatch({ type: "SET_LOGIC", logic: null });
      });

    dispatch({ type: "GET_LOGIC", object_id: object_id });
  },
  updateObjLogic: (logic: IStaticLogicDTO[]): AppThunkAction<KnownAction> => (
    dispatch,
    getState
  ) => {

    // Send data to the backend via POST
    let body = JSON.stringify(logic);
    var fetched = fetch(ApiLogicRootString + "/UpdateLogic", {
      method: "POST",
      headers: { "Content-type": "application/json" },
      body: body
    });


    fetched.then(response => response.json()).then(data => {
      let m: IStaticLogicDTO[] = data as IStaticLogicDTO[];
      dispatch({ type: "UPDATED_LOGIC", logic: m });
    });

    dispatch({ type: "UPDATING_LOGIC", logic: logic });
  },
  setObjLogicLocally: (logic: IStaticLogicDTO[]): AppThunkAction<KnownAction> => (
    dispatch,
    getState
  ) => {
    dispatch({ type: "SET_LOGIC_LOCALLY", logic: logic });
  }
  ,
  delObjLogic: (logic_id: string): AppThunkAction<KnownAction> => (
    dispatch,
    getState
  ) => {

    var fetched = fetch(ApiLogicRootString + "/" + logic_id, {
      method: "DELETE"
    });

    fetched.then(response => response.text())
      .then((data) => {
      let deleted_id: string = data as string;
      dispatch({ type: "DELETE_LOGIC", id: deleted_id });
      })
      .catch(err => console.log(err));
   
  }
};

const unloadedState: ObjLogicState = {
  logic: null,
  updated: false
};
// ----------------
// REDUCER - For a given state and action, returns the new state. To support time travel, this must not mutate the old state.

export const reducer: Reducer<ObjLogicState> =
  (state: ObjLogicState | undefined, incomingAction: Action): ObjLogicState => {
  if (state === undefined) {
    return unloadedState;
  }

  const action = incomingAction as KnownAction;
  switch (action.type) {
    case 'GET_LOGIC':
      return {
        ...state,
        logic: state.logic
      };
    case 'SET_LOGIC':
      return { logic: action.logic, updated: false };
    case 'SET_LOGIC_LOCALLY':
      return { logic: action.logic, updated: false };

    case 'UPDATED_LOGIC':
      return {
        ...state,
        logic: action.logic,
        updated: true
      };
    case 'UPDATING_LOGIC':
      return { logic: action.logic, updated: false };
    case 'DELETE_LOGIC':
      var newLogic = state.logic.filter(i => i.id != action.id);
      return {
        ...state,
        logic: newLogic,
        updated: true
      };
    default:
      return state;
  }
};
