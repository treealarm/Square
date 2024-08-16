import { createAsyncThunk, createSlice, PayloadAction } from '@reduxjs/toolkit'

import { IGetIntegrationLeafsDTO, IGetIntegrationsDTO, IIntegrationLeafDTO } from './Marker';
import { DoFetch } from './Fetcher';
import { ApiIntegrationsRootString, ApiIntegrationLeafsRootString } from './constants';


export interface IntegrationStates {
  integrations: IGetIntegrationsDTO|null;
  isFetching: boolean;
  selected_integration: string | null;
  integration_leafs: IGetIntegrationLeafsDTO | null;
}

const unloadedState: IntegrationStates = {
  integrations: null,
  isFetching: false,
  selected_integration: null,
  integration_leafs: null
};

export const fetchIntegrationsByParent = createAsyncThunk<IGetIntegrationsDTO, string|null>(
  'integrations/GetByParent',
  async (parent_id: string|null, { getState }) => {
    let body = JSON.stringify(parent_id);

    var fetched = await DoFetch(ApiIntegrationsRootString + "/GetByParent?parent_id=" + parent_id,
      {
        method: "GET"
      });

    var json = await fetched.json() as Promise<IGetIntegrationsDTO>;

    return json;
  }
)

export const fetchIntegrationLeafsByParent = createAsyncThunk<IGetIntegrationLeafsDTO, string>(
  'integrationleafs/GetByParent',
  async (parent_id: string, { getState }) => {
    let body = JSON.stringify(parent_id);

    var fetched = await DoFetch(ApiIntegrationLeafsRootString + "/GetByParent?parent_id=" + parent_id,
      {
        method: "GET"
      });

    var json = await fetched.json() as Promise<IGetIntegrationLeafsDTO>;

    return json;
  }
)

export const addIntegrationLeaf = createAsyncThunk<IIntegrationLeafDTO[], string>(
  'integrationleafs/Update',
  async (parent_id: string, { getState }) => {

    var newObjects: IIntegrationLeafDTO[] =
      [
        {
          id: null,
          parent_id: parent_id
        }
      ];
    let body = JSON.stringify(newObjects);

    var fetched = await DoFetch(ApiIntegrationLeafsRootString + "/Update",
      {
        method: "POST",
        headers: { "Content-type": "application/json" },
        body: body
      });

    var json = await fetched.json() as Promise<IIntegrationLeafDTO[]>;

    return json;
  }
)

export const deleteIntegrationLeaf = createAsyncThunk<any, string>(
  'integrationleafs/Delete',
  async (parent_id: string, { getState }) => {
    let body = JSON.stringify([parent_id]);

    var fetched = await DoFetch(ApiIntegrationLeafsRootString + "/Delete",
      {
        method: "DELETE",
        headers: { "Content-type": "application/json" },
        body: body
      });

    return fetched;
  }
)

const integrationsSlice = createSlice({
  name: 'IntegrationsStates',
  initialState: unloadedState,
  reducers: {
    set_selected_integration(state: IntegrationStates, action: PayloadAction<string | null>) {
      state.selected_integration = action.payload;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchIntegrationsByParent.pending, (state: IntegrationStates, action) => {
        state.isFetching = true;
      })
      .addCase(fetchIntegrationsByParent.fulfilled, (state: IntegrationStates, action) => {
        state.isFetching = false;
        const { requestId } = action.meta
        state.integrations = action.payload;
      })
      .addCase(fetchIntegrationsByParent.rejected, (state: IntegrationStates, action) => {
        state.isFetching = false;
        const { requestId } = action.meta
        state.integrations = null;
      })
    //fetchIntegrationLeafsByParent
      .addCase(fetchIntegrationLeafsByParent.pending, (state: IntegrationStates, action) => {
      })
      .addCase(fetchIntegrationLeafsByParent.fulfilled, (state: IntegrationStates, action) => {
        state.isFetching = false;
        const { requestId } = action.meta
        state.integration_leafs = action.payload;
      })
      .addCase(fetchIntegrationLeafsByParent.rejected, (state: IntegrationStates, action) => {
        state.isFetching = false;
        const { requestId } = action.meta
        state.integration_leafs = null;
      })
  },
})

export const { set_selected_integration } = integrationsSlice.actions
export const reducer = integrationsSlice.reducer



