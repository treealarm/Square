import { createSlice, createAsyncThunk, PayloadAction } from '@reduxjs/toolkit';
import { DoFetch } from './Fetcher';
import { IValueDTO } from './Marker';

export interface ValuesState {
  values: IValueDTO[];
  update_values_periodically : boolean;
  loading: boolean;
  error: string | null;
}

const initialState: ValuesState = {
  values: [],
  update_values_periodically: true,
  loading: false,
  error: null,
};

export const fetchValuesByOwners = createAsyncThunk<IValueDTO[], string[]>(
  'values/fetchByOwners',
  async (owners: string[]) => {
    const fetched = await DoFetch(`/api/Values/GetByOwners`, {
      method: 'POST',
      headers: { 'Content-type': 'application/json' },
      body: JSON.stringify(owners),
    });
    const json = (await fetched.json()) as Promise<IValueDTO[]>;
    return json;
  }
);

export const fetchValuesByIds = createAsyncThunk<IValueDTO[], string[]>(
  'values/fetchByIds',
  async (ids: string[]) => {
    const fetched = await DoFetch(`/api/Values/GetByIds`, {
      method: 'POST',
      headers: { 'Content-type': 'application/json' },
      body: JSON.stringify(ids),
    });
    const json = (await fetched.json()) as Promise<IValueDTO[]>;
    return json;
  }
);

export const updateValues = createAsyncThunk<void, IValueDTO[]>(
  'values/updateValues',
  async (values: IValueDTO[]) => {
    await DoFetch('/api/Values/UpdateValues', {
      method: 'POST',
      headers: { 'Content-type': 'application/json' },
      body: JSON.stringify(values),
    });
  }
);

export const deleteValues = createAsyncThunk<string[], string[]>(
  'values/deleteValues',
  async (ids: string[]) => {
    await DoFetch('/api/Values/DeleteValues', {
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
        state.values = action.payload;
      })
      .addCase(fetchValuesByOwners.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message || 'Error fetching values by owner';
      })
      ///fetchValuesByIds
      .addCase(fetchValuesByIds.pending, (state) => {
        state.loading = true;
      })
      .addCase(fetchValuesByIds.fulfilled, (state, action) => {
        state.loading = false;

        // Создаем Map из пришедших значений
        const incomingValuesMap = new Map(action.payload.map((v) => [v.id, v]));

        // Обновляем значения в state, заменяя по id, но не трогая owner_id
        state.values = state.values
          .map((existingValue) => {
            if (incomingValuesMap.has(existingValue.id)) {
              const incomingValue = incomingValuesMap.get(existingValue.id)!;

              // Создаем новый объект с обновленными значениями, но оставляем старый owner_id
              return {
                ...existingValue,
                value: incomingValue.value,
                min: incomingValue.min,
                max: incomingValue.max,
                name: incomingValue.name,
              };
            }
            return existingValue; // Если id не совпадает, оставляем без изменений
          })
          .concat(
            action.payload.filter(
              (newValue) => !state.values.some((v) => v.id === newValue.id)
            )
          );
      })

      .addCase(fetchValuesByIds.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message || 'Error fetching values by ID';
      })
      .addCase(updateValues.fulfilled, (state, action) => {
        state.values = state.values.map((v) =>
          action.meta.arg.find((updated) => updated.id === v.id) || v
        );
      })
      .addCase(deleteValues.fulfilled, (state, action) => {
        state.values = state.values.filter((v) => !action.payload.includes(v.id));
      });
  },
});

export const {
  set_update_values,
} = valuesSlice.actions

export const reducer = valuesSlice.reducer
