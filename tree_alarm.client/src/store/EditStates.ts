import { createSlice, PayloadAction } from '@reduxjs/toolkit';

// -----------------
// STATE

export const PolygonTool = 'Polygon';
export const CircleTool = 'Circle';
export const PolylineTool = 'Polyline';
export const DiagramTool = 'Diagram';

export const Figures: Record<string, string> = {
  Circle: 'Create Circle',
  Polyline: 'Create Polyline',
  Polygon: 'Create Polygon'
};

export const Diagrams: Record<string, string> = {
  Diagram: 'Create Diagram'
};

export interface EditState {
  edit_mode: boolean;
}

const initialState: EditState = {
  edit_mode: false
};

// -----------------
// SLICE

const editSlice = createSlice({
  name: 'edit',
  initialState,
  reducers: {
    setEditMode(state, action: PayloadAction<boolean>) {
      state.edit_mode = action.payload;
    }
  }
});

// -----------------
// ACTIONS

export const { setEditMode } = editSlice.actions;

// -----------------
// REDUCER

export const reducer = editSlice.reducer;
