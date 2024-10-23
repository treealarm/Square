import { IObjectRightsDTO, IRightValuesDTO } from './Marker';
import { DoFetch } from './Fetcher';
import { createAsyncThunk, createSlice, PayloadAction } from '@reduxjs/toolkit'
import { ApiRightsRootString } from './constants';


export interface ObjectRights {
  rights: IObjectRightsDTO[];
  updated: boolean;
  user: string;
  all_roles: string[];
  all_rights: IRightValuesDTO[]
}

const unloadedState: ObjectRights = {
  rights: null,
  updated: false,
  user: null,
  all_roles: [],
  all_rights: []
};

export const updateRights = createAsyncThunk<IObjectRightsDTO[], IObjectRightsDTO[]>(
  'rights/UpdateRights',
  async (object_ids: IObjectRightsDTO[], thunkAPI) => {

    let body = JSON.stringify(object_ids);
    var fetched = await DoFetch(ApiRightsRootString + "/UpdateRights", {
      method: "POST",
      headers: { "Content-type": "application/json" },
      body: body
    });

    var json = await fetched.json() as Promise<IObjectRightsDTO[]>;

    return json;
  }
)

export const fetchRightsByIds = createAsyncThunk<IObjectRightsDTO[], string[]>(
  'rights/GetRights',
  async (object_ids: string[], thunkAPI) => {

    let body = JSON.stringify(object_ids);
    var fetched = await DoFetch(ApiRightsRootString + "/GetRights", {
      method: "POST",
      headers: { "Content-type": "application/json" },
      body: body
    });

    var json = await fetched.json() as Promise<IObjectRightsDTO[]>;

    return json;
  }
)

export const fetchAllRightValues = createAsyncThunk<IRightValuesDTO[]>(
  'rights/GetRightValues',
  async (thunkAPI) => {
    var fetched = await DoFetch(ApiRightsRootString + "/GetRightValues", {
      method: "GET",
      headers: { "Content-type": "application/json" }
    });

    var json = await fetched.json() as Promise<IRightValuesDTO[]>;

    return json;
  }
)

export const fetchAllRoles = createAsyncThunk<string[]>(
  'rights/GetRoles',
  async (thunkAPI) => {
    var fetched = await DoFetch(ApiRightsRootString + "/GetRoles", {
      method: "GET",
      headers: { "Content-type": "application/json" }
    });

    var json = await fetched.json() as Promise<string[]>;

    return json;
  }
)

const rightsSlice = createSlice({
  name: 'RightsStates',
  initialState: unloadedState,
  reducers: {
    // Give case reducers meaningful past-tense "event"-style names
    set_rights(state: ObjectRights, action: PayloadAction<IObjectRightsDTO[]>) {
      state.rights = action.payload;
    }
    ,
    set_user(state: ObjectRights, action: PayloadAction<string>) {
      state.user = action.payload??null;
    }
  }
  ,
  extraReducers: (builder) => {
    builder
      .addCase(fetchRightsByIds.pending, (state, action) => {
        state.rights = null;
      })
      .addCase(fetchRightsByIds.fulfilled, (state, action) => {
        const { requestId } = action.meta
        state.rights = action.payload;
      })
      .addCase(fetchRightsByIds.rejected, (state, action) => {
        const { requestId } = action.meta
        state.rights = null;
      })
      // AllRights.
      .addCase(fetchAllRightValues.fulfilled, (state, action) => {
        state.all_rights = action.payload;
      })
      // AllRoles.
      .addCase(fetchAllRoles.fulfilled, (state, action) => {
        state.all_roles = action.payload;
      })
      // updateRights
      .addCase(updateRights.fulfilled, (state, action) => {
        const { requestId } = action.meta
        state.rights = action.payload;
      })
  },
})

export const { set_rights, set_user } = rightsSlice.actions
export const reducer = rightsSlice.reducer



