import { createSlice, createAsyncThunk, PayloadAction } from '@reduxjs/toolkit';
import { DoFetch } from './Fetcher';
import { IActionDescrDTO, IActionExeDTO, IValueDTO } from './Marker';
import { ApplicationState } from '.';
import { ApiIntegroRootString } from './constants';

export interface ActionsState {
  actions: IActionDescrDTO[];
  loading: boolean;
  error: string | null;
}

const initialState: ActionsState = {
  actions: [],
  loading: false,
  error: null,
};

export const fetchAvailableActions = createAsyncThunk<IActionDescrDTO[], string>(
  'actions/fetchAvailableActions',
  async (id: string) => {
    const fetched = await DoFetch(ApiIntegroRootString+'/GetAvailableActions?id='+id, {
      method: 'GET',
      headers: { 'Content-type': 'application/json' }
    });
    const json = (await fetched.json()) as Promise<IActionDescrDTO[]>;
    return json;
  }
);

export const executeAction = createAsyncThunk<boolean, IActionExeDTO>(
  'actions/executeAction',
  async (selectedAction: IActionExeDTO) => {

    const fetched = await DoFetch(ApiIntegroRootString+'/ExecuteActions', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify([selectedAction]),
    });
    const json = (await fetched.json()) as Promise<boolean>;
    return json;
  }
);

// Слайс
const valuesSlice = createSlice({
  name: 'values',
  initialState,
  reducers: {
    set_actions(state: ActionsState, action: PayloadAction<IActionDescrDTO[]>) {
      state.actions = action.payload;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchAvailableActions.pending, (state) => {
        state.loading = true;
      })
      .addCase(fetchAvailableActions.fulfilled, (state, action) => {
        state.loading = false;
       
        state.actions = action.payload;
      })
      .addCase(fetchAvailableActions.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message || 'Error fetching available actions';
      })
      //execute
      .addCase(executeAction.pending, (state, action) => {
        state.loading = false;
      })
      .addCase(executeAction.fulfilled, (state, action) => {
        state.loading = false;
      })
      .addCase(executeAction.rejected, (state, action) => {
        state.error = action.error.message || 'Error executing action';
      })
    ;
  },
});

export const selectActionsMapForOwner = (ownerId: string) => (state: ApplicationState) =>
  state?.valuesStates?.valuesMap?.[ownerId] ?? null;

export const {
  set_actions,
} = valuesSlice.actions;

export const reducer = valuesSlice.reducer;
