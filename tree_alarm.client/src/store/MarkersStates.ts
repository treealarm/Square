import { createSlice, PayloadAction, createAsyncThunk } from '@reduxjs/toolkit';
import { RootState } from './store';
import { ApiRootString } from './constants';
import { DoFetch } from './Fetcher';
import { BoundBox, ICommonFig, IFigures } from './Marker';

// -----------------
// STATE - This defines the type of data maintained in the Redux store.

export interface MarkersState {
  isLoading: boolean;
  markers: IFigures | null;
  box: BoundBox | null;
  isChanging?: number;
  initiateUpdateAll: number;
  selected_marker: ICommonFig | null;
}

const initialState: MarkersState = {
  markers: null,
  isLoading: false,
  box: null,
  isChanging: 0,
  initiateUpdateAll: 0,
  selected_marker: null
};

interface ReceiveMarkersAction {
  box: BoundBox;
  markers: IFigures;
}

// -----------------
// Async Thunks

export const fetchMarkersByBox = createAsyncThunk<ReceiveMarkersAction, BoundBox|null>(
  'markers/fetchMarkers',
  async (box: BoundBox|null) => {
    const body = JSON.stringify(box);
    const response = await DoFetch(`${ApiRootString}/GetByBox`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: body,
    });

    if (!response.ok) {
      throw new Error('Failed to fetch markers');
    }

    const data = (await response.json()) as IFigures;
    return { box:box, markers: data };
  }
);


export const updateMarkers = createAsyncThunk(
  'markers/updateMarkers',
  async (markers: IFigures, { rejectWithValue }) => {
    try {
      markers.add_tracks = true;
      const body = JSON.stringify(markers);
      const response = await DoFetch(`${ApiRootString}/UpdateFigures`, {
        method: 'POST',
        headers: { 'Content-type': 'application/json' },
        body: body,
      });

      const data = (await response.json()) as IFigures;
      return data;
    } catch (error) {
      return rejectWithValue(null);
    }
  }
);

export const deleteMarkers = createAsyncThunk(
  'markers/deleteMarkers',
  async (ids: string[], { rejectWithValue }) => {
    try {
      const body = JSON.stringify(ids);
      const response = await DoFetch(`${ApiRootString}`, {
        method: 'DELETE',
        headers: { 'Content-type': 'application/json' },
        body: body,
      });

      const deleted_ids = (await response.json()) as string[];
      return deleted_ids;
    } catch (error) {
      return rejectWithValue(null);
    }
  }
);

export const requestMarkersByIds = 
  async (ids: string[]): Promise<IFigures> => {
  const body = JSON.stringify(ids);
  const response = await DoFetch(`${ApiRootString}/GetByIds`, {
    method: 'POST',
    headers: { 'Content-type': 'application/json' },
    body: body,
  });

  if (!response.ok) {
    throw new Error(response.statusText);
  }

  const data = (await response.json()) as IFigures;
  return data;
}


export const fetchMarkersByIds = createAsyncThunk(
  'markers/fetchMarkersByIds',
  async (ids: string[], { rejectWithValue }) => {
    try {
      return requestMarkersByIds(ids);
    } catch (error) {
      console.log("fetchMarkersByIds:", error);
      return rejectWithValue(null);
    }
  }
);

function deepEqual(obj1: any, obj2: any): boolean {
  if (obj1 === obj2) return true; // Check for strict equality

  if (obj1 == null || obj2 == null) return false; // Handle null and undefined

  if (typeof obj1 !== 'object' || typeof obj2 !== 'object') return false; // Check if both are objects

  const keys1 = Object.keys(obj1);
  const keys2 = Object.keys(obj2);

  if (keys1.length !== keys2.length) return false; // Different number of keys

  for (const key of keys1) {
    if (!keys2.includes(key) || !deepEqual(obj1[key], obj2[key])) {
      return false; // Key is missing or values are not deeply equal
    }
  }

  return true;
}


// -----------------
// Slice

const markersSlice = createSlice({
  name: 'markers',
  initialState,
  reducers: {
    initiateUpdateAll: (state) => {
      state.initiateUpdateAll += 1;
    },
    deleteMarkersLocally: (state, action: PayloadAction<string[]>) => {
      state.markers = {
        figs: state.markers?.figs?.filter(marker => !action.payload.includes(marker.id ?? '')) || [],
      };
    },
    selectMarkerLocally: (state, action: PayloadAction<ICommonFig | null>) => {
      state.selected_marker = action.payload;

      if (state.selected_marker == null) {
        console.log("selected_marker is null");
      }
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchMarkersByBox.pending, (state, action) => {
        state.isLoading = true;
        state.box = action.meta.arg;
      })
      .addCase(fetchMarkersByBox.fulfilled, (state, action) => {
        if (deepEqual(action.payload.box,state.box)) {
          state.markers = action.payload.markers;
        }
        state.isLoading = false;
      })
      .addCase(fetchMarkersByBox.rejected, (state) => {
        state.isLoading = false;
      })
      .addCase(updateMarkers.pending, (state) => {
        state.isLoading = true;
      })
      .addCase(updateMarkers.fulfilled, (state, action) => {
        state.markers = {
          figs: [
            ...state.markers?.figs.filter(item => !action.payload.figs.some(newItem => newItem.id === item.id)) || [],
            ...action.payload.figs,
          ],
        };
        state.isLoading = false;
        state.isChanging! += 1;
      })
      .addCase(deleteMarkers.pending, (state) => {
        state.isLoading = true;
      })
      .addCase(fetchMarkersByIds.fulfilled, (state, action) => {
        const newMarkers = action.payload.figs;

        // Проверяем, что state.markers и state.markers.figs существуют
        if (state.markers && state.markers.figs) {
          newMarkers.forEach(newMarker => {
            const existingIndex = state.markers.figs.findIndex(marker => marker.id === newMarker.id);

            if (existingIndex !== -1) {
              // Если маркер существует, обновляем его
              state.markers.figs[existingIndex] = newMarker;
            } else {
              // Если маркер не существует, добавляем его
              state.markers.figs.push(newMarker);
            }
          });

          // Увеличиваем значение isChanging
          state.isChanging = (state.isChanging || 0) + 1;
        }
      });


  },
});

// -----------------
// Action creators and selectors

export const { initiateUpdateAll, deleteMarkersLocally, selectMarkerLocally } = markersSlice.actions;

export const selectMarkers = (state: RootState) => state.markers.markers;
export const selectIsLoading = (state: RootState) => state.markers.isLoading;
export const selectIsChanging = (state: RootState) => state.markers.isChanging;

export const reducer = markersSlice.reducer;
