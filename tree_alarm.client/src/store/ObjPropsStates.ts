import { createSlice, PayloadAction, createAsyncThunk } from '@reduxjs/toolkit';
import { RootState } from './store';
import { ApiRootString } from './constants';
import { DoFetch } from './Fetcher';
import { IObjProps } from './Marker';

// -----------------
// STATE - This defines the type of data maintained in the Redux store.

export interface ObjPropsState {
  objProps: IObjProps | null;
  updated: number;
}

const initialState: ObjPropsState = {
  objProps: null,
  updated: 0,
};

// -----------------
// Async thunks

export const fetchObjProps = createAsyncThunk(
  'objProps/fetchObjProps',
  async (object_id: string, { rejectWithValue }) => {
    try {
      if (!object_id) {
        return null;
      }

      const response = await DoFetch(`${ApiRootString}/GetObjProps?id=${object_id}`);

      if (!response.ok) {
        throw new Error(response.statusText);
      }

      const data = (await response.json()) as IObjProps;
      return data;
    } catch (error) {
      return rejectWithValue(null);
    }
  }
);

export const updateObjProps = createAsyncThunk(
  'objProps/updateObjProps',
  async (marker: IObjProps, { rejectWithValue }) => {
    try {
      const response = await DoFetch(`${ApiRootString}/UpdateProperties`, {
        method: 'POST',
        headers: { 'Content-type': 'application/json' },
        body: JSON.stringify(marker),
      });

      const data = (await response.json()) as IObjProps;
      return data;
    } catch (error) {
      return rejectWithValue(null);
    }
  }
);

// -----------------
// Slice

const objPropsSlice = createSlice({
  name: 'objProps',
  initialState,
  reducers: {
    setObjPropsLocally: (state, action: PayloadAction<IObjProps>) => {
      state.objProps = action.payload;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchObjProps.fulfilled, (state, action) => {
        state.objProps = action.payload;
      })
      .addCase(updateObjProps.fulfilled, (state, action) => {
        state.objProps = action.payload;
        state.updated += 1;
      })
      .addCase(fetchObjProps.rejected, (state) => {
        state.objProps = null;
      })
      .addCase(updateObjProps.rejected, (state) => {
        // Handle rejected updates if necessary
      });
  },
});

// -----------------
// Action creators and selectors

export const { setObjPropsLocally } = objPropsSlice.actions;

export const selectObjProps = (state: RootState) => state.objProps.objProps;
export const selectUpdatedCount = (state: RootState) => state.objProps.updated;

export const reducer = objPropsSlice.reducer;
