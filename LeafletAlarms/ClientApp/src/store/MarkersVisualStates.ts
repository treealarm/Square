import { Action, Reducer } from "redux";
import { ApiRootString, ApiStatesRootString, AppThunkAction } from "./";
import { MarkerVisualStateDTO } from "./Marker";


export interface MarkersVisualStates {
  visualStates: MarkerVisualStateDTO;
}

interface SetMarkersVisualStates {
  type: "SET_MARKERS_VISUAL_STATES";
  visualStates: MarkerVisualStateDTO;
}

interface UpdateMarkersVisualStates {
  type: "UPDATE_MARKERS_VISUAL_STATES";
  visualStates: MarkerVisualStateDTO;
}

interface GetMarkersVisualStates {
  type: "GET_MARKERS_VISUAL_STATES";
  visualStates: MarkerVisualStateDTO;
}

type KnownAction =
  | SetMarkersVisualStates
  | UpdateMarkersVisualStates
  | GetMarkersVisualStates
  ;

export const actionCreators = {
  setMarkersVisualStates: (visualStates: MarkerVisualStateDTO): AppThunkAction<KnownAction> => (
    dispatch,
    getState
  ) => {
    dispatch({ type: "SET_MARKERS_VISUAL_STATES", visualStates: visualStates });
  }
  ,
    updateMarkersVisualStates: (visualStates: MarkerVisualStateDTO): AppThunkAction<KnownAction> => (
      dispatch,
      getState
    ) => {
    dispatch({ type: "UPDATE_MARKERS_VISUAL_STATES", visualStates: visualStates });
  },
  requestMarkersVisualStates: (ids: string[]): AppThunkAction<KnownAction> => (
    dispatch,
    getState
  ) => {
 {

      let body = JSON.stringify(ids);
      var request = ApiStatesRootString + "/GetVisualStates";

      var fetched = fetch(request, {
        method: "POST",
        headers: { "Content-type": "application/json" },
        body: body
      });

      fetched
        .then(response => {
          if (!response.ok) throw response.statusText;
          var json = response.json();
          return json as Promise<MarkerVisualStateDTO>;
        })
        .then(data => {
          dispatch({ type: "SET_MARKERS_VISUAL_STATES", visualStates: data});
        })
        .catch((error) => {

        });
    }
  },
}

const unloadedState: MarkersVisualStates = {
  visualStates: {
    states: [],
    states_descr: []
  } 
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
    case "UPDATE_MARKERS_VISUAL_STATES":
      var newStates = state.visualStates.states
        .filter(item => null == action.visualStates.states.find(i => i.id == item.id));
      newStates = [...newStates, ...action.visualStates.states];

      var newDescrs = state.visualStates.states_descr
        .filter(item => null == action.visualStates.states_descr.find(i => i.id == item.id));
      newDescrs = [...newDescrs, ...action.visualStates.states_descr];

      var newVisualStates: MarkerVisualStateDTO =
      {
        states: newStates,
        states_descr: newDescrs
      }
      return {
        ...state,
        visualStates: newVisualStates
      };
    case "SET_MARKERS_VISUAL_STATES":
      return {
        ...state,
        visualStates: action.visualStates
      };
    }

  return state;
};