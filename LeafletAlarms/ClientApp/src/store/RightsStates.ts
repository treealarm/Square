import { ApiRightsRootString} from '.';
import { IObjectRightsDTO } from './Marker';
import { DoFetch } from './Fetcher';
import { createAsyncThunk, createSlice, PayloadAction } from '@reduxjs/toolkit'


export interface ObjectRights {
  rights: IObjectRightsDTO[];
  updated: boolean;
}

const unloadedState: ObjectRights = {
  rights: null,
  updated: false
};

export const fetchRightsByIds = createAsyncThunk<IObjectRightsDTO[], any>(
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

const rightsSlice = createSlice({
  name: 'RightsStates',
  initialState: unloadedState,
  reducers: {
    // Give case reducers meaningful past-tense "event"-style names
    set_rights(state: any, action: PayloadAction<IObjectRightsDTO[]>) {
      state.rights = action.payload;
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
  },
})

export const { set_rights } = rightsSlice.actions
export const reducer = rightsSlice.reducer



