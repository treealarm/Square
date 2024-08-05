import { createAsyncThunk, createSlice, PayloadAction } from '@reduxjs/toolkit'

import { IGetIntegrationsDTO, IIntegrationDTO } from './Marker';
import { DoFetch } from './Fetcher';
import { ApiDefaultPagingNum, ApiIntegrationsRootString } from './constants';


export interface IntegrationStates {
  integrations: IGetIntegrationsDTO|null;
  isFetching: boolean;
}

const unloadedState: IntegrationStates = {
  integrations: null,
  isFetching: false
};

export const fetchIntegrationsByParent = createAsyncThunk<IGetIntegrationsDTO, string>(
  'integrations/GetByParent',
  async (parent_id: string, { getState }) => {
    let body = JSON.stringify(parent_id);

    var fetched = await DoFetch(ApiIntegrationsRootString + "/GetByParent?parent_id=" + parent_id,
      {
        method: "GET"
      });

    var json = await fetched.json() as Promise<IGetIntegrationsDTO>;

    return json;
  }
)

const integrationsSlice = createSlice({
  name: 'IntegrationsStates',
  initialState: unloadedState,
  reducers: {

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
      //Next
  },
})

export const reducer = integrationsSlice.reducer



