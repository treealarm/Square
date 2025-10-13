import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import dayjs from 'dayjs';
import { MapViewOption, SearchFilterGUI, ViewOption } from './Marker';

// -----------------
// STATE

export interface GUIState {
  selected_id: string | null;
  checked: string[];
  requestedTreeUpdate: number;
  map_option: ViewOption | null;
  cur_map_view: MapViewOption | null;
  searchFilter: SearchFilterGUI;
  diagramDiving: boolean;

  cur_interface: string | null;
}

const initialState: GUIState = {
  selected_id: null,
  checked: [],
  requestedTreeUpdate: 0,
  map_option: { map_center: null },
  cur_map_view: { map_center: null },
  searchFilter: {
    time_start: dayjs().subtract(1, "day").toISOString(),
    time_end: dayjs().toISOString(),
    property_filter: {
      props: [{ prop_name: "track_name", str_val: "lisa_alert" }]
    },
    search_id: ""
  },
  diagramDiving: false,
  cur_interface: null
};

// -----------------
// SLICE

const guiSlice = createSlice({
  name: 'gui',
  initialState,
  reducers: {
    setDiagramDivingMode(state, action: PayloadAction<boolean>) {
      state.diagramDiving = action.payload;
    },
    selectTreeItem(state, action: PayloadAction<string | null>) {
      state.selected_id = action.payload;
    },
    checkTreeItem(state, action: PayloadAction<string[]>) {
      state.checked = action.payload;
    },
    requestTreeUpdate(state) {
      state.requestedTreeUpdate += 1;
    },
    setMapOption(state, action: PayloadAction<ViewOption | null>) {
      state.map_option = action.payload;
    },
    updateCurMapOption(state, action: PayloadAction<MapViewOption | null>) {
      state.cur_map_view = action.payload;
    },
    applyFilter(state, action: PayloadAction<SearchFilterGUI>) {
      state.searchFilter = action.payload;
    },
    set_cur_interface(state, action: PayloadAction<string>) {
      state.cur_interface = action.payload;
    }
  }
});

// -----------------
// ACTIONS

export const {
  selectTreeItem,
  checkTreeItem,
  requestTreeUpdate,
  setMapOption,
  updateCurMapOption,
  applyFilter,
  set_cur_interface,
  setDiagramDivingMode
} = guiSlice.actions;

// -----------------
// THUNKS

export const applySearchFilter = (filter: SearchFilterGUI): AppThunk => (dispatch) => {
  dispatch(applyFilter(filter));
};

// -----------------
// REDUCER

export const reducer = guiSlice.reducer;
