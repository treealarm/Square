import { createSlice, PayloadAction, createAsyncThunk } from '@reduxjs/toolkit';
import { RootState } from './store';
import { DoFetch } from './Fetcher';
import { ApiTracksRootString } from './constants';
import { BoxTrackDTO, IRoutLineDTO, ITrackPointDTO } from './Marker';
import { theStore } from './configureStore';

export interface TracksState {
  tracks: ITrackPointDTO[];
  box: BoxTrackDTO | null;
  selected_track: ITrackPointDTO | null;
}

const initialState: TracksState = {
  tracks: [],
  box: null,
  selected_track: null,
};

// Async thunks

export const fetchTracksByBox = createAsyncThunk(
  'tracks/fetchTracksByBox',
  async (box: BoxTrackDTO, { rejectWithValue }) => {
    try {
      const response = await DoFetch(`${ApiTracksRootString}/GetTracksByBox`, {
        method: 'POST',
        headers: { 'Content-type': 'application/json' },
        body: JSON.stringify(box),
      });

      if (!response.ok) {
        throw new Error(response.statusText);
      }

      const data = (await response.json()) as ITrackPointDTO[];
      return { box, tracks: data };
    } catch (error) {
      return rejectWithValue([]);
    }
  }
);

// Slice
const tracksSlice = createSlice({
  name: 'TracksState',
  initialState,
  reducers: {
    selectTrack: (state, action: PayloadAction<ITrackPointDTO|null>) => {
      state.selected_track = action.payload;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchTracksByBox.fulfilled, (state, action) => {
        state.box = action.payload.box;
        state.tracks = action.payload.tracks;
      })
      .addCase(fetchTracksByBox.rejected, (state) => {
        state.tracks = [];
      });
  },
});

export const { selectTrack } = tracksSlice.actions;

// Selectors
export const selectRoutes = (state: RootState) => state.tracks.routes;
export const selectTracks = (state: RootState) => state.tracks.tracks;
export const selectSelectedTrack = (state: RootState) => state.tracks.selected_track;

export const reducer = tracksSlice.reducer;

export const OnSelectTrack = (selected_track: ITrackPointDTO | null) =>
  (dispatch: typeof theStore.dispatch) => {
    dispatch(selectTrack( selected_track ));
  };