import * as ObjPropsStates from './ObjPropsStates';
import * as EditStates from './EditStates';
import * as MarkersStates from './MarkersStates';
import * as TreeStates from './TreeStates';
import * as GUIStates from './GUIStates';
import * as MarkersVisualStates from './MarkersVisualStates';
import * as TracksStates from './TracksStates';
import * as SearchResultStates from './SearchResultStates';
import * as ObjLogicStates from './ObjLogicStates';
import * as RightsStates from './RightsStates';
import * as PanelsStates from './PanelsStates';
import * as DiagramsStates from './DiagramsStates';
// The top-level state object
export interface ApplicationState {
  editState: EditStates.EditState | undefined;
  objPropsStates: ObjPropsStates.ObjPropsState | undefined;
  markersStates: MarkersStates.MarkersState | undefined;
  treeStates: TreeStates.TreeState | undefined;
  guiStates: GUIStates.GUIState | undefined;
  markersVisualStates: MarkersVisualStates.MarkersVisualStates | undefined;
  tracksStates: TracksStates.TracksState | undefined;
  searchResultStates: SearchResultStates.SearchResultState | undefined
  objLogicStates: ObjLogicStates.ObjLogicState | undefined;
  rightsStates: RightsStates.ObjectRights | undefined;
  panelsStates: PanelsStates.PanelsStates | undefined;
  diagramsStates: DiagramsStates.DiagramsStates | undefined;
}

// Whenever an action is dispatched, Redux will update each top-level application state property using
// the reducer with the matching name. It's important that the names match exactly, and that the reducer
// acts on the corresponding ApplicationState property type.
export const reducers = {
  editState: EditStates.reducer,
  objPropsStates: ObjPropsStates.reducer,
  markersStates: MarkersStates.reducer,
  treeStates: TreeStates.reducer,
  guiStates: GUIStates.reducer,
  markersVisualStates: MarkersVisualStates.reducer,
  tracksStates: TracksStates.reducer,
  searchResultStates: SearchResultStates.reducer,
  objLogicStates: ObjLogicStates.reducer,
  rightsStates: RightsStates.reducer,
  panelsStates: PanelsStates.reducer,
  diagramsStates: DiagramsStates.reducer
};

// This type can be used as a hint on action creators so that its 'dispatch' and 'getState' params are
// correctly typed to match your store.
export interface AppThunkAction<TAction> {
    (dispatch: (action: TAction) => void, getState: () => ApplicationState): void;
}


export const ApiRootString = 'api/map';
export const ApiTracksRootString = 'api/tracks';
export const ApiRouterRootString = 'api/router';
export const ApiStatesRootString = 'api/states';
export const ApiLogicRootString = 'api/logic';
export const ApiRightsRootString = 'api/rights';
export const ApiDiagramsRootString = 'api/diagrams';
export const ApiDefaultPagingNum = 100;
export const ApiDefaultMaxCountResult = 1000;