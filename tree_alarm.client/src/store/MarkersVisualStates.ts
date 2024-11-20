import { ApplicationState } from './index';
import { DoFetch } from "./Fetcher";
import { createAsyncThunk, createSlice, PayloadAction } from '@reduxjs/toolkit'
import { AlarmObject, MarkerVisualStateDTO } from "./Marker";
import { ApiStatesRootString } from './constants';


export interface MarkersVisualStates {
  visualStates: MarkerVisualStateDTO;
}

const unloadedState: MarkersVisualStates = {
  visualStates: {
    states: [],
    states_descr: [],
    alarmed_objects: []
  }
};

async function UpdateVisualStates(ids: string[]): Promise<MarkerVisualStateDTO> {
  const body = JSON.stringify(ids);
  const request = ApiStatesRootString + "/GetVisualStates";

  const fetched = await DoFetch(request, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: body
  });

  if (!fetched.ok) {
    throw new Error(`Failed to fetch visual states: ${fetched.statusText}`);
  }

  const json = (await fetched.json()) as MarkerVisualStateDTO;

  return json;
}


export const requestMarkersVisualStates = createAsyncThunk <MarkerVisualStateDTO, string[]>(
  'states/GetVisualStates',
  async (ids: string[], { getState }) => {
    return UpdateVisualStates(ids);
  }
)

export const requestAndUpdateMarkersVisualStates = createAsyncThunk<MarkerVisualStateDTO, string[]>(
  'states/GetAndUpdateVisualStates',
  async (ids: string[], { getState }) => {
    return UpdateVisualStates(ids);
  }
)

const stateSlice = createSlice({
  name: 'StateStates',
  initialState: unloadedState,
  reducers: {
    updateMarkersAlarmStates(state: MarkersVisualStates, action: PayloadAction<AlarmObject[]>) {
      var newAlarms = state.visualStates.alarmed_objects.filter(a => action.payload.find(d => d.id == a.id) == null);
      newAlarms = [... newAlarms, ...action.payload];
      state.visualStates.alarmed_objects = newAlarms;
    }
  }
  ,
  extraReducers: (builder) => {
    builder
      .addCase(requestMarkersVisualStates.pending, (state, action) => {
        //state.parents = null;
      })
      .addCase(requestMarkersVisualStates.fulfilled, (state, action) => {
        const { requestId } = action.meta
        state.visualStates = action.payload;
      })
      .addCase(requestMarkersVisualStates.rejected, (state, action) => {
        const { requestId } = action.meta
      })
    ////Update Visual States
      .addCase(requestAndUpdateMarkersVisualStates.fulfilled, (state, action) => {
        var newStates = state.visualStates.states
          .filter(item => null == action.payload.states.find(i => i.id == item.id));

        newStates = [...newStates, ...action.payload.states];

        var newDescrs = state.visualStates.states_descr
          .filter(item => null == action.payload.states_descr.find(i => i.id == item.id));
        newDescrs = [...newDescrs, ...action.payload.states_descr];

        state.visualStates.states = newStates;
        state.visualStates.states_descr = newDescrs; 
      })
  },
})

export const { updateMarkersAlarmStates } = stateSlice.actions
export const reducer = stateSlice.reducer
