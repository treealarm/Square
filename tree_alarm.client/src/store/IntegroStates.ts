import { createSlice, createAsyncThunk, PayloadAction } from '@reduxjs/toolkit';
import { DoFetch } from './Fetcher';
import { IActionDescrDTO, IActionExeDTO, IIntegroTypeDTO, Marker, IUpdateIntegroObjectDTO, IActionExeInfoDTO, IActionExeInfoRequestDTO } from './Marker';
import { ApplicationState } from '.';
import { ApiIntegroRootString } from './constants';

export interface IntegroState {
  actions: IActionDescrDTO[];
  integroType: IIntegroTypeDTO|null;
  loading: boolean;
  error: string | null;
  actionsByObject: IActionExeInfoDTO[];
  snapshots: Record<string, string>; // object_id -> base64 string
}

const initialState: IntegroState = {
  actions: [],
  integroType: null,
  loading: false,
  error: null,
  actionsByObject: [],
  snapshots: {},
};

export const fetchSnapshot = createAsyncThunk<
  { object_id: string; dataUrl: string },
  string
>(
  'actions/fetchSnapshot',
  async (object_id: string, { rejectWithValue }) => {
    try {
      const response = await DoFetch(ApiIntegroRootString + `/GetSnapshot?object_id=${object_id}`, {
        method: 'GET',
      });

      if (!response.ok) {
        const errorText = await response.text();
        return rejectWithValue(errorText);
      }

      const blob = await response.blob();
      const reader = new FileReader();

      const dataUrl = await new Promise<string>((resolve, reject) => {
        reader.onloadend = () => resolve(reader.result as string);
        reader.onerror = reject;
        reader.readAsDataURL(blob);
      });

      return { object_id, dataUrl };
    } catch (error: any) {
      return rejectWithValue(error.message || 'Error fetching snapshot');
    }
  }
);

export  async function fetchActionsByObjectIdRaw(requestDto: IActionExeInfoRequestDTO): Promise<IActionExeInfoDTO[]> {
  const response = await DoFetch(ApiIntegroRootString + '/GetActionsByObjectId', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(requestDto),
  });

  if (!response.ok) {
    const errorText = await response.text();
    throw new Error(errorText);
  }

  return await response.json() as IActionExeInfoDTO[];
}


export const fetchActionsByObjectId = createAsyncThunk<
  IActionExeInfoDTO[],
  IActionExeInfoRequestDTO>(
  'actions/fetchActionsByObjectId',
  async (requestDto: IActionExeInfoRequestDTO, { rejectWithValue }) => {
    try {
      const result = await fetchActionsByObjectIdRaw(requestDto);
      return result;
    } catch (error: any) {
      return rejectWithValue(error.message || 'Unknown error');
    }
  }
);


export const cancelAction = createAsyncThunk<boolean, string>(
  'actions/cancelAction',
  async (executionId: string, { rejectWithValue }) => {
    try {
      const response = await DoFetch(ApiIntegroRootString + '/CancelAction?uid=' + executionId, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
      });

      if (!response.ok) {
        const errorText = await response.text();
        return rejectWithValue(errorText);
      }

      return true;
    } catch (error: any) {
      return rejectWithValue(error.message);
    }
  }
);

export async function fetchAvailableActionsRaw(id: string): Promise<IActionDescrDTO[]> {
  const response = await DoFetch(ApiIntegroRootString + '/GetAvailableActions?id=' + id, {
    method: 'GET',
    headers: { 'Content-Type': 'application/json' },
  });

  if (!response.ok) {
    const errorText = await response.text();
    throw new Error(errorText);
  }

  return await response.json() as IActionDescrDTO[];
}

export const fetchAvailableActions = createAsyncThunk<
  IActionDescrDTO[],
  string
>(
  'actions/fetchAvailableActions',
  async (id: string, { rejectWithValue }) => {
    try {
      const result = await fetchAvailableActionsRaw(id);
      return result;
    } catch (error: any) {
      return rejectWithValue(error.message || 'Error fetching available actions');
    }
  }
);


export const fetchObjectIntegroType = createAsyncThunk<IIntegroTypeDTO | null, string>(
  'actions/fetchObjectIntegroType',
  async (id: string, { rejectWithValue }) => {
    const response = await DoFetch(ApiIntegroRootString + '/GetObjectIntegroType?id=' + id, {
      method: 'GET',
      headers: { 'Content-Type': 'application/json' },
    });

    if (response.status === 404) {
      return null; // норм, ничего не найдено
    }

    if (!response.ok) {
      return rejectWithValue(`Ошибка ${response.status}`);
    }

    const json = (await response.json()) as IIntegroTypeDTO;
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

export const updateIntegroObject = createAsyncThunk<Marker, IUpdateIntegroObjectDTO>(
  'actions/updateIntegroObject',
  async (dto: IUpdateIntegroObjectDTO, { rejectWithValue }) => {
    try {
      const response = await DoFetch(ApiIntegroRootString + '/UpdateIntegroObject', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(dto),
      });

      if (!response.ok) {
        const errorText = await response.text();
        return rejectWithValue(errorText);
      }

      const json = (await response.json()) as Marker;
      return json;
    } catch (error: any) {
      return rejectWithValue(error.message);
    }
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
        state.integroType = null;
        state.error = action.error.message || 'Error fetching child types';
      })
      ////Create total integro with object
      .addCase(updateIntegroObject.pending, (state) => {
        state.error = null;
      })
      .addCase(updateIntegroObject.fulfilled, (state, action) => {
      })
      .addCase(updateIntegroObject.rejected, (state, action) => {
        state.error = action.payload as string;
      })
      //fetchActionsByObjectId
      .addCase(fetchActionsByObjectId.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchActionsByObjectId.fulfilled, (state, action) => {
        state.loading = false;
        state.actionsByObject = action.payload;
      })
      .addCase(fetchActionsByObjectId.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string || 'Error fetching actions by object ID';
      })
      //snapshots
      .addCase(fetchSnapshot.fulfilled, (state, action) => {
        const { object_id, dataUrl } = action.payload;
        state.snapshots[object_id] = dataUrl;
      })
      .addCase(fetchSnapshot.rejected, (state, action) => {
        state.error = action.payload as string || 'Error fetching snapshot';
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
