import { createSlice, PayloadAction, createAsyncThunk } from '@reduxjs/toolkit';
import { RootState } from './store';
import { ApiTracksRootString } from './constants';
import { DoFetch } from './Fetcher';
import { GetTracksBySearchDTO, ITrackPointDTO, SearchFilterDTO } from './Marker';

// -----------------
// STATE - This defines the type of data maintained in the Redux store.

export interface SearchResultState {
  list: ITrackPointDTO[];
  filter: SearchFilterDTO | null;
}

const initialState: SearchResultState = {
  list: [],
  filter: null,
};

// -----------------
// Async thunks

export const fetchTracksByFilter = createAsyncThunk(
  'search/fetchTracksByFilter',
  async (filter: SearchFilterDTO, { rejectWithValue }) => {
    try {
      const response = await DoFetch(`${ApiTracksRootString}/GetByFilter`, {
        method: 'POST',
        headers: { 'Content-type': 'application/json' },
        body: JSON.stringify(filter),
      });

      const data = (await response.json()) as GetTracksBySearchDTO;

      return data;
    } catch (error) {
      return rejectWithValue(null);
    }
  }
);

// -----------------
// Slice

const searchSlice = createSlice({
  name: 'search',
  initialState,
  reducers: {
    setEmptyResult: (state) => {
      state.list = [];
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchTracksByFilter.pending, (state, action) => {
        state.filter = action.meta.arg;
      })
      .addCase(fetchTracksByFilter.fulfilled, (state, action) => {
        if (
          state.filter == null ||
          action.payload?.search_id === state.filter.search_id ||
          action.payload?.search_id === ''
        ) {
          state.list = action.payload ? action.payload.list : [];
        }
      })
      .addCase(fetchTracksByFilter.rejected, (state) => {
        state.list = [];
      });
  },
});

// -----------------
// Action creators and selectors

export const { setEmptyResult } = searchSlice.actions;

export const selectSearchResults = (state: RootState) => state.search.list;
export const selectSearchFilter = (state: RootState) => state.search.filter;

export const reducer = searchSlice.reducer;
