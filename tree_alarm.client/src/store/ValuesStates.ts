import { createSlice, createAsyncThunk, PayloadAction } from '@reduxjs/toolkit';
import { DoFetch } from './Fetcher';
import { IValueDTO } from './Marker';
import { ApplicationState } from '.';
import { ApiValuesRootString } from './constants';

export interface ValuesState {
  valuesMap: Record<string, IValueDTO[]>;
  update_values_periodically: boolean;
  loading: boolean;
  error: string | null;
}

const initialState: ValuesState = {
  valuesMap: {},
  update_values_periodically: false,
  loading: false,
  error: null,
};

export const fetchValuesByOwners = createAsyncThunk<IValueDTO[], string[]>(
  'values/fetchByOwners',
  async (owners: string[]) => {
    const fetched = await DoFetch(ApiValuesRootString+'/GetByOwners', {
      method: 'POST',
      headers: { 'Content-type': 'application/json' },
      body: JSON.stringify(owners),
    });
    const json = (await fetched.json()) as Promise<IValueDTO[]>;
    return json;
  }
);

export const updateValues = createAsyncThunk<void, IValueDTO[]>(
  'values/updateValues',
  async (values: IValueDTO[]) => {
    await DoFetch(ApiValuesRootString+'/UpdateValues', {
      method: 'POST',
      headers: { 'Content-type': 'application/json' },
      body: JSON.stringify(values),
    });
  }
);

export const deleteValues = createAsyncThunk<string[], string[]>(
  'values/deleteValues',
  async (ids: string[]) => {
    await DoFetch(ApiValuesRootString+'/DeleteValues', {
      method: 'DELETE',
      headers: { 'Content-type': 'application/json' },
      body: JSON.stringify(ids),
    });
    return ids;
  }
);

// Слайс
const valuesSlice = createSlice({
  name: 'values',
  initialState,
  reducers: {
    set_update_values(state: ValuesState, action: PayloadAction<boolean>) {
      state.update_values_periodically = action.payload;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchValuesByOwners.pending, (state) => {
        state.loading = true;
      })
      .addCase(fetchValuesByOwners.fulfilled, (state, action) => {
        state.loading = false;
        const newValuesMap: Record<string, IValueDTO[]> = {};
        action.payload.forEach(value => {
          if (!newValuesMap[value.owner_id]) {
            newValuesMap[value.owner_id] = [];
          }
          newValuesMap[value.owner_id].push(value);
        });
        state.valuesMap = newValuesMap;
      })
      .addCase(fetchValuesByOwners.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message || 'Error fetching values by owner';
      })
      // update values
      .addCase(updateValues.fulfilled, (state, action) => {
        action.meta.arg.forEach(updatedValue => {
          if (state.valuesMap[updatedValue.owner_id]) {
            const ownerValues = state.valuesMap[updatedValue.owner_id];
            state.valuesMap[updatedValue.owner_id] = ownerValues.map(v =>
              v.id === updatedValue.id ? { ...v, ...updatedValue } : v
            );
          }
        });
      })
      .addCase(deleteValues.fulfilled, (state, action) => {
        action.payload.forEach(id => {
          Object.keys(state.valuesMap).forEach(ownerId => {
            state.valuesMap[ownerId] = state.valuesMap[ownerId].filter(value => value.id !== id);
          });
        });
      });
  },
});

export const selectValuesMapForOwner = (ownerId: string) => (state: ApplicationState) =>
  state?.valuesStates?.valuesMap?.[ownerId] ?? null;

export const {
  set_update_values,
} = valuesSlice.actions;

export const reducer = valuesSlice.reducer;
