import { Action, Reducer } from 'redux';
import { AppThunkAction } from '.';

// -----------------
export const PolygonTool = 'Polygon';
export const CircleTool = 'Circle';
export const PolylineTool = 'Polyline';
export const DiagramTool = 'Diagram';

export const Figures: Record<string, string> =
{
  Circle: 'Create Circle',
  Polyline: 'Create Polyline',
  Polygon: 'Create Polygon'
};

export const Diagrams: Record<string, string> =
{
  Diagram: 'Create Diagram'
};

export interface EditState {
  edit_mode: boolean;
}

// -----------------
// ACTIONS - These are serializable (hence replayable) descriptions of state transitions.
// They do not themselves have any side-effects; they just describe something that is going to happen.
// Use @typeName and isActionType for type detection that works even after serialization/deserialization.

export interface SetEditModeAction {
  type: 'SET_EDIT_MODE';
  edit_mode: boolean;
}

// Declare a 'discriminated union' type. This guarantees that all references to 'type' properties contain one of the
// declared type strings (and not any other arbitrary string).
export type KnownAction = SetEditModeAction;

// ----------------
// ACTION CREATORS - These are functions exposed to UI components that will trigger a state transition.
// They don't directly mutate state, but they can have external side-effects (such as loading data).

export const actionCreators = {

  setFigureEditMode: (edit_mode:boolean): AppThunkAction<KnownAction> => (dispatch, getState) => {
    dispatch({ type: 'SET_EDIT_MODE', edit_mode: edit_mode });
  }
};

const unloadedState: EditState = {
  edit_mode: false
};
// ----------------
// REDUCER - For a given state and action, returns the new state. To support time travel, this must not mutate the old state.

export const reducer: Reducer<EditState> = (state: EditState | undefined, incomingAction: Action): EditState => {
  if (state === undefined) {
      return unloadedState;
    }

    const action = incomingAction as KnownAction;
    switch (action.type) {
      case 'SET_EDIT_MODE':
        return { edit_mode: action.edit_mode };
        default:
            return state;
    }
};
