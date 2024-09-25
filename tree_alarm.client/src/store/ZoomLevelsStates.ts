import { createSlice, PayloadAction, createAsyncThunk } from '@reduxjs/toolkit';
import { ApiRootString } from './constants'; // Adjust import as needed
import { DoFetch } from './Fetcher';
import { ILevelDTO } from './Marker';


export interface ZoomLevelsState {
  levels: ILevelDTO[];
  isLoading: boolean;
  error: string | null;
}

// Initial state for the zoom levels
const initialState: ZoomLevelsState = {
  levels: [],
  isLoading: false,
  error: null,
};

// Async thunk to fetch zoom levels
export const fetchZoomLevels = createAsyncThunk<ILevelDTO[], void>(
  'zoom/fetchZoomLevels',
  async () => {
    const response = await DoFetch(`${ApiRootString}/GetZoomLevels`);
    if (!response.ok) {
      throw new Error('Failed to fetch zoom levels');
    }
    return (await response.json()) as ILevelDTO[];
  }
);

// Create the slice
const zoomSlice = createSlice({
  name: 'zoom',
  initialState,
  reducers: {
    // You can define any additional synchronous actions here if needed
    clearError: (state) => {
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchZoomLevels.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(fetchZoomLevels.fulfilled, (state, action: PayloadAction<ILevelDTO[]>) => {
        state.isLoading = false;
        state.levels = action.payload;
      })
      .addCase(fetchZoomLevels.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.error.message || 'Unknown error';
      });
  },
});

// Export actions and reducer
export const { clearError } = zoomSlice.actions;
export const reducer = zoomSlice.reducer;
