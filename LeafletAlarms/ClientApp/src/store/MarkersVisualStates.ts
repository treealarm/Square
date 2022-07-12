import { Action, Reducer } from "redux";
import { ApiRootString, AppThunkAction } from "./";
import { MarkerVisualState } from "./Marker";


export interface MarkersVisualStates {
  visualStates: MarkerVisualState[];
}

interface SetMarkersVisualStates {
  type: "SET_MARKERS_VISUAL_STATES";
  visualStates: MarkerVisualState[];
}

type KnownAction =
  | SetMarkersVisualStates
  ;

export const actionCreators = {
  setMarkersVisualStates: (visualStates: MarkerVisualState[]): AppThunkAction<KnownAction> => (
    dispatch,
    getState
  ) => {
    dispatch({ type: "SET_MARKERS_VISUAL_STATES", visualStates: visualStates });
  }
}

const unloadedState: MarkersVisualStates = {
  visualStates: []
};

export const reducer: Reducer<MarkersVisualStates> = (
  state: MarkersVisualStates | undefined,
  incomingAction: Action
): MarkersVisualStates => {

  if (state === undefined) {
    return unloadedState;
  }

  const action = incomingAction as KnownAction;

  switch (action.type) {
    case "SET_MARKERS_VISUAL_STATES":
      return {
        ...state,
        visualStates: action.visualStates
      };
    }

  return state;
};