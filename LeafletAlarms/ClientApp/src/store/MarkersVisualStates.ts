import { ApiStatesRootString, ApplicationState } from './index';
import { DoFetch } from "./Fetcher";
import { createAsyncThunk, createSlice, PayloadAction } from '@reduxjs/toolkit'
import { AlarmObject, MarkerVisualStateDTO } from "./Marker";


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

export const requestMarkersVisualStates = createAsyncThunk <MarkerVisualStateDTO, string[]>(
  'states/GetVisualStates',
  async (ids: string[], { getState }) => {
    const state: ApplicationState = getState() as ApplicationState;

    let body = JSON.stringify(ids);
    var request = ApiStatesRootString + "/GetVisualStates";

    var fetched = await DoFetch(request, {
      method: "POST",
      headers: { "Content-type": "application/json" },
      body: body
    });

    var json = await fetched.json() as Promise<MarkerVisualStateDTO>;  

    return json;
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
    },
    updateMarkersVisualStates(state: MarkersVisualStates, action: PayloadAction<MarkerVisualStateDTO>) {
      var newStates = state.visualStates.states
        .filter(item => null == action.payload.states.find(i => i.id == item.id));

      newStates = [...newStates, ...action.payload.states];

      var newDescrs = state.visualStates.states_descr
        .filter(item => null == action.payload.states_descr.find(i => i.id == item.id));
      newDescrs = [...newDescrs, ...action.payload.states_descr];

      state.visualStates.states = newStates;
      state.visualStates.states_descr = newDescrs;      
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
  },
})

export const { updateMarkersAlarmStates, updateMarkersVisualStates } = stateSlice.actions
export const reducer = stateSlice.reducer
