import * as WeatherForecasts from './WeatherForecasts';
import * as EditStates from './EditStates';
import * as MarkersStates from './MarkersStates';
import * as TreeStates from './TreeStates';
import * as GUIStates from './GUIStates';

// The top-level state object
export interface ApplicationState {
  editState: EditStates.EditState | undefined;
  weatherForecasts: WeatherForecasts.WeatherForecastsState | undefined;
  markersStates: MarkersStates.MarkersState | undefined;
  treeStates: TreeStates.TreeState | undefined;
  guiStates: GUIStates.GUIState | undefined;
}

// Whenever an action is dispatched, Redux will update each top-level application state property using
// the reducer with the matching name. It's important that the names match exactly, and that the reducer
// acts on the corresponding ApplicationState property type.
export const reducers = {
  editState: EditStates.reducer,
  weatherForecasts: WeatherForecasts.reducer,
  markersStates: MarkersStates.reducer,
  treeStates: TreeStates.reducer,
  guiStates: GUIStates.reducer
};

// This type can be used as a hint on action creators so that its 'dispatch' and 'getState' params are
// correctly typed to match your store.
export interface AppThunkAction<TAction> {
    (dispatch: (action: TAction) => void, getState: () => ApplicationState): void;
}

export const ApiRootString = 'api/map';
