import { createSlice, createAsyncThunk, PayloadAction } from '@reduxjs/toolkit';
import { DoFetch } from './Fetcher';
import { IActionDescrDTO, IActionExeDTO, IIntegroTypeDTO } from './Marker';
import { ApplicationState } from '.';
import { ApiIntegroRootString } from './constants';

export interface IntegroState {
  actions: IActionDescrDTO[];
  integroType: IIntegroTypeDTO|null;
  loading: boolean;
  error: string | null;
}

const initialState: IntegroState = {
  actions: [],
  integroType: null,
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

export const fetchObjectIntegroType = createAsyncThunk<IIntegroTypeDTO, string>(
  'actions/fetchObjectIntegroType',
  async (id: string) => {
    const fetched = await DoFetch(ApiIntegroRootString + '/GetObjectIntegroType?id=' + id, {
      method: 'GET',
      headers: { 'Content-Type': 'application/json' },
    });
    const json = (await fetched.json()) as Promise<IIntegroTypeDTO>;
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
    set_actions(state: IntegroState, action: PayloadAction<IActionDescrDTO[]>) {
      state.actions = action.payload;
    },
    set_objectIntegroType(state: IntegroState, action: PayloadAction<IIntegroTypeDTO|null>) {
      state.integroType = action.payload;
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
      // childtypes
      .addCase(fetchObjectIntegroType.pending, (state) => {
        state.loading = true;
      })
      .addCase(fetchObjectIntegroType.fulfilled, (state, action) => {
        state.loading = false;
        state.integroType = action.payload;
      })
      .addCase(fetchObjectIntegroType.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message || 'Error fetching child types';
      })

    ;
  },
});

export const selectActionsMapForOwner = (ownerId: string) => (state: ApplicationState) =>
  state?.valuesStates?.valuesMap?.[ownerId] ?? null;

export const {
  set_actions,
  set_objectIntegroType
} = valuesSlice.actions;

export const reducer = valuesSlice.reducer;
